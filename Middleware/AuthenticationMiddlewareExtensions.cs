using Microsoft.AspNetCore.Builder;
using LabProject2.Middleware; // AuthenticationMiddleware'in bulunduğu namespace

namespace LabProject2.Middleware 
{
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}