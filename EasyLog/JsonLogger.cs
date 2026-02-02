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
        /// Constructor
        /// </summary>
        /// <param name="logDirectory">Directory where log files will be stored</param>
        public JsonLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            
            // Create log directory if it doesn't exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Log a file transfer action
        /// </summary>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="targetPath">Target file path</param>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <param name="transferTime">Transfer time in milliseconds (negative if error)</param>
        /// <returns>Task representing the asynchronous operation</returns>
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
        /// Write a log entry to the daily log file
        /// </summary>
        /// <param name="logEntry">Log entry to write</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            string logFileName = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            
            // Read existing log entries if the file exists
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
            
            // Add the new entry
            logEntries.Add(logEntry);
            
            // Write back to the file with indentation for readability
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(logEntries, options);
            await File.WriteAllTextAsync(logFileName, json);
        }
    }

    /// <summary>
    /// Class representing a log entry
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