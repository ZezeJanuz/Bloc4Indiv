using System.Threading.Tasks;

namespace Bloc4.Services
{
    public interface IAuthService
    {
        Task<bool> AuthenticateAdminAsync(string password);
    }

    public class SimpleAuthService : IAuthService
    {
        private readonly string _adminPassword;

        public SimpleAuthService(string adminPassword = "admin")
        {
            _adminPassword = adminPassword;
        }

        public Task<bool> AuthenticateAdminAsync(string password)
            => Task.FromResult(password == _adminPassword);
    }
}