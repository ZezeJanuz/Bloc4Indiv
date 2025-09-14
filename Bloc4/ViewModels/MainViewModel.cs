using System.Threading.Tasks;
using System.Windows.Controls;      // <-- important
using System.Windows.Input;
using Bloc4.Infrastructure;
using Bloc4.Services;

namespace Bloc4.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;

        public SearchViewModel Search { get; }
        public AdminViewModel Admin { get; }

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

        public MainViewModel(SearchViewModel search, AdminViewModel admin, IAuthService auth, ILoggingService log)
        {
            Search = search;
            Admin = admin;
            _auth = auth;
            _log  = log;

            ShowAdminPanelCommand = new RelayCommand(_ => IsAdminPanelVisible = true);
            HideAdminPanelCommand = new RelayCommand(_ =>
            {
                IsAdminPanelVisible = false;
                Admin.IsAuthorized  = false;
                AdminPassword       = string.Empty;
            });

            // ⬇️ On récupère le PasswordBox et on lit .Password
            AuthenticateAdminCommand = new RelayCommand(async p =>
            {
                var pwdBox = p as PasswordBox;
                AdminPassword = pwdBox?.Password ?? string.Empty;
                await AuthenticateAsync();
            });
        }

        private async Task AuthenticateAsync()
        {
            var ok = await _auth.AuthenticateAdminAsync(AdminPassword);
            if (ok)
            {
                await _log.LogInfoAsync("Accès administrateur accordé");
                Admin.IsAuthorized = true;
                IsAdminPanelVisible = true;
                AdminPassword = string.Empty;
                await Admin.LoadAllAsync(); // charge les données
            }
            else
            {
                await _log.LogErrorAsync("Accès administrateur refusé");
                Admin.IsAuthorized = false;
            }
        }
    }
}
