using System.Threading.Tasks;

namespace Bloc4.Services
{
    /// <summary>
    /// Contrat d'authentification admin.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valide le mot de passe administrateur.
        /// </summary>
        Task<bool> AuthenticateAdminAsync(string password);
    }
}