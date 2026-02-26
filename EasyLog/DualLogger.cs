using System.Threading.Tasks;

namespace EasyLog
{
    public class DualLogger : ILogger
    {
        private readonly ILogger _localLogger;
        private readonly ILogger _remoteLogger;

        public DualLogger(ILogger localLogger, ILogger remoteLogger)
        {
            _localLogger = localLogger;
            _remoteLogger = remoteLogger;
        }

        public async Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            await _localLogger.LogTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime);
            await _remoteLogger.LogTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime);
        }

        public async Task LogApplicationEventAsync(string eventName, string details)
        {
            await _localLogger.LogApplicationEventAsync(eventName, details);
            await _remoteLogger.LogApplicationEventAsync(eventName, details);
        }

        public async Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            await _localLogger.LogBackupOperationAsync(jobName, sourcePath, targetPath, fileSize, transferTime);
            await _remoteLogger.LogBackupOperationAsync(jobName, sourcePath, targetPath, fileSize, transferTime);
        }
    }
}