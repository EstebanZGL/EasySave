using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyLog
{
    // JSON implementation of the logger interface with encryption support
    public class JsonLogger : IEncryptionLogger
    {
        private readonly string _logDirectory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        private static readonly object _fileLock = new object(); // Lock object for thread safety

        // Constructor that uses the default log directory (application execution folder/logs)
        public JsonLogger() : this(GetDefaultLogDirectory())
        {
        }

        // Constructor with specified log directory
        public JsonLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        // Gets the default log directory path
        private static string GetDefaultLogDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logsDirectory = Path.Combine(baseDirectory, "Logs");
            
            // Create Logs directory if it doesn't exist
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            
            // Create Json subdirectory
            string jsonLogsDirectory = Path.Combine(logsDirectory, "Json");
            
            // Create Json directory if it doesn't exist
            if (!Directory.Exists(jsonLogsDirectory))
            {
                Directory.CreateDirectory(jsonLogsDirectory);
            }
            
            return jsonLogsDirectory;
        }

        // Implémentation de LogBackupOperationAsync
        public Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            // Réutilise la méthode LogTransferAsync existante
            return LogTransferAsync(jobName, sourcePath, targetPath, fileSize, transferTime);
        }

        // Implémentation de LogApplicationEventAsync
        public async Task LogApplicationEventAsync(string eventName, string details)
        {
            var logEntry = new
            {
                Timestamp = DateTime.Now,
                Event = eventName,
                Details = details,
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };
            
            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            
            try
            {
                string json = JsonSerializer.Serialize(logEntry, _jsonOptions);
                
                // Créer le répertoire de logs s'il n'existe pas
                Directory.CreateDirectory(_logDirectory);
                
                // Ajouter l'entrée au fichier de log de façon sécurisée
                await AppendLogSafelyAsync(logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging application event: {ex.Message}");
            }
        }

        // Logs a file transfer action (original method from ILogger)
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
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }

        // Logs a file transfer action with encryption time (new method from IEncryptionLogger)
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
                MachineName = LogIdentityProvider.GetMachineName(),
                UserName = LogIdentityProvider.GetUserName()
            };

            await WriteLogEntryAsync(logEntry);
        }

        // Helper method to safely append to log files
        private async Task AppendLogSafelyAsync(string filePath, string content)
        {
            // Use a lock to prevent concurrent file access issues
            lock (_fileLock)
            {
                try
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Check if file exists and initialize it with a JSON array if it doesn't
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, "[\n]");
                    }

                    // Read the file content
                    string fileContent = File.ReadAllText(filePath);

                    // Ensure the content is a valid JSON array
                    if (!fileContent.Trim().StartsWith("[") || !fileContent.Trim().EndsWith("]"))
                    {
                        // Backup the corrupted file
                        string backupFile = filePath + ".backup-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        File.Copy(filePath, backupFile, true);
                        
                        // Reset to an empty array
                        fileContent = "[\n]";
                    }

                    // Remove the closing bracket
                    fileContent = fileContent.TrimEnd().TrimEnd(']').TrimEnd();

                    // Add comma if there are existing entries
                    if (fileContent.Length > 1 && !fileContent.EndsWith(","))
                    {
                        fileContent += ",";
                    }

                    // Add the new entry and close the array
                    fileContent += (fileContent.Length > 1 ? "\n  " : "") + content + "\n]";

                    // Write back to the file
                    File.WriteAllText(filePath, fileContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error appending to log file: {ex.Message}");
                    
                    // Ensure we don't lose the log entry - write to a backup file if main file fails
                    try
                    {
                        string backupFilePath = filePath + ".backup-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        File.AppendAllText(backupFilePath, content + Environment.NewLine);
                    }
                    catch
                    {
                        // Last resort - just write to console
                        Console.WriteLine($"CRITICAL: Could not write log entry: {content}");
                    }
                }
            }

            await Task.CompletedTask; // To maintain async signature
        }

        // Writes a log entry to the daily log file
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            string json = JsonSerializer.Serialize(logEntry, _jsonOptions);
            
            await AppendLogSafelyAsync(logFileName, json);
        }
    }

    // Factory for creating logger instances
    public static class LoggerFactory
    {
        // Creates a JSON logger
        public static ILogger CreateJsonLogger(string? logDirectory = null)
        {
            return logDirectory != null ? new JsonLogger(logDirectory) : new JsonLogger();
        }
        
        // Creates a JSON logger with encryption support
        public static IEncryptionLogger CreateEncryptionJsonLogger(string? logDirectory = null)
        {
            return logDirectory != null ? new JsonLogger(logDirectory) : new JsonLogger();
        }
        
        // Creates an XML logger
        public static ILogger CreateXmlLogger(string? logDirectory = null)
        {
            return logDirectory != null ? new XmlLogger(logDirectory) : new XmlLogger();
        }

        // Creates an XML logger with encryption support
        public static IEncryptionLogger CreateEncryptionXmlLogger(string? logDirectory = null)
        {
            return logDirectory != null ? new XmlLogger(logDirectory) : new XmlLogger();
        }

        // Creates a remote logger with encryption support
        public static IEncryptionLogger CreateRemoteLogger(string serverUrl, string format = "json", string? logDirectory = null)
        {
            // Create a fallback logger based on the specified format
            var fallbackLogger = format.ToLower() == "xml" 
                ? CreateEncryptionXmlLogger(logDirectory) 
                : CreateEncryptionJsonLogger(logDirectory);
                
            return new RemoteLogger(serverUrl, fallbackLogger);
        }

        // Creates a logger based on the specified format
        public static ILogger CreateLogger(string format = "json", string? logDirectory = null)
        {
            return format.ToLower() == "xml" 
                ? CreateXmlLogger(logDirectory) 
                : CreateJsonLogger(logDirectory);
        }
        
        // Creates a logger with encryption support based on the specified format
        public static IEncryptionLogger CreateEncryptionLogger(string format = "json", string? logDirectory = null)
        {
            return format.ToLower() == "xml" 
                ? CreateEncryptionXmlLogger(logDirectory) 
                : CreateEncryptionJsonLogger(logDirectory);
        }
        
        // Creates a logger with encryption support based on the specified format and log destination
        public static IEncryptionLogger CreateEncryptionLogger(string format = "json", string? logDirectory = null, 
            string? serverUrl = null, LogDestination logDestination = LogDestination.Local)
        {
            // Create the appropriate logger based on the destination
            switch (logDestination)
            {
                case LogDestination.Remote:
                    if (string.IsNullOrEmpty(serverUrl))
                        throw new ArgumentException("Server URL is required for remote logging", nameof(serverUrl));
                    return CreateRemoteLogger(serverUrl, format, logDirectory);
                    
                case LogDestination.Both:
                    if (string.IsNullOrEmpty(serverUrl))
                        throw new ArgumentException("Server URL is required for remote logging", nameof(serverUrl));
                        
                    // Create a local logger
                    var localLogger = format.ToLower() == "xml" 
                        ? CreateEncryptionXmlLogger(logDirectory) 
                        : CreateEncryptionJsonLogger(logDirectory);
                        
                    // Create a remote logger with the local logger as fallback
                    return new RemoteLogger(serverUrl, localLogger);
                    
                case LogDestination.Local:
                default:
                    return format.ToLower() == "xml" 
                        ? CreateEncryptionXmlLogger(logDirectory) 
                        : CreateEncryptionJsonLogger(logDirectory);
            }
        }
    }

    // Decorator that adds performance metrics to logging
    public class PerformanceLogger : IEncryptionLogger
    {
        private readonly IEncryptionLogger _logger;

        // Constructor
        public PerformanceLogger(IEncryptionLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Implémentation de LogBackupOperationAsync
        public Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            // Calculate transfer rate in MB/s if transfer was successful
            string performanceInfo = string.Empty;
            if (transferTime > 0 && fileSize > 0)
            {
                double transferRateMBps = (fileSize / 1024.0 / 1024.0) / (transferTime / 1000.0);
                performanceInfo = $" [{transferRateMBps:F2} MB/s]";
            }

            // Add performance info to job name
            return _logger.LogBackupOperationAsync($"{jobName}{performanceInfo}", sourcePath, targetPath, fileSize, transferTime);
        }

        // Implémentation de LogApplicationEventAsync
        public Task LogApplicationEventAsync(string eventName, string details)
        {
            // Simplement déléguer à l'implémentation sous-jacente
            return _logger.LogApplicationEventAsync(eventName, details);
        }

        // Adds performance metrics before logging
        public Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            // Calculate transfer rate in MB/s if transfer was successful
            string performanceInfo = string.Empty;
            if (transferTime > 0 && fileSize > 0)
            {
                double transferRateMBps = (fileSize / 1024.0 / 1024.0) / (transferTime / 1000.0);
                performanceInfo = $" [{transferRateMBps:F2} MB/s]";
            }

            // Add performance info to backup name
            return _logger.LogTransferAsync($"{backupName}{performanceInfo}", sourcePath, targetPath, fileSize, transferTime);
        }

        // Adds performance metrics before logging encrypted transfers
        public Task LogEncryptedTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime)
        {
            // Calculate transfer rate in MB/s if transfer was successful
            string performanceInfo = string.Empty;
            if (transferTime > 0 && fileSize > 0)
            {
                double transferRateMBps = (fileSize / 1024.0 / 1024.0) / (transferTime / 1000.0);
                performanceInfo = $" [{transferRateMBps:F2} MB/s]";
            }

            // Add encryption info if applicable
            if (encryptionTime > 0)
            {
                performanceInfo += $" [Encrypted: {encryptionTime}ms]";
            }

            // Add performance info to backup name
            return _logger.LogEncryptedTransferAsync($"{backupName}{performanceInfo}", sourcePath, targetPath, fileSize, transferTime, encryptionTime);
        }
    }
    
    // Enum for log destination options
    public enum LogDestination
    {
        Local,  // Logs stored only locally
        Remote, // Logs sent only to remote server
        Both    // Logs stored locally and sent to remote server
    }
}