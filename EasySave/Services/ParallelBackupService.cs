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
    /// <summary>
    /// Service for executing backup jobs in parallel
    /// </summary>
    public class ParallelBackupService
    {
        private readonly IEncryptionLogger _logger;
        private readonly StateManager _stateManager;
        private readonly CryptoService _cryptoService;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly BackupJobRepository _backupJobRepository; // Ajout du repository pour mettre à jour les jobs
        
        private readonly ConcurrentDictionary<string, BackupJobState> _activeJobs = new ConcurrentDictionary<string, BackupJobState>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _jobCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        
        // Utiliser un bool pour suivre l'état de pause au lieu d'un ManualResetEventSlim
        private readonly ConcurrentDictionary<string, bool> _jobPauseStates = new ConcurrentDictionary<string, bool>();
        
        // Dictionnaire pour stocker le dernier fichier traité pour chaque travail (pour la reprise)
        private readonly ConcurrentDictionary<string, string> _lastProcessedFiles = new ConcurrentDictionary<string, string>();
        
        // Sémaphore pour les fichiers volumineux - limité à 1 fichier volumineux à la fois
        private readonly SemaphoreSlim _largeFileSemaphore;
        
        private List<string> _priorityExtensions = new List<string>();
        private long _largeFileThreshold = 1024 * 1024; // 1MB default

        // Constantes pour les statuts - Utiliser les constantes du modèle JobStatus
        private const string STATUS_RUNNING = JobStatus.Running;
        private const string STATUS_PAUSED = JobStatus.Paused;
        private const string STATUS_COMPLETED = JobStatus.Completed;
        private const string STATUS_CANCELED = JobStatus.Canceled;
        private const string STATUS_STOPPING = JobStatus.Stopping;
        
        /// <summary>
        /// Event raised when a backup job's status changes
        /// </summary>
        public event EventHandler<Models.BackupJobStatusEventArgs> BackupJobStatusChanged;
        
        /// <summary>
        /// Creates a new instance of the ParallelBackupService
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="stateManager">The state manager to use</param>
        /// <param name="cryptoService">The crypto service to use</param>
        /// <param name="settingsViewModel">The settings view model</param>
        /// <param name="backupJobRepository">The repository for backup jobs</param>
        public ParallelBackupService(
            IEncryptionLogger logger, 
            StateManager stateManager, 
            CryptoService cryptoService, 
            SettingsViewModel settingsViewModel,
            BackupJobRepository backupJobRepository) // Ajout du paramètre repository
        {
            _logger = logger;
            _stateManager = stateManager;
            _cryptoService = cryptoService;
            _settingsViewModel = settingsViewModel;
            _backupJobRepository = backupJobRepository; // Initialisation du repository
            
            // Initialize the semaphore with a count of 1 (only one large file at a time by default)
            _largeFileSemaphore = new SemaphoreSlim(1, 1);
            
            // Load priority extensions from settings
            _priorityExtensions = _settingsViewModel.PriorityExtensions ?? new List<string>();
            _largeFileThreshold = _settingsViewModel.LargeFileThreshold;
        }
        
        /// <summary>
        /// Sets the list of priority extensions
        /// </summary>
        /// <param name="extensions">The list of extensions to prioritize</param>
        public void SetPriorityExtensions(List<string> extensions)
        {
            _priorityExtensions = extensions ?? new List<string>();
        }
        
        /// <summary>
        /// Sets the threshold for large files
        /// </summary>
        /// <param name="threshold">The threshold in bytes</param>
        public void SetLargeFileThreshold(long threshold)
        {
            _largeFileThreshold = threshold;
        }
        
        /// <summary>
        /// Starts a backup job asynchronously
        /// </summary>
        /// <param name="job">The backup job to start</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task StartBackupJobAsync(BackupJob job)
        {
            // Check if the job is already running
            if (_activeJobs.ContainsKey(job.JobName))
            {
                throw new InvalidOperationException($"Backup job '{job.JobName}' is already running");
            }
            
            // Create a cancellation token source for this job
            var cts = new CancellationTokenSource();
            _jobCancellationTokens[job.JobName] = cts;
            
            // Set pause state to false (not paused)
            _jobPauseStates[job.JobName] = false;
            
            // Create a state object for this job - explicitement utiliser STATUS_RUNNING
            var state = new BackupJobState
            {
                JobName = job.JobName,
                Status = STATUS_RUNNING,
                StartTime = DateTime.Now
            };
            
            // Add the job to the active jobs dictionary
            _activeJobs[job.JobName] = state;
            
            // Update the state file
            await _stateManager.UpdateStateAsync(
                job.JobName, 
                BackupState.Active, 
                0, 0, 0, 0, 
                string.Empty, 
                string.Empty);
            
            // Raise the status changed event - Utiliser STATUS_RUNNING
            OnBackupJobStatusChanged(job.JobName, STATUS_RUNNING, 0, string.Empty);
            
            // Start the backup job in a background task
            _ = Task.Run(async () =>
            {
                try
                {
                    await ExecuteBackupJobAsync(job, state, cts.Token);
                    
                    // Update the state when the job completes successfully
                    state.Status = STATUS_COMPLETED;
                    state.EndTime = DateTime.Now;
                    state.Progress = 100;
                    
                    // Update the state file
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Completed, 
                        state.TotalFiles, state.TotalSize, 
                        0, 0, 
                        string.Empty, 
                        string.Empty);
                    
                    // Raise the status changed event
                    OnBackupJobStatusChanged(job.JobName, STATUS_COMPLETED, 100, string.Empty);
                    
                    // Clear the last processed file
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                    
                    // Mettre à jour la date de dernière sauvegarde dans le job
                    UpdateLastBackupTime(job.JobName);
                }
                catch (OperationCanceledException)
                {
                    // The job was canceled
                    state.Status = STATUS_CANCELED;
                    state.EndTime = DateTime.Now;
                    
                    // Update the state file
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Canceled, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        string.Empty, 
                        string.Empty);
                    
                    // Raise the status changed event
                    OnBackupJobStatusChanged(job.JobName, STATUS_CANCELED, state.Progress, string.Empty);
                    
                    // Clear the last processed file
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                }
                catch (Exception ex)
                {
                    // The job failed
                    string errorStatus = $"Failed: {ex.Message}";
                    state.Status = errorStatus;
                    state.EndTime = DateTime.Now;
                    
                    // Update the state file
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Error, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        string.Empty, 
                        string.Empty);
                    
                    // Raise the status changed event
                    OnBackupJobStatusChanged(job.JobName, errorStatus, state.Progress, string.Empty);
                    
                    // Log the error
                    _logger.LogError($"Backup job '{job.JobName}' failed: {ex.Message}");
                    
                    // Clear the last processed file
                    _lastProcessedFiles.TryRemove(job.JobName, out _);
                }
                finally
                {
                    // Clean up
                    _activeJobs.TryRemove(job.JobName, out _);
                    _jobPauseStates.TryRemove(job.JobName, out _);
                    
                    // Dispose of the cancellation token source
                    if (_jobCancellationTokens.TryRemove(job.JobName, out var tokenSource))
                    {
                        tokenSource.Dispose();
                    }
                }
            });
        }
        
        /// <summary>
        /// Updates the last backup time for a job
        /// </summary>
        /// <param name="jobName">The name of the job</param>
        private void UpdateLastBackupTime(string jobName)
        {
            try
            {
                // Récupérer le job depuis le repository
                var job = _backupJobRepository.GetBackupJob(jobName);
                if (job != null)
                {
                    // Mettre à jour la date de dernière sauvegarde
                    job.LastBackupTime = DateTime.Now;
                    
                    // Enregistrer le job mis à jour
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
        
        /// <summary>
        /// Pauses a running backup job
        /// </summary>
        /// <param name="jobName">The name of the job to pause</param>
        /// <returns>True if the job was paused, false otherwise</returns>
        public async Task<bool> PauseJobAsync(string jobName)
        {
            Debug.WriteLine($"PauseJobAsync called for job: {jobName}");
            
            if (!_activeJobs.TryGetValue(jobName, out var state))
            {
                Debug.WriteLine($"Job {jobName} not found in active jobs");
                return false;
            }
            
            // Vérifier l'état de pause actuel dans le dictionnaire _jobPauseStates
            // plutôt que de se fier uniquement à l'état du job
            bool isPaused;
            if (_jobPauseStates.TryGetValue(jobName, out isPaused) && isPaused)
            {
                Debug.WriteLine($"Job {jobName} is already paused according to _jobPauseStates");
                return true; // Already paused
            }
            
            // Update the state immediately
            state.Status = STATUS_PAUSED;
            Debug.WriteLine($"Job {jobName} status updated to: {STATUS_PAUSED}");
            
            // Set the pause flag
            _jobPauseStates[jobName] = true;
            Debug.WriteLine($"Pause flag set for job {jobName}");
            
            // Raise the status changed event immediately - Utiliser STATUS_PAUSED pour être cohérent
            OnBackupJobStatusChanged(jobName, STATUS_PAUSED, state.Progress, state.CurrentFile);
            
            // Update the state file
            await _stateManager.UpdateStateAsync(
                jobName, 
                BackupState.Paused, 
                state.TotalFiles, state.TotalSize, 
                state.TotalFilesRemaining, state.TotalSizeRemaining, 
                state.CurrentFile, 
                state.CurrentFileDestination);
            
            return true;
        }
        
        /// <summary>
        /// Resumes a paused backup job
        /// </summary>
        /// <param name="jobName">The name of the job to resume</param>
        /// <returns>True if the job was resumed, false otherwise</returns>
        public async Task<bool> ResumeJobAsync(string jobName)
        {
            Debug.WriteLine($"ResumeJobAsync called for job: {jobName}");
            
            if (!_activeJobs.TryGetValue(jobName, out var state))
            {
                Debug.WriteLine($"Job {jobName} not found in active jobs");
                return false;
            }
            
            // Vérifier l'état de pause actuel dans le dictionnaire _jobPauseStates
            // plutôt que de se fier uniquement à l'état du job
            bool isPaused;
            if (!(_jobPauseStates.TryGetValue(jobName, out isPaused) && isPaused))
            {
                Debug.WriteLine($"Job {jobName} is not paused according to _jobPauseStates");
                return false; // Not paused
            }
            
            // Update the state immediately
            state.Status = STATUS_RUNNING;
            Debug.WriteLine($"Job {jobName} status updated to: {STATUS_RUNNING}");
            
            // Clear the pause flag
            _jobPauseStates[jobName] = false;
            Debug.WriteLine($"Pause flag cleared for job {jobName}");
            
            // Raise the status changed event immediately - Utiliser STATUS_RUNNING pour être cohérent
            OnBackupJobStatusChanged(jobName, STATUS_RUNNING, state.Progress, state.CurrentFile);
            
            // Update the state file
            await _stateManager.UpdateStateAsync(
                jobName, 
                BackupState.Active, 
                state.TotalFiles, state.TotalSize, 
                state.TotalFilesRemaining, state.TotalSizeRemaining, 
                state.CurrentFile, 
                state.CurrentFileDestination);
            
            Debug.WriteLine($"State file updated for job {jobName}");
            return true;
        }
        
        /// <summary>
        /// Stops a running backup job
        /// </summary>
        /// <param name="jobName">The name of the job to stop</param>
        /// <returns>True if the job was stopped, false otherwise</returns>
        public bool StopJob(string jobName)
        {
            Debug.WriteLine($"StopJob called for job: {jobName}");
            
            if (!_jobCancellationTokens.TryGetValue(jobName, out var cts))
            {
                Debug.WriteLine($"Cancellation token not found for job {jobName}");
                return false;
            }
            
            // Update the state
            if (_activeJobs.TryGetValue(jobName, out var state))
            {
                state.Status = STATUS_STOPPING;
                Debug.WriteLine($"Job {jobName} status updated to: {STATUS_STOPPING}");
                
                // Raise the status changed event
                OnBackupJobStatusChanged(jobName, STATUS_STOPPING, state.Progress, state.CurrentFile);
                
                // Make sure the job isn't paused when we try to cancel it
                _jobPauseStates[jobName] = false;
                Debug.WriteLine($"Pause flag cleared for job {jobName} before cancellation");
            }
            
            // Cancel the job
            cts.Cancel();
            Debug.WriteLine($"Cancellation requested for job {jobName}");
            
            // Clear the last processed file
            _lastProcessedFiles.TryRemove(jobName, out _);
            
            return true;
        }
        
        /// <summary>
        /// Gets a list of all active jobs
        /// </summary>
        /// <returns>A list of active job states</returns>
        public List<BackupJobState> GetActiveJobs()
        {
            return _activeJobs.Values.ToList();
        }
        
        /// <summary>
        /// Gets the last processed file for a job
        /// </summary>
        /// <param name="jobName">The name of the job</param>
        /// <returns>The path of the last processed file, or null if none</returns>
        public string GetLastProcessedFile(string jobName)
        {
            _lastProcessedFiles.TryGetValue(jobName, out var lastFile);
            return lastFile;
        }
        
        /// <summary>
        /// Executes a backup job asynchronously
        /// </summary>
        /// <param name="job">The backup job to execute</param>
        /// <param name="state">The state object for this job</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ExecuteBackupJobAsync(BackupJob job, BackupJobState state, CancellationToken cancellationToken)
        {
            // Validate paths
            if (!Directory.Exists(job.SourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");
            }
            
            // Create target directory if it doesn't exist
            if (!Directory.Exists(job.TargetPath))
            {
                Directory.CreateDirectory(job.TargetPath);
            }
            
            // Update state to running - Utiliser STATUS_RUNNING
            state.Status = STATUS_RUNNING;
            state.TotalFiles = 0;
            state.TotalFilesRemaining = 0;
            state.TotalSize = 0;
            state.TotalSizeRemaining = 0;
            state.Progress = 0;
            state.StartTime = DateTime.Now;
            
            // Update the state file
            await _stateManager.UpdateStateAsync(
                job.JobName, 
                BackupState.Active, 
                0, 0, 0, 0, 
                string.Empty, 
                string.Empty);
            
            // Raise the status changed event - Utiliser STATUS_RUNNING
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
                
                // Sort files by priority (priority extensions first)
                var priorityFiles = allFiles
                    .Where(f => _priorityExtensions.Contains(f.Extension.ToLowerInvariant()))
                    .ToList();
                
                var normalFiles = allFiles
                    .Where(f => !_priorityExtensions.Contains(f.Extension.ToLowerInvariant()))
                    .ToList();
                
                var sortedFiles = priorityFiles.Concat(normalFiles).ToList();
                
                // Check if we have a last processed file to resume from
                string lastProcessedFile;
                _lastProcessedFiles.TryGetValue(job.JobName, out lastProcessedFile);
                
                int startIndex = 0;
                long totalProcessedSize = 0;
                
                if (!string.IsNullOrEmpty(lastProcessedFile))
                {
                    // Find the index of the last processed file
                    startIndex = sortedFiles.FindIndex(f => f.FullName == lastProcessedFile) + 1;
                    if (startIndex > 0)
                    {
                        Debug.WriteLine($"Resuming job {job.JobName} from file index {startIndex} ({lastProcessedFile})");
                        
                        // Update progress based on what's already been processed
                        totalProcessedSize = sortedFiles.Take(startIndex).Sum(f => f.Length);
                        state.Progress = (int)((double)totalProcessedSize / state.TotalSize * 100);
                        state.TotalFilesRemaining = state.TotalFiles - startIndex;
                        state.TotalSizeRemaining = state.TotalSize - totalProcessedSize;
                    }
                    else
                    {
                        Debug.WriteLine($"Last processed file {lastProcessedFile} not found, starting from beginning");
                        startIndex = 0;
                    }
                }
                
                // Process each file
                int processedFiles = startIndex;
                
                for (int i = startIndex; i < sortedFiles.Count; i++)
                {
                    var file = sortedFiles[i];
                    
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Check if paused - Utiliser le dictionnaire _jobPauseStates pour vérifier l'état de pause
                    bool isPaused = false;
                    while (_jobPauseStates.TryGetValue(job.JobName, out isPaused) && isPaused)
                    {
                        // Store the current file for resuming later
                        _lastProcessedFiles[job.JobName] = file.FullName;
                        Debug.WriteLine($"Job {job.JobName} is paused, last file: {file.FullName}");
                        
                        // Job is paused, wait a short time and check again
                        Debug.WriteLine($"Job {job.JobName} is paused, waiting...");
                        await Task.Delay(100, cancellationToken);
                        
                        // Check for cancellation again after delay
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    
                    // Double-check that we're not paused before continuing
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
                    
                    // Store the current file for resuming later - Mise à jour à chaque fichier
                    _lastProcessedFiles[job.JobName] = file.FullName;
                    
                    // Update the state file with current file information
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Active, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        state.CurrentFile, 
                        state.CurrentFileDestination);
                    
                    // Raise the status changed event with updated progress and current file
                    OnBackupJobStatusChanged(job.JobName, state.Status, state.Progress, state.CurrentFile);
                    
                    // Create target directory if it doesn't exist
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    // Determine if we need to copy the file based on job type
                    bool shouldCopy = job.Type == BackupType.Complete;
                    
                    if (job.Type == BackupType.Differential)
                    {
                        // For differential backup, only copy if the file doesn't exist or has been modified
                        if (!File.Exists(targetPath) || file.LastWriteTime > File.GetLastWriteTime(targetPath))
                        {
                            shouldCopy = true;
                        }
                    }
                    
                    if (shouldCopy)
                    {
                        // Check if this is a large file
                        bool isLargeFile = file.Length > _largeFileThreshold;
                        
                        try
                        {
                            // Si c'est un fichier volumineux, nous devons acquérir le sémaphore
                            if (isLargeFile)
                            {
                                Debug.WriteLine($"Job {job.JobName}: Large file detected ({file.Name}, {file.Length} bytes)");
                                
                                // Essayer d'acquérir le sémaphore sans bloquer d'abord
                                bool acquired = false;
                                
                                // Boucle d'attente pour acquérir le sémaphore
                                while (!acquired)
                                {
                                    // Vérifier l'annulation
                                    cancellationToken.ThrowIfCancellationRequested();
                                    
                                    // Vérifier si le job est en pause
                                    if (_jobPauseStates.TryGetValue(job.JobName, out isPaused) && isPaused)
                                    {
                                        // Attendre un peu et réessayer
                                        await Task.Delay(100, cancellationToken);
                                        continue;
                                    }
                                    
                                    // Essayer d'acquérir le sémaphore sans bloquer
                                    acquired = await _largeFileSemaphore.WaitAsync(0);
                                    
                                    // Si on n'a pas pu acquérir le sémaphore, attendre un peu et réessayer
                                    if (!acquired)
                                    {
                                        Debug.WriteLine($"Job {job.JobName}: Waiting for large file semaphore for file {file.Name}");
                                        await Task.Delay(500, cancellationToken);
                                    }
                                }
                                
                                Debug.WriteLine($"Job {job.JobName}: Acquired large file semaphore for file {file.Name}");
                                
                                // Copier le fichier volumineux
                                var stopwatch = Stopwatch.StartNew();
                                File.Copy(file.FullName, targetPath, true);
                                stopwatch.Stop();
                                
                                // Check if the file should be encrypted
                                long encryptionTime = 0;
                                if (_settingsViewModel.ShouldEncryptFile(file.FullName))
                                {
                                    var encryptStopwatch = Stopwatch.StartNew();
                                    await _cryptoService.EncryptFileAsync(targetPath);
                                    encryptStopwatch.Stop();
                                    encryptionTime = encryptStopwatch.ElapsedMilliseconds;
                                }
                                
                                // Log the file copy
                                _logger.LogFileTransfer(job.JobName, file.FullName, targetPath, file.Length, stopwatch.ElapsedMilliseconds, encryptionTime);
                                
                                // Libérer le sémaphore
                                _largeFileSemaphore.Release();
                                Debug.WriteLine($"Job {job.JobName}: Released large file semaphore for file {file.Name}");
                            }
                            else
                            {
                                // Pour les fichiers normaux, pas besoin d'acquérir le sémaphore
                                var stopwatch = Stopwatch.StartNew();
                                File.Copy(file.FullName, targetPath, true);
                                stopwatch.Stop();
                                
                                // Check if the file should be encrypted
                                long encryptionTime = 0;
                                if (_settingsViewModel.ShouldEncryptFile(file.FullName))
                                {
                                    var encryptStopwatch = Stopwatch.StartNew();
                                    await _cryptoService.EncryptFileAsync(targetPath);
                                    encryptStopwatch.Stop();
                                    encryptionTime = encryptStopwatch.ElapsedMilliseconds;
                                }
                                
                                // Log the file copy
                                _logger.LogFileTransfer(job.JobName, file.FullName, targetPath, file.Length, stopwatch.ElapsedMilliseconds, encryptionTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            // En cas d'erreur, s'assurer que le sémaphore est libéré si c'était un fichier volumineux
                            if (isLargeFile)
                            {
                                try
                                {
                                    _largeFileSemaphore.Release();
                                    Debug.WriteLine($"Job {job.JobName}: Released large file semaphore after error for file {file.Name}");
                                }
                                catch (SemaphoreFullException)
                                {
                                    // Le sémaphore était déjà libéré, ignorer
                                }
                            }
                            
                            // Journaliser l'erreur
                            _logger.LogError($"Error copying file {file.FullName} to {targetPath}: {ex.Message}");
                            
                            // Relancer l'exception pour qu'elle soit gérée par le bloc try/catch englobant
                            throw;
                        }
                    }
                    
                    // Update progress
                    processedFiles++;
                    totalProcessedSize += file.Length;
                    state.TotalFilesRemaining = state.TotalFiles - processedFiles;
                    state.TotalSizeRemaining = state.TotalSize - totalProcessedSize;
                    state.Progress = (int)((double)totalProcessedSize / state.TotalSize * 100);
                    
                    // Update the state file with progress information
                    await _stateManager.UpdateStateAsync(
                        job.JobName, 
                        BackupState.Active, 
                        state.TotalFiles, state.TotalSize, 
                        state.TotalFilesRemaining, state.TotalSizeRemaining, 
                        state.CurrentFile, 
                        state.CurrentFileDestination);
                    
                    // Raise the status changed event with updated progress and current file
                    OnBackupJobStatusChanged(job.JobName, state.Status, state.Progress, state.CurrentFile);
                }
                
                // Log completion
                _logger.LogBackupComplete(job.JobName, job.SourcePath, job.TargetPath, state.TotalFiles, state.TotalSize);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExecuteBackupJobAsync for job {job.JobName}: {ex.Message}");
                throw; // Re-throw to be handled by the caller
            }
        }
        
        /// <summary>
        /// Raises the BackupJobStatusChanged event
        /// </summary>
        /// <param name="jobName">The name of the job</param>
        /// <param name="status">The status of the job</param>
        /// <param name="progress">The progress percentage</param>
        /// <param name="currentFile">The current file being processed</param>
        private void OnBackupJobStatusChanged(string jobName, string status, int progress, string currentFile)
        {
            Debug.WriteLine($"Raising status changed event for job {jobName}: {status}, progress: {progress}, file: {currentFile}");
            BackupJobStatusChanged?.Invoke(this, new Models.BackupJobStatusEventArgs(jobName, status, progress, currentFile));
        }
    }
}