using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;
using BCrypt.Net; 

namespace NOTESPACK.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<EventContext> _contextFactory;

        public AuthService(IDbContextFactory<EventContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
    public async Task<bool> RegisterAsync(string email, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        if (await context.Users.AnyAsync(u => u.Email == email)) return false;

        var newUser = new User { 
            Email = email, 
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "User" 
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync(); // Aquí EF Core actualiza newUser.Id automáticamente
        return true;
    }
        public async Task<User?> LoginAsync(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            // VERIFICACIÓN: Comparamos el hash almacenado con la contraseña ingresada
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password)) 
            {
                return user;
            }
            return null; 
        }
    }
}