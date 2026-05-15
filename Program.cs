using Microsoft.AspNetCore.Authentication.Cookies;
using formulario_olv.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURACIÓN DE SERVICIOS
// ==========================================

// Cliente HTTP para consumir la API
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://10.100.10.9:5101";
    var apiKey = builder.Configuration["ApiSettings:ApiKey"] ?? "";
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Permitir certificados HTTPS no válidos (comunicación interna entre servidores)
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Autenticación por cookies
var pathBase = builder.Configuration["PathBase"] ?? "/olv";
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Use paths relative to the app; UsePathBase() añadirá el PathBase al generar/redirigir
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(25);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = pathBase;
    });

// MVC
builder.Services.AddControllersWithViews();

// Antiforgery: asegurar que la cookie esté ligada al path base y evitar colisiones con cookies previas
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery.olv";
    options.Cookie.Path = pathBase;
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==========================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UsePathBase(pathBase);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();
