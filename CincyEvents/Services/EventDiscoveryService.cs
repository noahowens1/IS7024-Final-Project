
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using CincyEvents.Models;

namespace CincyEvents.Services
{
    public class EventDiscoveryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EventDiscoveryService> _logger;
        private readonly IMemoryCache _cache;

        private static readonly SemaphoreSlim CacheLock = new(1, 1);

        private const string CacheKey = "CincinnatiExternalEvents";

        private const string WashingtonParkUrl =
            "https://washingtonpark.org/wp-json/events-widgets-cs/v1/events";

        private const string FountainSquareUrl =
            "https://myfountainsquare.com/wp-json/events-widgets-cs/v1/events";

        private const string MemorialHallUrl =
            "https://www.memorialhallotr.com/wp-json/wp/v2/whatson";

        public EventDiscoveryService(
            HttpClient httpClient,
            ILogger<EventDiscoveryService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<ExternalEvent>> GetEventsAsync()
        {
            if (_cache.TryGetValue(
                CacheKey, out List<ExternalEvent>? cachedEvents)
                && cachedEvents != null)
            {
                return cachedEvents.ToList();
            }

            await CacheLock.WaitAsync();

            try
            {
                if (_cache.TryGetValue(
                    CacheKey, out cachedEvents)
                    && cachedEvents != null)
                {
                    return cachedEvents.ToList();
                }

                var results = await Task.WhenAll(
                    GetSourceEventsAsync(
                        WashingtonParkUrl, "Washington Park"),
                    GetSourceEventsAsync(
                        FountainSquareUrl, "Fountain Square"),
                    GetMemorialHallEventsAsync()
                );

                var events = results
                    .SelectMany(items => items)
                    .GroupBy(e => new { e.Source, e.ExternalId })
                    .Select(group => group.First())
                    .OrderBy(e => e.StartsAt)
                    .ToList();

                if (events.Count > 0)
                {
                    var duration = results.Any(r => r.Count == 0)
                        ? TimeSpan.FromMinutes(5)
                        : TimeSpan.FromMinutes(30);

                    _cache.Set(CacheKey, events, duration);
                }

                return events.ToList();
            }
            finally
            {
                CacheLock.Release();
            }
        }

        private async Task<List<ExternalEvent>> GetSourceEventsAsync(
            string apiUrl, string source)
        {
            var events = new List<ExternalEvent>();

            try
            {
                var json = await _httpClient.GetStringAsync(apiUrl);

                using var document = JsonDocument.Parse(json);

                foreach (var item in document.RootElement.EnumerateArray())
                {
                    var title = WebUtility.HtmlDecode(
                        GetString(item, "title"));

                    var description = WebUtility.HtmlDecode(
                        GetString(item, "description_text"));

                    if (title.Equals("Porch is Open",
                        StringComparison.OrdinalIgnoreCase)
                        || title.Equals("Winterhaus is Open",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!item.TryGetProperty("start", out var startDate)
                        || startDate.ValueKind != JsonValueKind.String
                        || !startDate.TryGetDateTimeOffset(out var parsedStart))
                    {
                        continue;
                    }

                    var parsedEnd = parsedStart;

                    if (item.TryGetProperty("end", out var endDate)
                        && endDate.ValueKind == JsonValueKind.String
                        && endDate.TryGetDateTimeOffset(out var validEnd))
                    {
                        parsedEnd = validEnd;
                    }

                    item.TryGetProperty("venue", out var venue);

                    string imageUrl = string.Empty;

                    if (item.TryGetProperty("image", out var image)
                        && image.ValueKind == JsonValueKind.Object
                        && image.TryGetProperty("large", out var large))
                    {
                        imageUrl = GetString(large, "url");
                    }

                    var location = GetString(venue, "name");

                    if (string.IsNullOrWhiteSpace(location))
                    {
                        location = source;
                    }

                    var externalId = GetString(item, "instance_id");

                    if (string.IsNullOrWhiteSpace(externalId))
                    {
                        externalId = GetString(item, "permalink")
                            + "|" + parsedStart.ToString("O");
                    }

                    events.Add(new ExternalEvent
                    {
                        ExternalId = externalId,
                        Name = title,
                        Description = description,
                        StartsAt = parsedStart,
                        EndsAt = parsedEnd,
                        Location = location,
                        Address = GetString(venue, "address"),
                        EventUrl = GetString(item, "permalink"),
                        ImageUrl = imageUrl,
                        Category = DetermineCategory(title),
                        Price = DeterminePrice(description),
                        Source = source
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Unable to load events from {Source}", source);

                events.Clear();
            }

            return events;
        }

        private async Task<List<ExternalEvent>> GetMemorialHallEventsAsync()
        {
            var events = new List<ExternalEvent>();

            try
            {
                var json = await _httpClient.GetStringAsync(
                    MemorialHallUrl + "?per_page=100");

                using var document = JsonDocument.Parse(json);

                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return events;
                }

                using var limiter = new SemaphoreSlim(3);

                var tasks = document.RootElement
                    .EnumerateArray()
                    .Select(async item =>
                    {
                        await limiter.WaitAsync();

                        try
                        {
                            return await GetMemorialHallDetailsAsync(item);
                        }
                        finally
                        {
                            limiter.Release();
                        }
                    })
                    .ToArray();

                var results = await Task.WhenAll(tasks);

                events.AddRange(results.SelectMany(result => result));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Unable to load Memorial Hall events");
            }

            return events;
        }

        private async Task<List<ExternalEvent>> GetMemorialHallDetailsAsync(
            JsonElement item)
        {
            var events = new List<ExternalEvent>();

            try
            {
                if (!item.TryGetProperty("id", out var idElement)
                    || !idElement.TryGetInt32(out var id))
                {
                    return events;
                }

                item.TryGetProperty("title", out var titleElement);
                item.TryGetProperty("content", out var contentElement);

                var title = CleanHtml(
                    GetString(titleElement, "rendered"));

                var description = CleanHtml(
                    GetString(contentElement, "rendered"));

                var eventUrl = GetString(item, "link");

                if (title.Contains("wait list",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return events;
                }

                var json = await _httpClient.GetStringAsync(
                    MemorialHallUrl + "/" + id);

                using var document = JsonDocument.Parse(json);

                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty("", out var details)
                    || details.ValueKind != JsonValueKind.Object)
                {
                    return events;
                }

                var imageUrl = GetString(details, "featuredimage");

                if (!details.TryGetProperty("metas", out var metas)
                    || metas.ValueKind != JsonValueKind.Array)
                {
                    return events;
                }

                foreach (var group in metas.EnumerateArray())
                {
                    if (group.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var performance in group.EnumerateArray())
                    {
                        var startText = GetString(
                            performance, "whatson_t_start_date");

                        var endText = GetString(
                            performance, "whatson_t_end_date");

                        var parsedStart = ParseCincinnatiDate(startText);

                        if (!parsedStart.HasValue
                            || parsedStart.Value < DateTimeOffset.Now)
                        {
                            continue;
                        }

                        var parsedEnd =
                            ParseCincinnatiDate(endText)
                            ?? parsedStart.Value;

                        var priceText = GetString(
                            performance, "whatson_t_ticket_info");

                        var prices = ParseTicketPrices(priceText);

                        var instanceId = GetString(
                            performance, "whatson_t_book_url");

                        events.Add(new ExternalEvent
                        {
                            ExternalId = id + "|" + instanceId
                                + "|" + parsedStart.Value.ToString("O"),
                            Name = title,
                            Description = description,
                            StartsAt = parsedStart.Value,
                            EndsAt = parsedEnd,
                            Location = "Memorial Hall",
                            Address = "1225 Elm Street, Cincinnati, OH",
                            EventUrl = eventUrl,
                            ImageUrl = imageUrl,
                            Category = DetermineMemorialHallCategory(
                                item, title),
                            Price = prices.Min,
                            MaxPrice = prices.Max,
                            Source = "Memorial Hall"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Unable to load a Memorial Hall event");
            }

            return events;
        }

        private static DateTimeOffset? ParseCincinnatiDate(string value)
        {
            if (!DateTime.TryParseExact(
                value,
                "yyyy-MM-ddTHH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return null;
            }

            TimeZoneInfo eastern;

            try
            {
                eastern = TimeZoneInfo.FindSystemTimeZoneById(
                    "Eastern Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                eastern = TimeZoneInfo.FindSystemTimeZoneById(
                    "America/New_York");
            }

            date = DateTime.SpecifyKind(
                date, DateTimeKind.Unspecified);

            if (eastern.IsInvalidTime(date))
            {
                return null;
            }

            return new DateTimeOffset(
                date, eastern.GetUtcOffset(date));
        }

        private static (decimal? Min, decimal? Max)
            ParseTicketPrices(string text)
        {
            text = WebUtility.HtmlDecode(text).Trim();

            if (text.Equals("Free", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Free Admission",
                    StringComparison.OrdinalIgnoreCase))
            {
                return (0m, 0m);
            }

            var matches = Regex.Matches(
                text, @"\$\s*(\d+(?:,\d{3})*(?:\.\d{1,2})?)");

            var prices = new List<decimal>();

            foreach (Match match in matches)
            {
                if (decimal.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var price))
                {
                    prices.Add(price);
                }
            }

            if (prices.Count == 0)
            {
                return (null, null);
            }

            return (prices.Min(), prices.Max());
        }

        private static string DetermineMemorialHallCategory(
            JsonElement item, string title)
        {
            if (item.TryGetProperty("class_list", out var classes)
                && classes.ValueKind == JsonValueKind.Array)
            {
                var categories = string.Join(" ",
                    classes.EnumerateArray()
                        .Where(c => c.ValueKind == JsonValueKind.String)
                        .Select(c => c.GetString()))
                    .ToLowerInvariant();

                if (categories.Contains("whatson_categories-rock")
                    || categories.Contains("whatson_categories-jazz")
                    || categories.Contains("whatson_categories-blues")
                    || categories.Contains("whatson_categories-folk")
                    || categories.Contains("whatson_categories-celtic")
                    || categories.Contains("whatson_categories-classical")
                    || categories.Contains("whatson_categories-americana")
                    || categories.Contains("whatson_categories-rb"))
                {
                    return "music-concerts";
                }

                if (categories.Contains("whatson_categories-comedy")
                    || categories.Contains("whatson_categories-film")
                    || categories.Contains("whatson_categories-theater")
                    || categories.Contains("whatson_categories-dance"))
                {
                    return "arts-culture";
                }

                if (categories.Contains("whatson_categories-family"))
                {
                    return "festivals-community";
                }
            }

            return DetermineCategory(title);
        }

        private static string CleanHtml(string value)
        {
            var text = Regex.Replace(value, "<[^>]*>", " ");

            text = WebUtility.HtmlDecode(text);

            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private static decimal? DeterminePrice(string description)
        {
            var text = description.ToLowerInvariant();

            if (text.Contains("free admission")
                || text.Contains("free to attend")
                || text.Contains("free and open to the public")
                || text.Contains("this event is free")
                || text.Contains("free event")
                || text.Contains("sunday soundbaths are free"))
            {
                return 0m;
            }

            return null;
        }

        private static string DetermineCategory(string title)
        {
            var text = title.ToLowerInvariant();

            if (text.Contains("soccer")
                || text.Contains("football")
                || text.Contains("baseball")
                || text.Contains("prematch")
                || text.Contains("fc cincinnati")
                || text.Contains("bengals")
                || text.Contains("fanfest")
                || text.Contains("tailgate")
                || text.Contains("fitness")
                || text.Contains("soundbath")
                || text.Contains("skating")
                || text.Contains("yoga"))
            {
                return "sports";
            }

            if (text.Contains("market")
                || text.Contains("flea")
                || text.Contains("santa")
                || text.Contains("community")
                || text.Contains("holiday")
                || text.Contains("light up the square")
                || text.Contains("festival"))
            {
                return "festivals-community";
            }

            if (text.Contains("concert")
                || text.Contains("live music")
                || text.Contains("jazz")
                || text.Contains("orchestra")
                || text.Contains("dj "))
            {
                return "music-concerts";
            }

            if (text.Contains("paint")
                || text.Contains("film")
                || text.Contains("flicks")
                || text.Contains("movie")
                || text.Contains("theater")
                || text.Contains("art ")
                || text.Contains("exhibit"))
            {
                return "arts-culture";
            }

            if (text.Contains("food")
                || text.Contains("dining")
                || text.Contains("tasting")
                || text.Contains("culinary"))
            {
                return "food-dining";
            }

            return "uncategorized";
        }

        private static string GetString(
            JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
