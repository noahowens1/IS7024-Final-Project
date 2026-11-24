using Microsoft.AspNetCore.Identity;

namespace CincyEvents.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public ICollection<EventParticipant> EventParticipations { get; set; } = [];
}
