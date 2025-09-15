using System.Net.Http;
using System.Windows;
using Bloc4.Data;
using Bloc4.Services;          // ✅ indispensable pour IAuthService, HashedAuthService, etc.
using Bloc4.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bloc4
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var db = new AppDbContext();
            await DbInitializer.EnsureCreatedAsync(db);

            ILoggingService logger = new FileLoggingService();

            // ✅ A majuscule dans IAuthService
            const string ADMIN_HASH = "v1:200000:/b4kpYvjRQ1DZg9vuMtEmA==:Smzj8gRi+iArU9BoQPwchvBXMKdO7RmN3dBXKribdUc=";
            IAuthService auth = new HashedAuthService(ADMIN_HASH);

            ILoginAuditService audit = new EfLoginAuditService(db, logger);

            IDataService data = new EfDataService(db);
            IPdfService  pdf  = new PdfService();
            var http          = new HttpClient();
            IRandomUserService random = new RandomUserService(db, http);

            if (!await db.Salaries.AnyAsync())
                await random.ImportRandomUsersAsync(10, "fr");

            var searchVm = new SearchViewModel(data, pdf, logger);
            await searchVm.LoadListsAsync();

            var adminVm = new AdminViewModel(data, pdf);
            var mainVm  = new MainViewModel(searchVm, adminVm, auth, logger, audit);

            var win = new MainWindow { DataContext = mainVm };
            win.Show();
        }
    }
}