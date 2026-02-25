using System;

namespace EasyLog
{
    /// <summary>
    /// Log entry for a file transfer operation (serializable)
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        /// <summary>
        /// Timestamp of the log entry
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Name of the backup job
        /// </summary>
        public string BackupName { get; set; } = string.Empty;
        
        /// <summary>
        /// Source file path
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Target file path
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Transfer time in milliseconds (negative if error)
        /// </summary>
        public long TransferTime { get; set; }
        
        /// <summary>
        /// Encryption time in milliseconds (0 if not encrypted, negative if error)
        /// </summary>
        public long EncryptionTime { get; set; }
        
        
        /// <summary>
        /// Name of the machine where the log was generated
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the user who executed the operation
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}