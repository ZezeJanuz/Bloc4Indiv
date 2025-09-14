using System;
using System.IO;
using System.Threading.Tasks;

namespace Bloc4.Services
{
    public interface ILoggingService
    {
        Task LogInfoAsync(string message);
        Task LogErrorAsync(string message, Exception? ex = null);
    }

    public class FileLoggingService : ILoggingService
    {
        private readonly string _logPath;

        public FileLoggingService(string? logPath = null)
        {
            _logPath = logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
        }

        public Task LogInfoAsync(string message) 
            => AppendAsync($"[INFO ] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");

        public Task LogErrorAsync(string message, Exception? ex = null) 
            => AppendAsync($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message} {ex}");

        private async Task AppendAsync(string line)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            await File.AppendAllTextAsync(_logPath, line + Environment.NewLine);
        }
    }
}