using System;
using System.Threading.Tasks;

namespace EasyLog
{
     
    /// Interface for logging backup operations with encryption support
     
    public interface IEncryptionLogger : ILogger
    {
         
        /// Logs a file transfer operation with encryption time
        Task LogEncryptedTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime);
    }
}