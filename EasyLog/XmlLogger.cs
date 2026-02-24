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
        private static readonly object _fileLock = new object(); // Lock object for thread safety

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
            string logsDirectory = Path.Combine(baseDirectory, "Logs");
            
            // Create Logs directory if it doesn't exist
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            
            // Create Xml subdirectory
            string xmlLogsDirectory = Path.Combine(logsDirectory, "Xml");
            
            // Create Xml directory if it doesn't exist
            if (!Directory.Exists(xmlLogsDirectory))
            {
                Directory.CreateDirectory(xmlLogsDirectory);
            }
            
            return xmlLogsDirectory;
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
                EncryptionTime = 0, // Default to 0 for non-encrypted transfers
                OperationType = "FileTransfer",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
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
                EncryptionTime = encryptionTime,
                OperationType = "EncryptedFileTransfer",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }

        /// <summary>
        /// Writes a log entry to the daily log file in a safe manner that prevents log loss
        /// </summary>
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
            
            // Use a lock to prevent concurrent file access issues
            lock (_fileLock)
            {
                try
                {
                    List<LogEntry> logEntries = new List<LogEntry>();
                    
                    // Ensure the directory exists
                    if (!Directory.Exists(_logDirectory))
                    {
                        Directory.CreateDirectory(_logDirectory);
                    }
                    
                    // Read existing entries if file exists
                    if (File.Exists(logFileName))
                    {
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<LogEntry>));
                            using (FileStream fs = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var existingEntries = (List<LogEntry>)serializer.Deserialize(fs);
                                if (existingEntries != null)
                                {
                                    logEntries = existingEntries;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // If deserialization fails, create a backup of the corrupted file
                            Console.WriteLine($"Error reading log file: {ex.Message}. Creating a backup and starting a new log.");
                            string backupFile = logFileName + ".backup-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                            File.Copy(logFileName, backupFile, true);
                            
                            // Start with a new list but don't delete the original file yet
                            logEntries = new List<LogEntry>();
                        }
                    }
                    
                    // Add new entry
                    logEntries.Add(logEntry);
                    
                    // Write to a temporary file first
                    string tempFile = Path.GetTempFileName();
                    
                    XmlSerializer writer = new XmlSerializer(typeof(List<LogEntry>));
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        Async = false // Changed to synchronous for the lock context
                    };
                    
                    using (XmlWriter xmlWriter = XmlWriter.Create(tempFile, settings))
                    {
                        writer.Serialize(xmlWriter, logEntries);
                        xmlWriter.Flush();
                    }
                    
                    // Verify the temp file was written successfully
                    if (new FileInfo(tempFile).Length > 0)
                    {
                        // Replace the original file only if the temp file was written successfully
                        File.Move(tempFile, logFileName, true);
                    }
                    else
                    {
                        // If temp file is empty, don't replace the original
                        File.Delete(tempFile);
                        throw new IOException("Failed to write log entry: temporary file is empty");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to log file: {ex.Message}");
                    
                    // Ensure we don't lose the log entry - write to a backup file if main file fails
                    try
                    {
                        // Create a simple XML representation of the single entry
                        string backupFilePath = logFileName + ".backup-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        
                        XmlSerializer singleWriter = new XmlSerializer(typeof(LogEntry));
                        using (XmlWriter xmlWriter = XmlWriter.Create(backupFilePath))
                        {
                            singleWriter.Serialize(xmlWriter, logEntry);
                        }
                    }
                    catch
                    {
                        // Last resort - just write to console
                        Console.WriteLine($"CRITICAL: Could not write log entry: {logEntry.BackupName}, {logEntry.SourcePath}, {logEntry.TargetPath}");
                    }
                }
            }
            
            await Task.CompletedTask; // To maintain async signature
        }
        
        /// <summary>
        /// Logs a directory creation operation.
        /// </summary>
        public async Task LogDirectoryCreationAsync(string backupName, string directoryPath)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = directoryPath,
                TargetPath = string.Empty,
                FileSize = 0,
                TransferTime = 0,
                EncryptionTime = 0,
                OperationType = "DirectoryCreation",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }
        
        /// <summary>
        /// Logs a directory deletion operation.
        /// </summary>
        public async Task LogDirectoryDeletionAsync(string backupName, string directoryPath)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = directoryPath,
                TargetPath = string.Empty,
                FileSize = 0,
                TransferTime = 0,
                EncryptionTime = 0,
                OperationType = "DirectoryDeletion",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }
        
        /// <summary>
        /// Logs an application event (startup, shutdown, error, etc.).
        /// </summary>
        public async Task LogApplicationEventAsync(string eventType, string message, string? details = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = eventType,
                SourcePath = message,
                TargetPath = details ?? string.Empty,
                FileSize = 0,
                TransferTime = 0,
                EncryptionTime = 0,
                OperationType = "ApplicationEvent",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }
        
        /// <summary>
        /// Logs a backup job management operation (creation, deletion, modification).
        /// </summary>
        public async Task LogJobManagementAsync(string operationType, string jobName, string? details = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = jobName,
                SourcePath = operationType,
                TargetPath = details ?? string.Empty,
                FileSize = 0,
                TransferTime = 0,
                EncryptionTime = 0,
                OperationType = "JobManagement",
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }
    }
}