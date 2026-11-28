
using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.Categories
{
    public class DetailsModel : PageModel
    {
        private readonly EventDiscoveryService _eventService;

        public EventCategory Category { get; private set; } = default!;

        public List<ExternalEvent> Events { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public DetailsModel(EventDiscoveryService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var category = CategoryCatalog.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            Category = category;

            try
            {
                var events = await _eventService.GetEventsAsync();

                Events = events
                    .Where(e =>
                        string.Equals(
                            e.Category,
                            Category.Id,
                            StringComparison.OrdinalIgnoreCase)
                        && e.StartsAt >= DateTimeOffset.Now)
                    .OrderBy(e => e.StartsAt)
                    .ToList();
            }
            catch (Exception)
            {
                ErrorMessage =
                    "Unable to load events. Please try again later.";
            }

            return Page();
        }
    }
}
