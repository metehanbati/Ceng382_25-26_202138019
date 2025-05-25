using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LabProject2.Middleware 
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kimlik doğrulamanın atlanacağı yollar (login, logout, statik dosyalar vb.)
            string path = context.Request.Path.Value?.ToLower() ?? "";
            
            if (path.Contains("/login") || 
                path.Contains("/logout") || 
                path.Contains("/css") || 
                path.Contains("/js") || 
                path.Contains("/lib") ||
                path.Contains("/images") ||
                path.Contains("/favicon") ||
                path == "/")
            {
                await _next(context);
                return;
            }

            // Oturum ve çerezlerden kullanıcı bilgilerini al
            string? sessionUsername = context.Session.GetString("username");
            string? sessionToken = context.Session.GetString("token");
            string? sessionId = context.Session.GetString("session_id");

            string? cookieUsername = context.Request.Cookies["username"];
            string? cookieToken = context.Request.Cookies["token"];
            string? cookieSessionId = context.Request.Cookies["session_id"];

            // Kullanıcının oturum açmış olup olmadığını kontrol et
            bool isAuthenticated = !string.IsNullOrEmpty(sessionUsername) && 
                                    !string.IsNullOrEmpty(sessionToken) && 
                                    !string.IsNullOrEmpty(sessionId) &&
                                    sessionUsername == cookieUsername && 
                                    sessionToken == cookieToken && 
                                    sessionId == cookieSessionId;

            if (!isAuthenticated)
            {
                // Oturum açmamışsa login sayfasına yönlendir
                context.Response.Redirect("/Login");
                return;
            }

            // Kullanıcı oturum açmış, isteği bir sonraki middleware'e ilet
            await _next(context);
        }
    }
}