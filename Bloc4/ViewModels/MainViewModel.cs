using System.Threading.Tasks;
using System.Windows.Input;
using Bloc4.Infrastructure;
using Bloc4.Services;

namespace Bloc4.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;
        private readonly ILoginAuditService _audit;

        public SearchViewModel Search { get; }
        public AdminViewModel  Admin  { get; }
        public AuditViewModel  Audit  { get; }   // utilisé par l’overlay (Connexions admin)

        private bool _isAdminPanelVisible;
        public bool IsAdminPanelVisible
        {
            get => _isAdminPanelVisible;
            set => SetProperty(ref _isAdminPanelVisible, value);
        }

        private string _adminPassword = string.Empty;
        public string AdminPassword
        {
            get => _adminPassword;
            set => SetProperty(ref _adminPassword, value);
        }

        public ICommand ShowAdminPanelCommand { get; }
        public ICommand HideAdminPanelCommand { get; }
        public ICommand AuthenticateAdminCommand { get; }

        public MainViewModel(
            SearchViewModel search,
            AdminViewModel admin,
            IAuthService auth,
            ILoggingService log,
            ILoginAuditService audit)
        {
            Search = search;
            Admin  = admin;
            _auth  = auth;
            _log   = log;
            _audit = audit;

            // VM des logs (pour la grille "Connexions admin")
            Audit = new AuditViewModel(_audit, _log);

            ShowAdminPanelCommand = new RelayCommand(_ => IsAdminPanelVisible = true);

            HideAdminPanelCommand = new RelayCommand(_ =>
            {
                IsAdminPanelVisible = false;
                Admin.IsAuthorized  = false;
                AdminPassword       = string.Empty;
            });

            AuthenticateAdminCommand = new RelayCommand(async _ => await AuthenticateAsync());
        }

        private async Task AuthenticateAsync()
        {
            var pwd = (AdminPassword ?? string.Empty).Trim();

            bool ok = false;
            string? reason = null;

            try
            {
                ok = await _auth.AuthenticateAdminAsync(pwd);
                if (!ok) reason = "Bad password";
            }
            finally
            {
                // ⬅️ Logue CHAQUE tentative (succès/échec)
                await _audit.LogAdminAttemptAsync(ok, reason);
            }

            if (ok)
            {
                await _log.LogInfoAsync("Accès administrateur accordé");
                Admin.IsAuthorized = true;
                IsAdminPanelVisible = true;
                AdminPassword = string.Empty;

                await Admin.LoadAllAsync(); // recharge Sites/Services/Salariés
                await Audit.LoadAsync();    // recharge la grille des logs
            }
            else
            {
                await _log.LogErrorAsync("Accès administrateur refusé");
                Admin.IsAuthorized = false;
            }
        }
    }
}