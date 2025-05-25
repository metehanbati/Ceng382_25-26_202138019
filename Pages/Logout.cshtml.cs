using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Logging; 

namespace LabProject2.Pages
{
    public class LogoutPageModel : PageModel
    {
        private readonly ILogger<LogoutPageModel> _logger;

        public LogoutPageModel(ILogger<LogoutPageModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            string? username = HttpContext.Session.GetString("username");
            
            // Clear session
            HttpContext.Session.Clear();

            // Clear all authentication cookies
            string[] cookiesToDelete = { "username", "token", "session_id", "role" };
            
            foreach (string cookieName in cookiesToDelete)
            {
                if (Request.Cookies.ContainsKey(cookieName))
                {
                    Response.Cookies.Delete(cookieName, new CookieOptions
                    {
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        HttpOnly = true
                    });
                }
            }

            _logger.LogInformation("User {Username} logged out successfully", username ?? "Unknown");
            
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToPage("/Login");
        }
    }
}