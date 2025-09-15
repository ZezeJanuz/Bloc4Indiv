using System.Threading.Tasks;
using Bloc4.Data.Models;

namespace Bloc4.Services
{
    public interface IPdfService
    {
        Task<string> GenerateSalariePdfAsync(Salarie s, string? outputPath = null);
        Task ExportSalarieAsync(Salarie s, string? outputPath = null);
        Task<string> ExportPdfAsync(Salarie s, string? outputPath = null);
    }
}