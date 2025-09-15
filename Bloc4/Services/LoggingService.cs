using System;
using System.IO;
using System.Threading.Tasks;

namespace Bloc4.Services
{
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
            var dir = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.AppendAllTextAsync(_logPath, line + Environment.NewLine);
        }
    }
}