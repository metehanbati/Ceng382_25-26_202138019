using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject2.Models;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabProject2.Pages
{
    public class LoginPageModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LoginPageModel> _logger;

        [BindProperty]
        public LoginViewModel LoginData { get; set; } = new LoginViewModel(); 

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginPageModel(IWebHostEnvironment environment, ILogger<LoginPageModel> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is already logged in
            if (IsUserLoggedIn())
            {
                return RedirectToPage("/Table"); // Week 7'deki table sayfanıza redirect
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string usersFilePath = Path.Combine(_environment.WebRootPath, "data", "users.json");
                
                // Check if the file exists
                if (!System.IO.File.Exists(usersFilePath))
                {
                    ErrorMessage = "User database not found. Please contact administrator.";
                    _logger.LogError("User database file not found at {FilePath}", usersFilePath);
                    return Page();
                }

                // Read users from the file
                string json = await System.IO.File.ReadAllTextAsync(usersFilePath);
                var users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();

                // Find the user
                var user = users.FirstOrDefault(u => 
                    u.Username.Equals(LoginData.Username, StringComparison.OrdinalIgnoreCase) && 
                    u.Password == LoginData.Password && 
                    u.IsActive);

                if (user == null)
                {
                    ErrorMessage = "Username or password is incorrect.";
                    _logger.LogWarning("Failed login attempt for username: {Username}", LoginData.Username);
                    return Page();
                }

                // Generate a simple token
                string token = GenerateToken();
                
                // Store values in session
                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("token", token);
                HttpContext.Session.SetString("session_id", HttpContext.Session.Id);
                HttpContext.Session.SetString("role", user.Role);

                // Store values in cookies
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(30),
                    HttpOnly = true,
                    Secure = Request.IsHttps, // HTTPS için true, HTTP için false
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("username", user.Username, cookieOptions);
                Response.Cookies.Append("token", token, cookieOptions);
                Response.Cookies.Append("session_id", HttpContext.Session.Id, cookieOptions);
                Response.Cookies.Append("role", user.Role, cookieOptions);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                // Redirect to table page (Week 7'deki ana sayfa)
                return RedirectToPage("/Table");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", LoginData.Username);
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        private bool IsUserLoggedIn()
        {
            string? sessionUsername = HttpContext.Session.GetString("username");
            string? sessionToken = HttpContext.Session.GetString("token");
            string? sessionId = HttpContext.Session.GetString("session_id");

            string? cookieUsername = Request.Cookies["username"];
            string? cookieToken = Request.Cookies["token"];
            string? cookieSessionId = Request.Cookies["session_id"];

            return !string.IsNullOrEmpty(sessionUsername) && 
                   !string.IsNullOrEmpty(sessionToken) && 
                   !string.IsNullOrEmpty(sessionId) &&
                   sessionUsername == cookieUsername && 
                   sessionToken == cookieToken && 
                   sessionId == cookieSessionId;
        }

        private string GenerateToken()
        {
            // Generate a random token
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}