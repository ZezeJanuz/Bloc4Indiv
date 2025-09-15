using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bloc4.Data.Models;

namespace Bloc4.Services
{
    public interface ILoginAuditService
    {
        Task LogAdminAttemptAsync(bool success, string? reason = null);
        Task<List<AdminLoginLog>> GetLastAsync(int take = 100);
        Task<List<AdminLoginLog>> GetByDateRangeAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc);
        Task<int> ExportCsvAsync(string filePath, DateTimeOffset? fromUtc, DateTimeOffset? toUtc);
    }
}