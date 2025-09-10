using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
    }
}
