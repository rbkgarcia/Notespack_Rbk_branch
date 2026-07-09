using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<EventContext>(options => options.UseSqlite("Data Source=notespack.db"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.Cookie.HttpOnly = true; options.LoginPath = "/login"; });
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<EventService>();

var app = builder.Build();

// Inicialización BD
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IDbContextFactory<EventContext>>().CreateDbContext().Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de Login (POST)
app.MapPost("/Account/Login", async (HttpContext context, AuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var user = await authService.LoginAsync(form["email"], form["password"]);
    
    if (user != null)
    {
        var claims = new List<Claim> { 
            new Claim(ClaimTypes.Name, user.Email), 
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString()) };
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
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