using System;

namespace EasyLog
{
     
    /// Log entry for a file transfer operation (serializable)
     
    [Serializable]
    public class LogEntry
    {
         
        /// Timestamp of the log entry
         
        public DateTime Timestamp { get; set; }
        
         
        /// Name of the backup job
         
        public string BackupName { get; set; } = string.Empty;
        
         
        /// Source file path
         
        public string SourcePath { get; set; } = string.Empty;
        
         
        /// Target file path
         
        public string TargetPath { get; set; } = string.Empty;
        
         
        /// Size of the file in bytes
         
        public long FileSize { get; set; }
        
         
        /// Transfer time in milliseconds (negative if error)
         
        public long TransferTime { get; set; }
        
         
        /// Encryption time in milliseconds (0 if not encrypted, negative if error)
         
        public long EncryptionTime { get; set; }
        
         
        /// Type of operation (FileTransfer, DirectoryCreation, DirectoryDeletion, ApplicationEvent, JobManagement, etc.)
         
        public string OperationType { get; set; } = string.Empty;
    }
}