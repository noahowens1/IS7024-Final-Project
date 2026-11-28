using System.Security.Claims;
using CincyEvents.Data;
using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Pages.Discover
{
    public class IndexModel : PageModel
    {
        private readonly EventDiscoveryService _eventService;
        private readonly ApplicationDbContext _db;
        private const decimal MemorialHallServiceCharge = 4m;

        public List<ExternalEvent> Events { get; set; } = new();
        public HashSet<string> ConnectedEventKeys { get; private set; } = [];
        public Dictionary<string, int> ConnectedEventIds { get; private set; } = [];
        [TempData]
        public string? ErrorMessage { get; set; }
        public IReadOnlyList<EventCategory> Categories => CategoryCatalog.All;

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

        public IndexModel(EventDiscoveryService eventService, ApplicationDbContext db)
        {
            _eventService = eventService;
            _db = db;
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

                if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
                {
                    ErrorMessage = "Start date cannot be after end date.";
                    return;
                }

                Events = await _eventService.GetEventsAsync();
                await LoadConnectedEventsAsync();
                Events = Events.Where(e => e.StartsAt >= DateTimeOffset.Now).OrderBy(e => e.StartsAt).ToList();

                if (StartDate.HasValue)
                    Events = Events.Where(e => e.StartsAt.Date >= StartDate.Value.Date).ToList();
                if (EndDate.HasValue)
                    Events = Events.Where(e => e.StartsAt.Date <= EndDate.Value.Date).ToList();
                if (!string.IsNullOrWhiteSpace(Location))
                {
                    var search = Location.Trim();
                    Events = Events.Where(e => e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || e.Address.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(Category))
                    Events = Events.Where(e => string.Equals(e.Category, Category, StringComparison.OrdinalIgnoreCase)).ToList();
                if (MaxBudget.HasValue)
                {
                    Events = Events.Where(e =>
                    {
                        var totalPrice = GetMinimumTotalPrice(e);
                        return totalPrice.HasValue && totalPrice.Value <= MaxBudget.Value;
                    }).ToList();
                }
                if (FreeOnly)
                    Events = Events.Where(e => e.Price == 0 && (!e.MaxPrice.HasValue || e.MaxPrice.Value == 0)).ToList();
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to load events. Please try again later.";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostConnectAsync(string source, string externalId)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(externalId))
            {
                ErrorMessage = "The selected event could not be connected.";
                return RedirectToPage();
            }

            var externalEvent = (await _eventService.GetEventsAsync()).SingleOrDefault(item =>
                string.Equals(item.Source, source, StringComparison.Ordinal)
                && string.Equals(item.ExternalId, externalId, StringComparison.Ordinal));

            if (externalEvent is null)
            {
                ErrorMessage = "That event is no longer available. Please refresh and try again.";
                return RedirectToPage();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var eventItem = await _db.Events.SingleOrDefaultAsync(item =>
                item.Source == externalEvent.Source && item.ExternalId == externalEvent.ExternalId);

            if (eventItem is null)
            {
                eventItem = new Event
                {
                    Source = externalEvent.Source,
                    ExternalId = externalEvent.ExternalId,
                    Name = externalEvent.Name,
                    Description = externalEvent.Description,
                    StartsAt = externalEvent.StartsAt.UtcDateTime
                };
                _db.Events.Add(eventItem);
            }

            var isParticipant = eventItem.Id != 0 && await _db.EventParticipants.AnyAsync(item =>
                item.EventId == eventItem.Id && item.UserId == userId);
            if (!isParticipant)
                eventItem.Participants.Add(new EventParticipant { UserId = userId });

            await _db.SaveChangesAsync();
            return RedirectToPage("/Chat/Index", new { eventId = eventItem.Id });
        }

        public string EventKey(string source, string externalId) => $"{source}\u001f{externalId}";

        private async Task LoadConnectedEventsAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ConnectedEventIds = (await _db.EventParticipants
                .Where(participant => participant.UserId == userId
                    && participant.Event.ExternalId != null
                    && participant.Event.Source != null)
                .Select(participant => new { participant.EventId, participant.Event.Source, participant.Event.ExternalId })
                .ToListAsync())
                .ToDictionary(item => EventKey(item.Source!, item.ExternalId!), item => item.EventId, StringComparer.Ordinal);
            ConnectedEventKeys = ConnectedEventIds.Keys.ToHashSet(StringComparer.Ordinal);
        }

        private static decimal? GetMinimumTotalPrice(ExternalEvent item)
        {
            if (!item.Price.HasValue)
                return null;

            decimal totalPrice = item.Price.Value;
            if (string.Equals(item.Source, "Memorial Hall", StringComparison.OrdinalIgnoreCase) && totalPrice > 0)
                totalPrice += MemorialHallServiceCharge;
            return totalPrice;
        }
    }
}
