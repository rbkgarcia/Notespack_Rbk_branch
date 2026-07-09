using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;

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

            context.Users.Add(new User { Email = email, Password = password, Role = "User" });
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            // Solo validamos, no iniciamos sesión aquí
            if (user != null && user.Password == password) 
            {
                return user; // Retornamos el objeto usuario
            }
            return null;
        }
    }
}