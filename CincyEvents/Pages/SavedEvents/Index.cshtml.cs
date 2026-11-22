using CincyEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.SavedEvents;

[Authorize]
[AutoValidateAntiforgeryToken]
public sealed class IndexModel(SavedEventsService savedEventsService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<SavedEventDetails> SavedEvents { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        SavedEvents = await savedEventsService.ListAsync(User, cancellationToken);
    }

    public async Task<IActionResult> OnPostRemoveAsync(
        int savedEventId,
        CancellationToken cancellationToken)
    {
        var result = await savedEventsService.RemoveAsync(User, savedEventId, cancellationToken);
        StatusMessage = result == RemoveSavedEventResult.Removed
            ? "Event removed from your saved list."
            : "That saved event could not be found.";

        return RedirectToPage();
    }
}
