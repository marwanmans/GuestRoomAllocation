using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuestRoomAllocation.Web.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Add success message
            TempData["InfoMessage"] = "You have been successfully logged out.";

            // Redirect to login page
            return RedirectToPage("/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Add success message
            TempData["InfoMessage"] = "You have been successfully logged out.";

            // Redirect to login page
            return RedirectToPage("/Login");
        }
    }
}