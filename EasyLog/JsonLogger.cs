using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyLog
{
    // JSON implementation of the logger interface
    public class JsonLogger : ILogger
    {
        private readonly string _logDirectory;

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
            string easySaveDirectory = Path.Combine(baseDirectory, "EasySave");
            
            // Create EasySave directory if it doesn't exist
            if (!Directory.Exists(easySaveDirectory))
            {
                Directory.CreateDirectory(easySaveDirectory);
            }
            
            string logDirectory = Path.Combine(easySaveDirectory, "logs");
            
            // Create logs directory if it doesn't exist
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            return logDirectory;
        }

        // Logs a file transfer action
        public async Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTime = transferTime
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

    // Log entry for a file transfer operation
    public class LogEntry
    {
        // Timestamp of the log entry
        public DateTime Timestamp { get; set; }
        
        // Name of the backup job
        public string BackupName { get; set; }
        
        // Source file path
        public string SourcePath { get; set; }
        
        // Target file path
        public string TargetPath { get; set; }
        
        // Size of the file in bytes
        public long FileSize { get; set; }
        
        // Transfer time in milliseconds (negative if error)
        public long TransferTime { get; set; }
    }

    // Factory for creating logger instances
    public static class LoggerFactory
    {
        // Creates a JSON logger
        public static ILogger CreateJsonLogger(string logDirectory = null)
        {
            return logDirectory != null ? new JsonLogger(logDirectory) : new JsonLogger();
        }
        
        // Future: Add CreateXmlLogger when needed for v1.1
    }

    // Decorator that adds performance metrics to logging
    public class PerformanceLogger : ILogger
    {
        private readonly ILogger _logger;

        // Constructor
        public PerformanceLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Adds performance metrics before logging
        public async Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            // Calculate transfer rate in MB/s if transfer was successful
            string performanceInfo = string.Empty;
            if (transferTime > 0 && fileSize > 0)
            {
                double transferRateMBps = (fileSize / 1024.0 / 1024.0) / (transferTime / 1000.0);
                performanceInfo = $" [{transferRateMBps:F2} MB/s]";
            }

            // Add performance info to backup name
            await _logger.LogTransferAsync($"{backupName}{performanceInfo}", sourcePath, targetPath, fileSize, transferTime);
        }
    }
}