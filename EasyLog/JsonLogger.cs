using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyLog
{
    /// <summary>
    /// JSON implementation of the logger interface
    /// </summary>
    public class JsonLogger : ILogger
    {
        private readonly string _logDirectory;

        /// <summary>
        /// Constructor that uses the default log directory (application execution folder/logs)
        /// </summary>
        public JsonLogger() : this(GetLogDirectory())
        {
        }

        /// <summary>
        /// Constructor with specified log directory
        /// </summary>
        /// <param name="logDirectory">Directory where log files will be stored</param>
        public JsonLogger(string logDirectory)
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
        private static string GetLogDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logDirectory = Path.Combine(baseDirectory, "logs");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            return logDirectory;
        }

        /// <summary>
        /// Logs a file transfer action
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
                TransferTime = transferTime
            };

            await WriteLogEntryAsync(logEntry);
        }

        /// <summary>
        /// Writes a log entry to the daily log file
        /// </summary>
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            
            List<LogEntry> logEntries = new List<LogEntry>();
            if (File.Exists(logFileName))
            {
                string existingJson = await File.ReadAllTextAsync(logFileName);
                try
                {
                    logEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
                catch
                {
                    // If deserialization fails, start with a new list
                    logEntries = new List<LogEntry>();
                }
            }
            
            logEntries.Add(logEntry);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(logEntries, options);
            
            await File.WriteAllTextAsync(logFileName, json);
        }
    }

    /// <summary>
    /// Log entry for a file transfer operation
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Timestamp of the log entry
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Name of the backup job
        /// </summary>
        public string BackupName { get; set; }
        
        /// <summary>
        /// Source file path
        /// </summary>
        public string SourcePath { get; set; }
        
        /// <summary>
        /// Target file path
        /// </summary>
        public string TargetPath { get; set; }
        
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Transfer time in milliseconds (negative if error)
        /// </summary>
        public long TransferTime { get; set; }
    }
}