using EasyLog;
using System.Threading.Tasks;

namespace EasySave.Services
{
    /// <summary>
    /// Extension methods for IEncryptionLogger interface
    /// </summary>
    public static class EncryptionLoggerExtensions
    {
        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="message">The error message</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static Task LogError(this IEncryptionLogger logger, string message)
        {
            // Since we don't have access to the ILogger implementation, we'll create a workaround
            // by using the LogEncryptedTransferAsync method with special values to indicate an error
            return logger.LogEncryptedTransferAsync(
                "ERROR", // Use ERROR as backup name to indicate this is an error log
                message, // Use the error message as source path
                string.Empty, // Empty target path
                0, // Zero file size
                -1, // Negative transfer time to indicate error
                0); // Zero encryption time
        }

        /// <summary>
        /// Logs a file transfer operation
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="targetPath">Target file path</param>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <param name="transferTime">Transfer time in milliseconds</param>
        /// <param name="encryptionTime">Encryption time in milliseconds</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static Task LogFileTransfer(this IEncryptionLogger logger, string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime)
        {
            // This is a direct pass-through to the LogEncryptedTransferAsync method
            return logger.LogEncryptedTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime, encryptionTime);
        }

        /// <summary>
        /// Logs the completion of a backup job
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="targetPath">Target directory path</param>
        /// <param name="totalFiles">Total number of files processed</param>
        /// <param name="totalSize">Total size of files processed in bytes</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static Task LogBackupComplete(this IEncryptionLogger logger, string backupName, string sourcePath, string targetPath, int totalFiles, long totalSize)
        {
            // We'll use the LogEncryptedTransferAsync method with special values to indicate completion
            return logger.LogEncryptedTransferAsync(
                backupName,
                sourcePath,
                targetPath,
                totalSize,
                0, // Use 0 for transfer time to indicate this is a summary, not a file transfer
                -999); // Use a special value for encryption time to indicate this is a completion log
        }
    }
}