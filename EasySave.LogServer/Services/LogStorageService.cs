using EasySave.LogServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.LogServer.Services
{
     
    /// Service responsible for managing centralized logs
     
    public class LogStorageService
    {
        private readonly string _logDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, List<LogEntry>> _dailyLogs;
        private readonly object _logLock = new object();
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

         
        /// Creates a new instance of the LogStorageService
         
        public LogStorageService(string logDirectory)
        {
            _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
            
            // Create the log directory if it doesn't exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            
            // Create subdirectories for JSON and XML logs
            EnsureDirectoryExists(Path.Combine(_logDirectory, "Json"));
            EnsureDirectoryExists(Path.Combine(_logDirectory, "Xml"));
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            _dailyLogs = new Dictionary<string, List<LogEntry>>();
            
            // Load existing logs for today
            LoadTodaysLogs();
        }

         
        /// Adds a log entry to the centralized log system
         
        public async Task AddLogEntryAsync(LogEntry logEntry)
        {
            if (logEntry == null)
                throw new ArgumentNullException(nameof(logEntry));
                
            // Set the timestamp if not already set
            if (logEntry.Timestamp == default)
            {
                logEntry.Timestamp = DateTime.Now;
            }
            
            // Get the date key for today
            string dateKey = logEntry.Timestamp.ToString("yyyy-MM-dd");
            
            // Use a lock for thread safety when modifying the in-memory collection
            lock (_logLock)
            {
                // Add the log entry to the in-memory collection
                if (!_dailyLogs.TryGetValue(dateKey, out var logs))
                {
                    logs = new List<LogEntry>();
                    _dailyLogs[dateKey] = logs;
                }
                
                logs.Add(logEntry);
            }
            
            // Save the logs to disk
            await SaveLogsToDiskAsync(dateKey);
        }

         
        /// Gets all log entries for a specific date
         
        public List<LogEntry> GetLogEntriesForDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");
            string jsonFilePath = GetJsonLogFilePath(dateKey);

            // === 1. LE CACHE INTELLIGENT (POUR ALLER VITE) ===
            // Si on demande une date passée (hier, avant-hier...), le fichier ne changera plus.
            // On regarde d'abord si on l'a déjà en mémoire vive (RAM) !
            if (date.Date < DateTime.Now.Date)
            {
                lock (_logLock)
                {
                    if (_dailyLogs.TryGetValue(dateKey, out var memLogs))
                    {
                        return memLogs.ToList(); 
                    }
                }
            }

            // === 2. LA LECTURE SUR LE DISQUE DUR ===
            // Pour aujourd'hui (qui change tout le temps), ou si c'est le tout premier chargement d'une ancienne date.
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    using (var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        
                        // Si le fichier est vide, on renvoie une liste vide
                        if (string.IsNullOrWhiteSpace(jsonContent))
                            return new List<LogEntry>();

                        // On force la tolérance sur les majuscules/minuscules pour les anciens logs
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent, options);
                        
                        if (logs != null)
                        {
                            // On sauvegarde dans la RAM pour les prochains clics !
                            lock (_logLock)
                            {
                                _dailyLogs[dateKey] = logs;
                            }
                            return logs;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Erreur de lecture en direct pour {dateKey}: {ex.Message}");
                }
            }

            // === 3. SOLUTION DE SECOURS ===
            // Si le fichier n'existe pas ou a planté, on regarde en mémoire au cas où
            lock (_logLock)
            {
                if (_dailyLogs.TryGetValue(dateKey, out var logs))
                {
                    return logs.ToList(); 
                }
            }

            return new List<LogEntry>();
        }

         
        /// Gets all log entries for a specific date range
         
        public async Task<List<LogEntry>> GetLogEntriesForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var result = new List<LogEntry>();
            
            // Normalize dates to midnight
            startDate = startDate.Date;
            endDate = endDate.Date;
            
            // Iterate through each date in the range
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                string dateKey = date.ToString("yyyy-MM-dd");
                
                // Check if we have the logs in memory
                List<LogEntry> dailyLogs;
                bool logsInMemory;
                
                lock (_logLock)
                {
                    logsInMemory = _dailyLogs.TryGetValue(dateKey, out var logs);
                    if (logsInMemory)
                    {
                        dailyLogs = logs.ToList(); // Get a copy
                    }
                    else
                    {
                        dailyLogs = new List<LogEntry>();
                    }
                }
                
                // If logs are not in memory, load them from disk
                if (!logsInMemory)
                {
                    dailyLogs = await LoadLogsFromDiskAsync(dateKey);
                }
                
                result.AddRange(dailyLogs);
            }
            
            return result;
        }

         
        /// Loads today's logs from disk
         
        private void LoadTodaysLogs()
        {
            string dateKey = DateTime.Now.ToString("yyyy-MM-dd");
            
            // Load JSON logs
            string jsonFilePath = GetJsonLogFilePath(dateKey);
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    var logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent, _jsonOptions);
                    
                    if (logs != null)
                    {
                        lock (_logLock)
                        {
                            _dailyLogs[dateKey] = logs;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading logs for {dateKey}: {ex.Message}");
                }
            }
        }

         
        /// Loads logs for a specific date from disk
         
        private async Task<List<LogEntry>> LoadLogsFromDiskAsync(string dateKey)
        {
            // Use SemaphoreSlim for async operations instead of lock
            await _asyncLock.WaitAsync();
            
            try
            {
                // Check if another thread has already loaded these logs while we were waiting
                lock (_logLock)
                {
                    if (_dailyLogs.TryGetValue(dateKey, out var existingLogs))
                    {
                        return existingLogs.ToList();
                    }
                }
                
                // Load JSON logs
                string jsonFilePath = GetJsonLogFilePath(dateKey);
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                        var logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent, _jsonOptions);
                        
                        if (logs != null)
                        {
                            // Store the logs in memory
                            lock (_logLock)
                            {
                                _dailyLogs[dateKey] = logs;
                            }
                            
                            return logs;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error loading logs for {dateKey}: {ex.Message}");
                    }
                }
                
                return new List<LogEntry>();
            }
            finally
            {
                _asyncLock.Release();
            }
        }

         
        /// Saves logs for a specific date to disk
         
        private async Task SaveLogsToDiskAsync(string dateKey)
        {
            List<LogEntry> logs;
            
            lock (_logLock)
            {
                if (!_dailyLogs.TryGetValue(dateKey, out var existingLogs))
                {
                    return; // No logs to save
                }
                
                // Make a copy to avoid holding the lock during I/O
                logs = existingLogs.ToList();
            }
            
            // Use SemaphoreSlim for async file operations
            await _asyncLock.WaitAsync();
            
            try
            {
                // Save to JSON
                string jsonFilePath = GetJsonLogFilePath(dateKey);
                string jsonContent = JsonSerializer.Serialize(logs, _jsonOptions);
                await File.WriteAllTextAsync(jsonFilePath, jsonContent);
                
                // Save to XML 
                string xmlFilePath = GetXmlLogFilePath(dateKey);
                await File.WriteAllTextAsync(xmlFilePath, jsonContent);
            }
            finally
            {
                _asyncLock.Release();
            }
        }

         
        /// Gets the path to the JSON log file for a specific date
         
        private string GetJsonLogFilePath(string dateKey)
        {
            return Path.Combine(_logDirectory, "Json", $"{dateKey}.json");
        }

         
        /// Gets the path to the XML log file for a specific date
         
        private string GetXmlLogFilePath(string dateKey)
        {
            return Path.Combine(_logDirectory, "Xml", $"{dateKey}.xml");
        }

         
        /// Ensures a directory exists
         
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}