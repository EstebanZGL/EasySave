using System;
using System.Text.Json.Serialization;

namespace EasySave.LogServer.Models
{
     
    /// Represents a log entry in the centralized log system
     
    public class LogEntry
    {
         
        /// Gets or sets the unique identifier for the log entry
         
        public Guid Id { get; set; } = Guid.NewGuid();
        
         
        /// Gets or sets the timestamp of the log entry
         
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
         
        /// Gets or sets the name of the backup job
         
        public string BackupName { get; set; } = string.Empty;
        
         
        /// Gets or sets the source file path
         
        public string SourcePath { get; set; } = string.Empty;
        
         
        /// Gets or sets the target file path
         
        public string TargetPath { get; set; } = string.Empty;
        
         
        /// Gets or sets the size of the file in bytes
         
        public long FileSize { get; set; }
        
         
        /// Gets or sets the transfer time in milliseconds
         
        public long TransferTime { get; set; }
        
         
        /// Gets or sets the encryption time in milliseconds (0 if not encrypted, negative if error)
         
        public long EncryptionTime { get; set; }
        
         
        /// Gets or sets the client machine name
         
        public string MachineName { get; set; } = string.Empty;
        
         
        /// Gets or sets the client username
         
        public string UserName { get; set; } = string.Empty;
        
         
        /// Gets or sets the log type (e.g., "Transfer", "ApplicationEvent", "BackupOperation")
         
        public string LogType { get; set; } = string.Empty;
        
         
        /// Gets or sets additional details for the log entry
         
        public string Details { get; set; } = string.Empty;
    }
}