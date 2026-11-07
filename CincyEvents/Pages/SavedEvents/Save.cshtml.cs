using CincyEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.SavedEvents;

[Authorize]
[AutoValidateAntiforgeryToken]
public sealed class SaveModel(SavedEventsService savedEventsService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(
        int eventId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var result = await savedEventsService.SaveAsync(User, eventId, cancellationToken);
        StatusMessage = result switch
        {
            SaveEventResult.Saved => "Event Saved.",
            SaveEventResult.AlreadySaved => "This event is already saved.",
            SaveEventResult.EventNotFound => "That event could not be found.",
            SaveEventResult.EventUnavailable => "Only upcoming events can be saved.",
            SaveEventResult.InvalidEventId => "A valid event is required.",
            _ => throw new InvalidOperationException($"Unexpected save result: {result}.")
        };

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl!)
            : RedirectToPage("/Events/Index");
    }
}
