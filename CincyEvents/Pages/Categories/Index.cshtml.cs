using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.Categories
{
    public class IndexModel : PageModel
    {
        public IReadOnlyList<EventCategory> Categories => CategoryCatalog.All;
    }
}