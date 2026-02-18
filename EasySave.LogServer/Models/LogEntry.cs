using System;
using System.Text.Json.Serialization;

namespace EasySave.LogServer.Models
{
    /// <summary>
    /// Represents a log entry in the centralized log system
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier for the log entry
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the timestamp of the log entry
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the name of the backup job
        /// </summary>
        public string BackupName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the source file path
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the target file path
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Gets or sets the transfer time in milliseconds
        /// </summary>
        public long TransferTime { get; set; }
        
        /// <summary>
        /// Gets or sets the encryption time in milliseconds (0 if not encrypted, negative if error)
        /// </summary>
        public long EncryptionTime { get; set; }
        
        /// <summary>
        /// Gets or sets the client machine name
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the client username
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the log type (e.g., "Transfer", "ApplicationEvent", "BackupOperation")
        /// </summary>
        public string LogType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets additional details for the log entry
        /// </summary>
        public string Details { get; set; } = string.Empty;
    }
}