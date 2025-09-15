using System;
using System.Threading.Tasks;

namespace Bloc4.Services
{
    public interface ILoggingService
    {
        Task LogInfoAsync(string message);
        Task LogErrorAsync(string message, Exception? ex = null);
    }
}