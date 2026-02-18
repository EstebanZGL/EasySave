using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyLog
{
    /// <summary>
    /// Logger that sends log entries to a centralized server
    /// </summary>
    public class RemoteLogger : IEncryptionLogger
    {
        private readonly string _serverUrl;
        private readonly HttpClient _httpClient;
        private readonly string _machineName;
        private readonly string _userName;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IEncryptionLogger _fallbackLogger;
        
        /// <summary>
        /// Creates a new instance of the RemoteLogger
        /// </summary>
        /// <param name="serverUrl">The URL of the log server</param>
        /// <param name="fallbackLogger">A fallback logger to use if the server is unavailable</param>
        public RemoteLogger(string serverUrl, IEncryptionLogger fallbackLogger)
        {
            _serverUrl = serverUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(serverUrl));
            _fallbackLogger = fallbackLogger ?? throw new ArgumentNullException(nameof(fallbackLogger));
            
            _httpClient = new HttpClient();
            _machineName = Environment.MachineName;
            _userName = Environment.UserName;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
        
        /// <summary>
        /// Logs a file transfer operation
        /// </summary>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="targetPath">Target file path</param>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <param name="transferTime">Transfer time in milliseconds (negative if error)</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            try
            {
                var logEntry = new
                {
                    BackupName = backupName,
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    FileSize = fileSize,
                    TransferTime = transferTime,
                    EncryptionTime = 0L,
                    MachineName = _machineName,
                    UserName = _userName,
                    LogType = "Transfer",
                    Timestamp = DateTime.Now
                };
                
                await SendLogToServerAsync(logEntry);
            }
            catch (Exception ex)
            {
                // If sending to the server fails, use the fallback logger
                Console.Error.WriteLine($"Error sending log to server: {ex.Message}");
                await _fallbackLogger.LogTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime);
            }
        }
        
        /// <summary>
        /// Logs a file transfer operation with encryption time
        /// </summary>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="targetPath">Target file path</param>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <param name="transferTime">Transfer time in milliseconds (negative if error)</param>
        /// <param name="encryptionTime">Encryption time in milliseconds (0 if not encrypted, negative if error)</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LogEncryptedTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime, long encryptionTime)
        {
            try
            {
                var logEntry = new
                {
                    BackupName = backupName,
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    FileSize = fileSize,
                    TransferTime = transferTime,
                    EncryptionTime = encryptionTime,
                    MachineName = _machineName,
                    UserName = _userName,
                    LogType = "EncryptedTransfer",
                    Timestamp = DateTime.Now
                };
                
                await SendLogToServerAsync(logEntry);
            }
            catch (Exception ex)
            {
                // If sending to the server fails, use the fallback logger
                Console.Error.WriteLine($"Error sending log to server: {ex.Message}");
                await _fallbackLogger.LogEncryptedTransferAsync(backupName, sourcePath, targetPath, fileSize, transferTime, encryptionTime);
            }
        }
        
        /// <summary>
        /// Logs an application event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="details">Event details</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LogApplicationEventAsync(string eventName, string details)
        {
            try
            {
                var logEntry = new
                {
                    BackupName = string.Empty,
                    SourcePath = string.Empty,
                    TargetPath = string.Empty,
                    FileSize = 0L,
                    TransferTime = 0L,
                    EncryptionTime = 0L,
                    MachineName = _machineName,
                    UserName = _userName,
                    LogType = "ApplicationEvent",
                    Details = $"{eventName}: {details}",
                    Timestamp = DateTime.Now
                };
                
                await SendLogToServerAsync(logEntry);
            }
            catch (Exception ex)
            {
                // If sending to the server fails, use the fallback logger
                Console.Error.WriteLine($"Error sending log to server: {ex.Message}");
                await _fallbackLogger.LogApplicationEventAsync(eventName, details);
            }
        }
        
        /// <summary>
        /// Logs a backup operation
        /// </summary>
        /// <param name="jobName">Name of the backup job</param>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="targetPath">Target directory path</param>
        /// <param name="fileSize">Total size of files in bytes</param>
        /// <param name="transferTime">Total transfer time in milliseconds</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LogBackupOperationAsync(string jobName, string sourcePath, string targetPath, long fileSize, long transferTime)
        {
            try
            {
                var logEntry = new
                {
                    BackupName = jobName,
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    FileSize = fileSize,
                    TransferTime = transferTime,
                    EncryptionTime = 0L,
                    MachineName = _machineName,
                    UserName = _userName,
                    LogType = "BackupOperation",
                    Timestamp = DateTime.Now
                };
                
                await SendLogToServerAsync(logEntry);
            }
            catch (Exception ex)
            {
                // If sending to the server fails, use the fallback logger
                Console.Error.WriteLine($"Error sending log to server: {ex.Message}");
                await _fallbackLogger.LogBackupOperationAsync(jobName, sourcePath, targetPath, fileSize, transferTime);
            }
        }
        
        /// <summary>
        /// Sends a log entry to the server
        /// </summary>
        /// <param name="logEntry">The log entry to send</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task SendLogToServerAsync(object logEntry)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/logs", logEntry);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Server returned status code {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send log to server: {ex.Message}", ex);
            }
        }
    }
}