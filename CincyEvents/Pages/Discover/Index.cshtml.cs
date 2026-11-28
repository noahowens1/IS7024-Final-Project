
using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.Discover
{
    public class IndexModel : PageModel
    {
        private readonly EventDiscoveryService _eventService;

        private const decimal MemorialHallServiceCharge = 4m;

        public List<ExternalEvent> Events { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IReadOnlyList<EventCategory> Categories =>
            CategoryCatalog.All;

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxBudget { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool FreeOnly { get; set; }

        public IndexModel(EventDiscoveryService eventService)
        {
            _eventService = eventService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Please enter valid filter values.";
                    return;
                }

                if (MaxBudget.HasValue && MaxBudget.Value < 0)
                {
                    ErrorMessage = "Budget cannot be negative.";
                    return;
                }

                if (StartDate.HasValue
                    && EndDate.HasValue
                    && StartDate.Value.Date > EndDate.Value.Date)
                {
                    ErrorMessage =
                        "Start date cannot be after end date.";

                    return;
                }

                Events = await _eventService.GetEventsAsync();

                Events = Events
                    .Where(e => e.StartsAt >= DateTimeOffset.Now)
                    .OrderBy(e => e.StartsAt)
                    .ToList();

                if (StartDate.HasValue)
                {
                    Events = Events
                        .Where(e => e.StartsAt.Date >= StartDate.Value.Date)
                        .ToList();
                }

                if (EndDate.HasValue)
                {
                    Events = Events
                        .Where(e => e.StartsAt.Date <= EndDate.Value.Date)
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(Location))
                {
                    var search = Location.Trim();

                    Events = Events
                        .Where(e =>
                            e.Location.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)
                            || e.Address.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(Category))
                {
                    Events = Events
                        .Where(e => string.Equals(
                            e.Category,
                            Category,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (MaxBudget.HasValue)
                {
                    Events = Events
                        .Where(e =>
                        {
                            var totalPrice = GetMinimumTotalPrice(e);

                            return totalPrice.HasValue
                                && totalPrice.Value <= MaxBudget.Value;
                        })
                        .ToList();
                }

                if (FreeOnly)
                {
                    Events = Events
                        .Where(e => e.Price == 0
                            && (!e.MaxPrice.HasValue
                                || e.MaxPrice.Value == 0))
                        .ToList();
                }
            }
            catch (Exception)
            {
                ErrorMessage =
                    "Unable to load events. Please try again later.";
            }
        }

        private static decimal? GetMinimumTotalPrice(
            ExternalEvent item)
        {
            if (!item.Price.HasValue)
            {
                return null;
            }

            decimal totalPrice = item.Price.Value;

            if (string.Equals(
                    item.Source,
                    "Memorial Hall",
                    StringComparison.OrdinalIgnoreCase)
                && totalPrice > 0)
            {
                totalPrice += MemorialHallServiceCharge;
            }

            return totalPrice;
        }
    }
}
