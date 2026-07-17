using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<EventContext>(options => options.UseSqlite("Data Source=notespack.db"));
builder.Services.AddHostedService<CampusEventSyncService>();
builder.Services.AddHttpClient();
// Configuración de Cookies (Autenticación nativa)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { 
        options.Cookie.HttpOnly = true; 
        options.LoginPath = "/login"; 
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState(); // Necesario para <AuthorizeView>
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>(provider => 
    (CustomAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

var app = builder.Build();

// Inicialización BD
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IDbContextFactory<EventContext>>().CreateDbContext().Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication(); // Activa la autenticación
app.UseAuthorization();  // Activa la autorización

// Endpoints de manejo de sesión (Backend)
app.MapPost("/Account/Login", async (HttpContext context, AuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString() ?? "";
    var password = form["password"].ToString() ?? "";
    
    var user = await authService.LoginAsync(email, password);
    
    if (user != null)
    {
        // Aseguramos que el Id se convierta a string de forma segura
        var userId = user.Id.ToString(); 

        var claims = new List<Claim> { 
            new Claim(ClaimTypes.Name, user.Email), 
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", userId) // Ahora pasamos el ID real
        };

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        
        context.Response.Redirect("/");
    }
    else { context.Response.Redirect("/login?error=true"); }
});

app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.MapRazorComponents<NOTESPACK.App>().AddInteractiveServerRenderMode();
app.Run();