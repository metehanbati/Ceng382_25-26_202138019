using LabProject2.Middleware; // Middleware uzantılarınızın olduğu namespace
using System.Text.Json; 
using System.IO; 
using System.Collections.Generic; 
using System; 
using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting; 
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Session hizmetlerini ekleyin
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum zaman aşımı
    options.Cookie.HttpOnly = true; // JavaScript erişimini engeller
    options.Cookie.IsEssential = true; // GDPR uyumluluğu için
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTP/HTTPS için otomatik ayar
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF koruması
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure data directory and users.json exist
EnsureDataDirectoryExists(app.Environment.WebRootPath);

app.UseRouting();

// Session middleware'i kimlik doğrulama middleware'inden ÖNCE gelmeli
app.UseSession(); 

// Özel kimlik doğrulama middleware'i kullanın
app.UseCustomAuthentication(); 

app.UseAuthorization(); // Eğer kimlik doğrulama kullanıyorsanız

app.MapRazorPages();

app.Run();

// Helper method to ensure users.json exists
static void EnsureDataDirectoryExists(string webRootPath)
{
    var dataDirectory = Path.Combine(webRootPath, "data");
    if (!Directory.Exists(dataDirectory))
    {
        Directory.CreateDirectory(dataDirectory);
    }
    
    var usersFilePath = Path.Combine(dataDirectory, "users.json");
    if (!File.Exists(usersFilePath))
    {
        var defaultUsers = new List<object>
        {
            new
            {
                Username = "admin",
                Password = "admin123",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new
            {
                Username = "user",
                Password = "user123",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new
            {
                Username = "manager",
                Password = "manager123",
                Role = "Manager",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(defaultUsers, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(usersFilePath, json);
    }
}