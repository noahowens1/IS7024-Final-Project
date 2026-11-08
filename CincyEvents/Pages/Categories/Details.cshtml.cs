using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CincyEvents.Pages.Categories
{
    public class DetailsModel : PageModel
    {
        public EventCategory? Category { get; private set; }

        public IActionResult OnGet(string? id)
        {
            Category = CategoryCatalog.GetById(id);

            if (Category is null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}