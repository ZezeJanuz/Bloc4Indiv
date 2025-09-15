using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Bloc4.Data.Models;
using Bloc4.Infrastructure;
using Bloc4.Services;

namespace Bloc4.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        private readonly IDataService _data;
        private readonly IPdfService _pdf;
        private readonly ILoggingService _log;

        public ObservableCollection<Site> Sites { get; } = new();
        public ObservableCollection<Service> Services { get; } = new();
        public ObservableCollection<Salarie> Results { get; } = new();

        // Sentinelles "Tout" (Id=0 pour signifier "pas de filtre")
        private static readonly Site AllSite = new Site { Id = 0, Ville = "Tout" };
        private static readonly Service AllService = new Service { Id = 0, Nom = "Tout" };

        private Salarie? _selectedSalarie;
        public Salarie? SelectedSalarie
        {
            get => _selectedSalarie;
            set
            {
                if (SetProperty(ref _selectedSalarie, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private Site? _selectedSite;
        public Site? SelectedSite
        {
            get => _selectedSite;
            set
            {
                if (SetProperty(ref _selectedSite, value))
                    _ = DebouncedSearchAsync();
            }
        }

        private Service? _selectedService;
        public Service? SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetProperty(ref _selectedService, value))
                    _ = DebouncedSearchAsync();
            }
        }

        private string _nomQuery = string.Empty;
        public string NomQuery
        {
            get => _nomQuery;
            set
            {
                if (SetProperty(ref _nomQuery, value ?? string.Empty))
                    _ = DebouncedSearchAsync();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand RefreshListsCommand { get; }
        public ICommand ExportSelectedPdfCommand { get; }

        private CancellationTokenSource? _cts;

        public SearchViewModel(IDataService data, IPdfService pdf, ILoggingService log)
        {
            _data = data;
            _pdf  = pdf;
            _log  = log;

            RefreshListsCommand = new RelayCommand(async _ => await LoadListsAsync());
            ExportSelectedPdfCommand = new RelayCommand(
                async _ => await ExportSelectedAsync(),
                _ => SelectedSalarie != null
            );
        }

        /// <summary>
        /// Charge Sites/Services, insère "Tout" en tête, sélectionne "Tout" par défaut, puis lance la recherche.
        /// </summary>
        public async Task LoadListsAsync()
        {
            try
            {
                IsBusy = true;

                // Sites
                Sites.Clear();
                Sites.Add(AllSite); // "Tout"
                var sites = await _data.GetSitesAsync();
                foreach (var s in sites.OrderBy(x => x.Ville))
                    Sites.Add(s);

                // Services
                Services.Clear();
                Services.Add(AllService); // "Tout"
                var services = await _data.GetServicesAsync();
                foreach (var s in services.OrderBy(x => x.Nom))
                    Services.Add(s);

                // Par défaut : "Tout" sélectionné
                SelectedSite ??= AllSite;
                SelectedService ??= AllService;

                await SearchAsync();
            }
            finally { IsBusy = false; }
        }

        private async Task DebouncedSearchAsync(int delayMs = 250)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(delayMs, token);
                await SearchAsync();
            }
            catch (TaskCanceledException) { /* normal */ }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                var name = string.IsNullOrWhiteSpace(NomQuery) ? null : NomQuery.Trim();

                // Si "Tout" (Id=0) => pas de filtre
                int? siteId    = (SelectedSite != null && SelectedSite.Id    != 0) ? SelectedSite.Id    : null;
                int? serviceId = (SelectedService != null && SelectedService.Id != 0) ? SelectedService.Id : null;

                var list = await _data.SearchSalariesAsync(name, siteId, serviceId);

                Results.Clear();
                foreach (var r in list)
                    Results.Add(r);
            }
            catch (Exception ex)
            {
                await _log.LogErrorAsync($"Recherche KO : {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        private async Task ExportSelectedAsync()
        {
            if (SelectedSalarie == null) return;

            try
            {
                await _pdf.ExportSalarieAsync(SelectedSalarie);
                await _log.LogInfoAsync($"PDF exporté pour {SelectedSalarie.Prenom} {SelectedSalarie.Nom}");
            }
            catch (Exception ex)
            {
                await _log.LogErrorAsync($"Export PDF a échoué: {ex.Message}");
            }
        }
    }
}