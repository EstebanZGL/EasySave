using EasyLog;
using System.Threading.Tasks;

namespace EasySave.Services
{
     
    /// Extension methods for IEncryptionLogger interface
     
    public static class EncryptionLoggerExtensions
    {
         
        /// Logs an error message
         
        public static Task LogError(this IEncryptionLogger logger, string message)
        {
            // Since we don't have access to the ILogger implementation, we'll create a workaround
            // by using the LogEncryptedTransferAsync method with special values to indicate an error
            return logger.LogEncryptedTransferAsync(
                "ERROR", // Use ERROR as backup name to indicate this is an error log
                message, 
                string.Empty, // Empty target path
                0, // Zero file size
                -1, // Negative to indicate error
                0); // Zero encryption time
        }

         
        /// Logs a file transfer operation
         
        public static Task LogFileTransfer(this IEncryptionLogger logger, string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime)
        {
            // This is a direct pass-through to the LogEncryptedTransferAsync method
            return logger.LogEncryptedTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime, encryptionTime);
        }

         
        /// Logs the completion of a backup job
         
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