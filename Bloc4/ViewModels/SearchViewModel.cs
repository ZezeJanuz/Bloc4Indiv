using System.Collections.ObjectModel;
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

        public ObservableCollection<Site> Sites { get; } = new();
        public ObservableCollection<Service> Services { get; } = new();
        public ObservableCollection<Salarie> Results { get; } = new();

        private string? _nomQuery;
        public string? NomQuery
        {
            get => _nomQuery;
            set
            {
                if (SetProperty(ref _nomQuery, value))
                    _ = SearchAsync();
            }
        }

        private Site? _selectedSite;
        public Site? SelectedSite
        {
            get => _selectedSite;
            set
            {
                if (SetProperty(ref _selectedSite, value))
                    _ = SearchAsync();
            }
        }

        private Service? _selectedService;
        public Service? SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetProperty(ref _selectedService, value))
                    _ = SearchAsync();
            }
        }

        public ICommand RefreshListsCommand { get; }

        public SearchViewModel(IDataService data)
        {
            _data = data;
            RefreshListsCommand = new RelayCommand(async _ => await LoadListsAsync());
        }

        public async Task LoadListsAsync()
        {
            Sites.Clear();
            foreach (var s in await _data.GetSitesAsync()) Sites.Add(s);

            Services.Clear();
            foreach (var s in await _data.GetServicesAsync()) Services.Add(s);

            await SearchAsync();
        }

        public async Task SearchAsync()
        {
            Results.Clear();
            var list = await _data.SearchSalariesAsync(NomQuery, SelectedSite?.Id, SelectedService?.Id);
            foreach (var s in list) Results.Add(s);
        }
    }
}
