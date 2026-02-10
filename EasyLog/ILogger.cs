using System;
using System.Threading.Tasks;

namespace EasyLog
{
    // Interface for logging backup operations with support for different implementations
    public interface ILogger
    {
        // Logs a file transfer operation
        // backupName: Name of the backup job
        // sourcePath: Source file path
        // targetPath: Target file path
        // fileSize: Size of the file in bytes
        // transferTime: Transfer time in milliseconds (negative if error)
        // Returns: Task representing the asynchronous operation
        Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime);
        Task LogApplicationEventAsync(string eventName, string details);
        Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime);
    }
}