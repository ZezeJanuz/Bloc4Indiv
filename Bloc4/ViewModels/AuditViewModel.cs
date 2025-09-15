using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Bloc4.Data.Models;
using Bloc4.Infrastructure;
using Bloc4.Services;

namespace Bloc4.ViewModels
{
    public class AuditViewModel : ObservableObject
    {
        private readonly ILoginAuditService _audit;
        private readonly ILoggingService _log;

        public ObservableCollection<AdminLoginLog> Logs { get; } = new();

        // Dates locales depuis le XAML (DatePicker)
        private DateTime? _dateFrom;
        public DateTime? DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        private DateTime? _dateTo;
        public DateTime? DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportCsvCommand { get; }

        public AuditViewModel(ILoginAuditService audit, ILoggingService log)
        {
            _audit = audit;
            _log   = log;

            RefreshCommand   = new RelayCommand(async _ => await LoadAsync());
            ExportCsvCommand = new RelayCommand(async _ => await ExportAsync());
        }

        public async Task LoadAsync()
        {
            try
            {
                // Si pas de filtre => on prend les N derniers pour que ça reste rapide
                var fromUtc = ToUtcStartOfDay(DateFrom);
                var toUtc   = ToUtcEndOfDay(DateTo);

                var items = (fromUtc == null && toUtc == null)
                    ? await _audit.GetLastAsync(200)
                    : await _audit.GetByDateRangeAsync(fromUtc, toUtc);

                Logs.Clear();
                foreach (var l in items.OrderByDescending(x => x.AttemptedAt))
                    Logs.Add(l);
            }
            catch (Exception ex)
            {
                await _log.LogErrorAsync($"Chargement des logs KO : {ex.Message}", ex);
            }
        }

        private async Task ExportAsync()
        {
            try
            {
                var fromUtc = ToUtcStartOfDay(DateFrom);
                var toUtc   = ToUtcEndOfDay(DateTo);

                var fileName = $"admin-log_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                var count = await _audit.ExportCsvAsync(path, fromUtc, toUtc);
                await _log.LogInfoAsync($"Export CSV : {count} lignes -> {path}");
            }
            catch (Exception ex)
            {
                await _log.LogErrorAsync($"Export CSV KO : {ex.Message}", ex);
            }
        }

        // Helpers de conversion : Date locale -> UTC (DateTimeOffset) en début/fin de journée
        private static DateTimeOffset? ToUtcStartOfDay(DateTime? localDate)
        {
            if (localDate == null) return null;
            var local = DateTime.SpecifyKind(localDate.Value.Date, DateTimeKind.Local);
            var utc = TimeZoneInfo.ConvertTimeToUtc(local);
            return new DateTimeOffset(utc, TimeSpan.Zero);
        }

        private static DateTimeOffset? ToUtcEndOfDay(DateTime? localDate)
        {
            if (localDate == null) return null;
            var local = DateTime.SpecifyKind(localDate.Value.Date.AddDays(1).AddMilliseconds(-1), DateTimeKind.Local);
            var utc = TimeZoneInfo.ConvertTimeToUtc(local);
            return new DateTimeOffset(utc, TimeSpan.Zero);
        }
    }
}