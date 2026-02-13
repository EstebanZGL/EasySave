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
                Details = details
            };
            
            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            
            try
            {
                string json = JsonSerializer.Serialize(logEntry, _jsonOptions);
                
                // Créer le répertoire de logs s'il n'existe pas
                Directory.CreateDirectory(_logDirectory);
                
                // Ajouter l'entrée au fichier de log
                await File.AppendAllTextAsync(logFilePath, json + Environment.NewLine);
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
                EncryptionTime = 0 // Default to 0 for non-encrypted transfers
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
                EncryptionTime = encryptionTime
            };

            await WriteLogEntryAsync(logEntry);
        }

        // Writes a log entry to the daily log file
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            
            // Optimization: Use a more efficient approach to read and write log entries
            List<LogEntry> logEntries;
            
            if (File.Exists(logFileName))
            {
                try
                {
                    // Read existing log file
                    string existingJson = await File.ReadAllTextAsync(logFileName);
                    logEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
                catch
                {
                    // If deserialization fails, start with a new list
                    logEntries = new List<LogEntry>();
                }
            }
            else
            {
                // If file doesn't exist, create a new list
                logEntries = new List<LogEntry>();
            }
            
            // Add new entry
            logEntries.Add(logEntry);
            
            // Write back to file with indentation for readability
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(logEntries, options);
            
            // Use atomic write operation to prevent file corruption
            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, json);
            File.Move(tempFile, logFileName, true);
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
}