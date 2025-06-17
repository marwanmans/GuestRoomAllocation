
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuestRoomAllocation.Web.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // If user is already authenticated, redirect to admin dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Admin/Index");
            }

            return Page();
        }
    }
}