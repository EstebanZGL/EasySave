using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog
{
    /// <summary>
    /// XML implementation of the logger interface with encryption support
    /// </summary>
    public class XmlLogger : IEncryptionLogger
    {
        private readonly string _logDirectory;

        /// <summary>
        /// Constructor that uses the default log directory (application execution folder/logs)
        /// </summary>
        public XmlLogger() : this(GetDefaultLogDirectory())
        {
        }

        /// <summary>
        /// Constructor with specified log directory
        /// </summary>
        /// <param name="logDirectory">Directory where log files will be stored</param>
        public XmlLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Gets the default log directory path
        /// </summary>
        /// <returns>Path to the logs directory</returns>
        private static string GetDefaultLogDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logDirectory = Path.Combine(baseDirectory, "logs");
            
            // Créer le dossier logs s'il n'existe pas
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            return logDirectory;
        }

        /// <summary>
        /// Implements the ILogger.LogBackupOperationAsync method
        /// </summary>
        public Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            // Réutiliser la méthode LogTransferAsync existante
            return LogTransferAsync(jobName, sourcePath, targetPath, fileSize, transferTime);
        }

        /// <summary>
        /// Implements the ILogger.LogApplicationEventAsync method
        /// </summary>
        public Task LogApplicationEventAsync(string eventName, string details)
        {
            // Réutiliser la méthode LogApplicationEventAsync existante avec un paramètre null pour le troisième argument
            return LogApplicationEventAsync(eventName, details, null);
        }

        /// <summary>
        /// Logs a file transfer action (original method from ILogger)
        /// </summary>
        public async Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTime = transferTime,
                EncryptionTime = 0 // Default to 0 for non-encrypted transfers
            };

            await WriteLogEntryAsync(logEntry);
        }

        /// <summary>
        /// Logs a file transfer action with encryption time (new method from IEncryptionLogger)
        /// </summary>
        public async Task LogEncryptedTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTime = transferTime,
                EncryptionTime = encryptionTime
            };

            await WriteLogEntryAsync(logEntry);
        }

        /// <summary>
        /// Writes a log entry to the daily log file
        /// </summary>
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
            
            List<LogEntry> logEntries = new List<LogEntry>();
            if (File.Exists(logFileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<LogEntry>));
                    using (FileStream fs = new FileStream(logFileName, FileMode.Open))
                    {
                        logEntries = (List<LogEntry>)serializer.Deserialize(fs) ?? new List<LogEntry>();
                    }
                }
                catch
                {
                    // If deserialization fails, start with a new list
                    logEntries = new List<LogEntry>();
                }
            }
            
            logEntries.Add(logEntry);
            
            XmlSerializer writer = new XmlSerializer(typeof(List<LogEntry>));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Async = true
            };
            
            using (XmlWriter xmlWriter = XmlWriter.Create(logFileName, settings))
            {
                writer.Serialize(xmlWriter, logEntries);
                await xmlWriter.FlushAsync();
            }
        }
        
        /// <summary>
        /// Logs a directory creation operation.
        /// </summary>
        public async Task LogDirectoryCreationAsync(string backupName, string directoryPath)
        {
            // Implement this method as needed
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Logs a directory deletion operation.
        /// </summary>
        public async Task LogDirectoryDeletionAsync(string backupName, string directoryPath)
        {
            // Implement this method as needed
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Logs an application event (startup, shutdown, error, etc.).
        /// </summary>
        public async Task LogApplicationEventAsync(string eventType, string message, string? details = null)
        {
            // Implement this method as needed
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Logs a backup job management operation (creation, deletion, modification).
        /// </summary>
        public async Task LogJobManagementAsync(string operationType, string jobName, string? details = null)
        {
            // Implement this method as needed
            await Task.CompletedTask;
        }
    }
}