using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyLog;
using EasySave.Models;
using EasySave.ViewModels;

namespace EasySave.Services
{
    public class ParallelBackupService
    {
        private readonly IEncryptionLogger _logger;
        private readonly StateManager _stateManager;
        private readonly CryptoService _cryptoService;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly BackupJobRepository _backupJobRepository;
        
        private readonly ConcurrentDictionary<string, BackupJobState> _activeJobs = new ConcurrentDictionary<string, BackupJobState>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _jobCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly ConcurrentDictionary<string, bool> _jobPauseStates = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, string> _lastProcessedFiles = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, int> _jobPendingPriorityTransfers = new ConcurrentDictionary<string, int>();
        private int _globalPendingPriorityTransfers;
        
        // Limits large file transfers to one at a time
        private readonly SemaphoreSlim _largeFileSemaphore;
        
        private List<string> _priorityExtensions = new List<string>();
        private long _largeFileThreshold = 1024 * 1024; // 1MB default

        // Job status constants
        private const string STATUS_RUNNING = JobStatus.Running;
        private const string STATUS_PAUSED = JobStatus.Paused;
        private const string STATUS_COMPLETED = JobStatus.Completed;
        private const string STATUS_CANCELED = JobStatus.Canceled;
        private const string STATUS_STOPPING = JobStatus.Stopping;
        
        public event EventHandler<Models.BackupJobStatusEventArgs> BackupJobStatusChanged;
        
        public ParallelBackupService(
            IEncryptionLogger logger, 
            StateManager stateManager, 
            CryptoService cryptoService, 
            SettingsViewModel settingsViewModel,
            BackupJobRepository backupJobRepository)
        {
            _logger = logger;
            _stateManager = stateManager;
            _cryptoService = cryptoService;
            _settingsViewModel = settingsViewModel;
            _backupJobRepository = backupJobRepository;
            
            _largeFileSemaphore = new SemaphoreSlim(1, 1);
            _priorityExtensions = _settingsViewModel.PriorityExtensions ?? new List<string>();
            _largeFileThreshold = _settingsViewModel.LargeFileThreshold;
        }
        
        public void SetPriorityExtensions(List<string> extensions)
        {
            _priorityExtensions = extensions ?? new List<string>();
        }
        
        public void SetLargeFileThreshold(long threshold)
        {
            _largeFileThreshold = threshold;
        }
        
        public async Task StartBackupJobAsync(BackupJob job)
        {
            // Check if job is already running
            if (_activeJobs.ContainsKey(job.JobName))
            {
                throw new InvalidOperationException($"Backup job '{job.JobName}' is already running");
            }
            
            // Create cancellation token source for this job
            var cts = new CancellationTokenSource();
            _jobCancellationTokens[job.JobName] = cts;
            _jobPauseStates[job.JobName] = false;
            
            // Create job state object
            var state = new BackupJobState
            {
                JobName = job.JobName,
                Status = STATUS_RUNNING,
                StartTime = DateTime.Now
            };
            
            _activeJobs[job.JobName] = state;
            
            await _stateManager.UpdateStateAsync(
                job.JobName, 
                BackupState.Active, 
                0, 0, 0, 0, 
                string.Empty, 
                string.Empty);
            
            OnBackupJobStatusChanged(job.JobName, STATUS_RUNNING, 0, string.Empty);
            
            #pragma warning disable CS4014 // Execution continues before task completes
            Task.Run(async () =>
            {
                try
                {
                    await ExecuteBackupJobAsync(job, state, cts.Token);
                    
                    // Update state on successful completion
                    state.Status = STATUS_COMPLETED;
                    state.EndTime = DateTime.Now;
                    state.Progress = 100;
                    
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Completed, 
                        state.TotalFiles, state.TotalSize, 
                        0, 0, 
                        string.Empty, 
                        string.Empty);
                    
                    OnBackupJobStatusChanged(job.JobName, STATUS_COMPLETED, 100, string.Empty);
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                    UpdateLastBackupTime(job.JobName);
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation
                    state.Status = STATUS_CANCELED;
                    state.EndTime = DateTime.Now;
                    
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Canceled, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        string.Empty, 
                        string.Empty);
                    
                    OnBackupJobStatusChanged(job.JobName, STATUS_CANCELED, state.Progress, string.Empty);
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                }
                catch (Exception ex)
                {
                    // Handle failure
                    string errorStatus = $"Failed: {ex.Message}";
                    state.Status = errorStatus;
                    state.EndTime = DateTime.Now;
                    
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Error, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        string.Empty, 
                        string.Empty);
                    
                    OnBackupJobStatusChanged(job.JobName, errorStatus, state.Progress, string.Empty);
                    await _logger.LogApplicationEventAsync("Error", $"Backup job '{job.JobName}' failed: {ex.Message}");
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                }
                finally
                {
                    ReleaseRemainingPriorityTransfers(job.JobName);

                    // Clean up resources
                    _activeJobs.TryRemove(job.JobName, out _);
                    _jobPauseStates.TryRemove(job.JobName, out _);
                    
                    if (_jobCancellationTokens.TryRemove(job.JobName, out var tokenSource))
                    {
                        tokenSource.Dispose();
                    }
                }
            });
            #pragma warning restore CS4014
        }
        
        private void UpdateLastBackupTime(string jobName)
        {
            try
            {
                var job = _backupJobRepository.GetBackupJob(jobName);
                if (job != null)
                {
                    job.LastBackupTime = DateTime.Now;
                    _backupJobRepository.UpdateBackupJob(job);
                    Debug.WriteLine($"Updated last backup time for job {jobName} to {job.LastBackupTime}");
                }
                else
                {
                    Debug.WriteLine($"Job {jobName} not found in repository, could not update last backup time");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating last backup time for job {jobName}: {ex.Message}");
            }
        }
        
        public async Task<bool> PauseJobAsync(string jobName)
        {
            Debug.WriteLine($"PauseJobAsync called for job: {jobName}");
            
            if (!_activeJobs.TryGetValue(jobName, out var state))
            {
                Debug.WriteLine($"Job {jobName} not found in active jobs");
                return false;
            }
            
            // Check if already paused
            bool isPaused;
            if (_jobPauseStates.TryGetValue(jobName, out isPaused) && isPaused)
            {
                Debug.WriteLine($"Job {jobName} is already paused");
                return true;
            }
            
            state.Status = STATUS_PAUSED;
            _jobPauseStates[jobName] = true;
            
            OnBackupJobStatusChanged(jobName, STATUS_PAUSED, state.Progress, state.CurrentFile);
            
            await _stateManager.UpdateStateAsync(
                jobName, 
                BackupState.Paused, 
                state.TotalFiles, state.TotalSize, 
                state.TotalFilesRemaining, state.TotalSizeRemaining, 
                state.CurrentFile, 
                state.CurrentFileDestination);
            
            return true;
        }
        
        public async Task<bool> ResumeJobAsync(string jobName)
        {
            Debug.WriteLine($"ResumeJobAsync called for job: {jobName}");
            
            if (!_activeJobs.TryGetValue(jobName, out var state))
            {
                Debug.WriteLine($"Job {jobName} not found in active jobs");
                return false;
            }
            
            // Check if paused
            bool isPaused;
            if (!(_jobPauseStates.TryGetValue(jobName, out isPaused) && isPaused))
            {
                Debug.WriteLine($"Job {jobName} is not paused");
                return false;
            }
            
            state.Status = STATUS_RUNNING;
            _jobPauseStates[jobName] = false;
            
            OnBackupJobStatusChanged(jobName, STATUS_RUNNING, state.Progress, state.CurrentFile);
            
            await _stateManager.UpdateStateAsync(
                jobName, 
                BackupState.Active, 
                state.TotalFiles, state.TotalSize, 
                state.TotalFilesRemaining, state.TotalSizeRemaining, 
                state.CurrentFile, 
                state.CurrentFileDestination);
            
            return true;
        }
        
        public bool StopJob(string jobName)
        {
            Debug.WriteLine($"StopJob called for job: {jobName}");
            
            if (!_jobCancellationTokens.TryGetValue(jobName, out var cts))
            {
                Debug.WriteLine($"Cancellation token not found for job {jobName}");
                return false;
            }
            
            if (_activeJobs.TryGetValue(jobName, out var state))
            {
                state.Status = STATUS_STOPPING;
                OnBackupJobStatusChanged(jobName, STATUS_STOPPING, state.Progress, state.CurrentFile);
                _jobPauseStates[jobName] = false;
            }
            
            cts.Cancel();
            _lastProcessedFiles.TryRemove(jobName, out _);
            
            return true;
        }
        
        public List<BackupJobState> GetActiveJobs()
        {
            return _activeJobs.Values.ToList();
        }
        
        public string GetLastProcessedFile(string jobName)
        {
            _lastProcessedFiles.TryGetValue(jobName, out var lastFile);
            return lastFile;
        }
        
        private async Task ExecuteBackupJobAsync(BackupJob job, BackupJobState state, CancellationToken cancellationToken)
        {
            // Validate paths
            if (!Directory.Exists(job.SourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");
            }
            
            // Create target directory if needed
            if (!Directory.Exists(job.TargetPath))
            {
                Directory.CreateDirectory(job.TargetPath);
            }
            
            // Initialize state
            state.Status = STATUS_RUNNING;
            state.TotalFiles = 0;
            state.TotalFilesRemaining = 0;
            state.TotalSize = 0;
            state.TotalSizeRemaining = 0;
            state.Progress = 0;
            state.StartTime = DateTime.Now;
            
            await _stateManager.UpdateStateAsync(
                job.JobName, 
                BackupState.Active, 
                0, 0, 0, 0, 
                string.Empty, 
                string.Empty);
            
            OnBackupJobStatusChanged(job.JobName, STATUS_RUNNING, 0, string.Empty);
            
            try
            {
                // Get all files to backup
                var sourceDir = new DirectoryInfo(job.SourcePath);
                var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);
                
                // Calculate total size and count
                state.TotalFiles = allFiles.Length;
                state.TotalSize = allFiles.Sum(f => f.Length);
                state.TotalFilesRemaining = state.TotalFiles;
                state.TotalSizeRemaining = state.TotalSize;
                
                // Sort files by priority
                var priorityFiles = allFiles
                    .Where(f => _priorityExtensions.Contains(f.Extension.ToLowerInvariant()))
                    .ToList();
                
                var normalFiles = allFiles
                    .Where(f => !_priorityExtensions.Contains(f.Extension.ToLowerInvariant()))
                    .ToList();
                
                var sortedFiles = priorityFiles.Concat(normalFiles).ToList();
                RegisterPriorityTransfersForJob(job, priorityFiles);
                
                // Check for resume point
                string lastProcessedFile;
                _lastProcessedFiles.TryGetValue(job.JobName, out lastProcessedFile);
                
                int startIndex = 0;
                long totalProcessedSize = 0;
                
                if (!string.IsNullOrEmpty(lastProcessedFile))
                {
                    // Find index of last processed file
                    startIndex = sortedFiles.FindIndex(f => f.FullName == lastProcessedFile) + 1;
                    if (startIndex > 0)
                    {
                        Debug.WriteLine($"Resuming job {job.JobName} from file index {startIndex}");
                        
                        // Update progress based on processed files
                        totalProcessedSize = sortedFiles.Take(startIndex).Sum(f => f.Length);
                        state.Progress = (int)((double)totalProcessedSize / state.TotalSize * 100);
                        state.TotalFilesRemaining = state.TotalFiles - startIndex;
                        state.TotalSizeRemaining = state.TotalSize - totalProcessedSize;
                    }
                    else
                    {
                        startIndex = 0;
                    }
                }
                
                // Process each file
                int processedFiles = startIndex;
                
                for (int i = startIndex; i < sortedFiles.Count; i++)
                {
                    var file = sortedFiles[i];
                    
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Handle pause state
                    bool isPaused = false;
                    while (_jobPauseStates.TryGetValue(job.JobName, out isPaused) && isPaused)
                    {
                        _lastProcessedFiles[job.JobName] = file.FullName;
                        await Task.Delay(100, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    
                    // Double-check pause state
                    if (_jobPauseStates.TryGetValue(job.JobName, out isPaused) && isPaused)
                    {
                        i--; // Retry this file
                        continue;
                    }
                    
                    // Get relative path to maintain directory structure
                    string relativePath = file.FullName.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                    string targetPath = Path.Combine(job.TargetPath, relativePath);
                    string targetDir = Path.GetDirectoryName(targetPath);
                    
                    // Update current file in state
                    state.CurrentFile = file.FullName;
                    state.CurrentFileDestination = targetPath;
                    _lastProcessedFiles[job.JobName] = file.FullName;
                    
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Active, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        state.CurrentFile, 
                        state.CurrentFileDestination);
                    
                    OnBackupJobStatusChanged(job.JobName, state.Status, state.Progress, state.CurrentFile);
                    
                    // Create target directory if needed
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    bool isPriorityFile = _priorityExtensions.Contains(file.Extension.ToLowerInvariant());
                    bool shouldCopy = ShouldCopyFile(job, file, targetPath);
                    
                    if (shouldCopy)
                    {
                        if (!isPriorityFile)
                        {
                            await WaitForPriorityBarrierAsync(job.JobName, cancellationToken);
                        }

                        // Check if this is a large file
                        bool isLargeFile = file.Length > _largeFileThreshold;
                        
                        try
                        {
                            // For large files, acquire semaphore
                            if (isLargeFile)
                            {
                                Debug.WriteLine($"Job {job.JobName}: Large file detected ({file.Name}, {file.Length} bytes)");
                                
                                bool acquired = false;
                                
                                // Wait loop for semaphore
                                while (!acquired)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    
                                    if (_jobPauseStates.TryGetValue(job.JobName, out isPaused) && isPaused)
                                    {
                                        await Task.Delay(100, cancellationToken);
                                        continue;
                                    }
                                    
                                    acquired = await _largeFileSemaphore.WaitAsync(0);
                                    
                                    if (!acquired)
                                    {
                                        Debug.WriteLine($"Job {job.JobName}: Waiting for large file semaphore");
                                        await Task.Delay(500, cancellationToken);
                                    }
                                }
                                
                                // Copy and encrypt large file
                                var stopwatch = Stopwatch.StartNew();
                                File.Copy(file.FullName, targetPath, true);
                                stopwatch.Stop();
                                
                                long encryptionTime = 0;
                                if (_settingsViewModel.ShouldEncryptFile(file.FullName))
                                {
                                    var encryptStopwatch = Stopwatch.StartNew();
                                    await _cryptoService.EncryptFileAsync(targetPath);
                                    encryptStopwatch.Stop();
                                    encryptionTime = encryptStopwatch.ElapsedMilliseconds;
                                }
                                
                                await _logger.LogEncryptedTransferAsync(job.JobName, file.FullName, targetPath, file.Length, stopwatch.ElapsedMilliseconds, encryptionTime);
                                
                                _largeFileSemaphore.Release();
                            }
                            else
                            {
                                // For normal files, no semaphore needed
                                var stopwatch = Stopwatch.StartNew();
                                File.Copy(file.FullName, targetPath, true);
                                stopwatch.Stop();
                                
                                long encryptionTime = 0;
                                if (_settingsViewModel.ShouldEncryptFile(file.FullName))
                                {
                                    var encryptStopwatch = Stopwatch.StartNew();
                                    await _cryptoService.EncryptFileAsync(targetPath);
                                    encryptStopwatch.Stop();
                                    encryptionTime = encryptStopwatch.ElapsedMilliseconds;
                                }
                                
                                await _logger.LogEncryptedTransferAsync(job.JobName, file.FullName, targetPath, file.Length, stopwatch.ElapsedMilliseconds, encryptionTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ensure semaphore is released on error
                            if (isLargeFile)
                            {
                                try
                                {
                                    _largeFileSemaphore.Release();
                                }
                                catch (SemaphoreFullException)
                                {
                                    // Semaphore was already released
                                }
                            }
                            
                            await _logger.LogApplicationEventAsync("Error", $"Error copying file {file.FullName} to {targetPath}: {ex.Message}");
                            throw;
                        }
                        finally
                        {
                            if (isPriorityFile)
                            {
                                MarkPriorityTransferCompleted(job.JobName);
                            }
                        }
                    }
                    
                    // Update progress
                    processedFiles++;
                    totalProcessedSize += file.Length;
                    state.TotalFilesRemaining = state.TotalFiles - processedFiles;
                    state.TotalSizeRemaining = state.TotalSize - totalProcessedSize;
                    state.Progress = (int)((double)totalProcessedSize / state.TotalSize * 100);
                    
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Active, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        state.CurrentFile, 
                        state.CurrentFileDestination);
                    
                    OnBackupJobStatusChanged(job.JobName, state.Status, state.Progress, state.CurrentFile);
                }
                
                await _logger.LogApplicationEventAsync("BackupComplete", 
                    $"Job {job.JobName} completed successfully. Files: {state.TotalFiles}, Size: {state.TotalSize} bytes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExecuteBackupJobAsync for job {job.JobName}: {ex.Message}");
                throw;
            }
        }
        
        private bool ShouldCopyFile(BackupJob job, FileInfo file, string targetPath)
        {
            if (job.Type == BackupType.Complete)
            {
                return true;
            }

            // Differential backup: copy only new or modified files
            return !File.Exists(targetPath) || file.LastWriteTime > File.GetLastWriteTime(targetPath);
        }

        private void RegisterPriorityTransfersForJob(BackupJob job, List<FileInfo> priorityFiles)
        {
            int transferablePriorityCount = 0;
            foreach (var file in priorityFiles)
            {
                string relativePath = file.FullName.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                string targetPath = Path.Combine(job.TargetPath, relativePath);
                if (ShouldCopyFile(job, file, targetPath))
                {
                    transferablePriorityCount++;
                }
            }

            _jobPendingPriorityTransfers[job.JobName] = transferablePriorityCount;
            if (transferablePriorityCount > 0)
            {
                Interlocked.Add(ref _globalPendingPriorityTransfers, transferablePriorityCount);
            }
        }

        private void MarkPriorityTransferCompleted(string jobName)
        {
            if (!_jobPendingPriorityTransfers.TryGetValue(jobName, out var remaining) || remaining <= 0)
            {
                return;
            }

            _jobPendingPriorityTransfers[jobName] = remaining - 1;
            Interlocked.Decrement(ref _globalPendingPriorityTransfers);
        }

        private async Task WaitForPriorityBarrierAsync(string jobName, CancellationToken cancellationToken)
        {
            while (Volatile.Read(ref _globalPendingPriorityTransfers) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }

        private void ReleaseRemainingPriorityTransfers(string jobName)
        {
            if (_jobPendingPriorityTransfers.TryRemove(jobName, out int remaining) && remaining > 0)
            {
                Interlocked.Add(ref _globalPendingPriorityTransfers, -remaining);
            }
        }

        private void OnBackupJobStatusChanged(string jobName, string status, int progress, string currentFile)
        {
            Debug.WriteLine($"Raising status changed event for job {jobName}: {status}, progress: {progress}, file: {currentFile}");
            BackupJobStatusChanged?.Invoke(this, new Models.BackupJobStatusEventArgs(jobName, status, progress, currentFile));
        }
    }
}