using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, ILogger<AuthService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("Login failed for non-existent user {username}", username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for {username}", username);
                return null;
            }

            _logger.LogInformation("User {username} authenticated as {role}", username, user.Role);
            return user;
        }
    }
}
