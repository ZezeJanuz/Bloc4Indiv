using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloc4.Data;
using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Services
{
    public class EfLoginAuditService : ILoginAuditService
    {
        private readonly AppDbContext _db;
        private readonly ILoggingService? _fileLog;

        public EfLoginAuditService(AppDbContext db, ILoggingService? fileLog = null)
        {
            _db = db;
            _fileLog = fileLog;
        }

        public async Task LogAdminAttemptAsync(bool success, string? reason = null)
        {
            await EnsureTableExistsAsync();

            var entry = new AdminLoginLog
            {
                AttemptedAt = DateTimeOffset.UtcNow,
                Success = success,
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                Reason = reason
            };

            _db.AdminLoginLogs.Add(entry);
            await _db.SaveChangesAsync();

            var status = success ? "SUCCESS" : "FAIL";
            var line = $"[ADMIN LOGIN] {status} at {entry.AttemptedAt:u} / {entry.UserName}@{entry.MachineName} {(string.IsNullOrWhiteSpace(reason) ? "" : $"({reason})")}";

            if (_fileLog != null)
                await _fileLog.LogInfoAsync(line);
            else
            {
                try
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "admin-login.log");
                    await File.AppendAllLinesAsync(path, new[] { $"{DateTimeOffset.UtcNow:u}\t{status}\t{Environment.UserName}@{Environment.MachineName}\t{reason}" });
                }
                catch { /* best effort */ }
            }
        }

        public async Task<List<AdminLoginLog>> GetLastAsync(int take = 100)
        {
            await EnsureTableExistsAsync();

            if (_db.Database.IsSqlite())
            {
                var all = await _db.AdminLoginLogs.AsNoTracking().ToListAsync();
                return all.OrderByDescending(x => x.AttemptedAt).Take(take).ToList();
            }

            return await _db.AdminLoginLogs
                            .OrderByDescending(x => x.AttemptedAt)
                            .Take(take)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<List<AdminLoginLog>> GetByDateRangeAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc)
        {
            await EnsureTableExistsAsync();

            if (_db.Database.IsSqlite())
            {
                var all = await _db.AdminLoginLogs.AsNoTracking().ToListAsync();
                if (fromUtc.HasValue) all = all.Where(x => x.AttemptedAt >= fromUtc.Value).ToList();
                if (toUtc.HasValue)   all = all.Where(x => x.AttemptedAt <= toUtc.Value).ToList();
                return all.OrderByDescending(x => x.AttemptedAt).ToList();
            }

            var q = _db.AdminLoginLogs.AsNoTracking().AsQueryable();
            if (fromUtc.HasValue) q = q.Where(x => x.AttemptedAt >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(x => x.AttemptedAt <= toUtc.Value);
            return await q.OrderByDescending(x => x.AttemptedAt).ToListAsync();
        }

        public async Task<int> ExportCsvAsync(string filePath, DateTimeOffset? fromUtc, DateTimeOffset? toUtc)
        {
            var logs = await GetByDateRangeAsync(fromUtc, toUtc);
            var sb = new StringBuilder();
            sb.AppendLine("Id;AttemptedAt(UTC);Success;UserName;MachineName;Reason");
            foreach (var l in logs)
            {
                var date = l.AttemptedAt.ToString("u", CultureInfo.InvariantCulture);
                var reason = (l.Reason ?? "").Replace(";", ",");
                sb.AppendLine($"{l.Id};{date};{(l.Success ? 1 : 0)};{l.UserName};{l.MachineName};{reason}");
            }
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return logs.Count;
        }

        private async Task EnsureTableExistsAsync()
        {
            await _db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS AdminLoginLogs
(
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    AttemptedAt   TEXT    NOT NULL,
    Success       INTEGER NOT NULL,
    MachineName   TEXT    NULL,
    UserName      TEXT    NULL,
    Reason        TEXT    NULL
);");
        }
    }
}