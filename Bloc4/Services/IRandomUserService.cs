using System.Threading.Tasks;

namespace Bloc4.Services
{
    public interface IRandomUserService
    {
        Task<int> ImportRandomUsersAsync(int count = 10, string nat = "fr");
    }
}