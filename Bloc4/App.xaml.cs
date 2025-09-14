using System.Net.Http;
using System.Windows;
using Bloc4.Data;
using Bloc4.Services;
using Bloc4.ViewModels;

namespace Bloc4
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- Composition root (DI très simple) ---
            var db = new AppDbContext();
            await DbInitializer.EnsureCreatedAsync(db);

            var logger = new FileLoggingService();
            var auth   = new SimpleAuthService("admin"); // à changer pour la démo
            var data   = new EfDataService(db);
            var pdf    = new PdfService();
            var http   = new HttpClient();
            var random = new RandomUserService(db, http);

            // Seed rapide si BDD quasi vide (API RandomUser)
            await random.ImportRandomUsersAsync(10, "fr");

            // --- ViewModels ---
            var searchVm = new SearchViewModel(data);
            await searchVm.LoadListsAsync();
            var adminVm  = new AdminViewModel(data, pdf);
            var mainVm   = new MainViewModel(searchVm, adminVm, auth, logger);

            // --- Window ---
            var win = new MainWindow { DataContext = mainVm };
            win.Show();
        }
    }
}