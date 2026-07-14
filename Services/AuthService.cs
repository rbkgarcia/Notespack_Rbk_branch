using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;
using BCrypt.Net; 

namespace NOTESPACK.Services
{
    public enum AuthLoginStatus
    {
        Success,
        EmailNotFound,
        WrongPassword
    }

    public class AuthLoginResult
    {
        public AuthLoginStatus Status { get; set; }
        public User? User { get; set; }
    }

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

            var newUser = new User
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<AuthLoginResult> LoginAsync(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new AuthLoginResult { Status = AuthLoginStatus.EmailNotFound };
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new AuthLoginResult { Status = AuthLoginStatus.WrongPassword };
            }

            return new AuthLoginResult { Status = AuthLoginStatus.Success, User = user };
        }
    }
}