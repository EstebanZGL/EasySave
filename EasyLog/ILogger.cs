using System;
using System.Threading.Tasks;

namespace EasyLog
{
    /// <summary>
    /// Interface for logging backup operations with support for different implementations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a file transfer operation.
        /// </summary>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="targetPath">Target file path</param>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <param name="transferTime">Transfer time in milliseconds (negative if error)</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime);
    }
}