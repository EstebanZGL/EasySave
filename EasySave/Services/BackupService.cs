using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.ViewModels;
using EasyLog;

namespace EasySave.Services
{
    // Service for executing backup operations
    public class BackupService
    {
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;
        private readonly SettingsViewModel _settings;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPaused;
        private readonly object _pauseLock = new object();

        // Constructor that initializes the logger and state manager
        public BackupService(ILogger logger, StateManager stateManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _settings = new SettingsViewModel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Executes a backup job asynchronously
        // Throws ArgumentException if job is invalid
        // Throws DirectoryNotFoundException if source directory doesn't exist
        public async Task ExecuteBackupJobAsync(BackupJob job)
        {
            // Validate job
            if (job == null || !job.Validate())
            {
                throw new ArgumentException("Invalid backup job", nameof(job));
            }

            // Check if business software is running
            if (_settings.IsBusinessSoftwareRunning())
            {
                await _logger.LogApplicationEventAsync(
                    "BackupCancelled",
                    $"Backup job {job.JobName} cancelled because business software is running: {_settings.BusinessSoftwareName}");
                throw new InvalidOperationException($"Cannot start backup: Business software ({_settings.BusinessSoftwareName}) is running.");
            }

            Debug.WriteLine($"Starting backup job: {job.JobName}");

            try
            {
                // Reset cancellation token
                _cancellationTokenSource = new CancellationTokenSource();
                _isPaused = false;

                // Check source directory
                if (!Directory.Exists(job.SourcePath))
                {
                    throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");
                }

                // Create target directory if needed
                if (!Directory.Exists(job.TargetPath))
                {
                    Directory.CreateDirectory(job.TargetPath);
                }

                // Get all source files
                var sourceFiles = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);
                long totalSize = sourceFiles.Sum(f => new FileInfo(f).Length);

                // Initialize state
                await _stateManager.UpdateStateAsync(
                    job.JobName,
                    BackupState.Active,
                    sourceFiles.Length,
                    totalSize,
                    sourceFiles.Length,
                    totalSize
                );

                // Process each file
                int processedCount = 0;
                long processedSize = 0;
                
                foreach (string sourceFile in sourceFiles)
                {
                    // Check for cancellation
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await _stateManager.UpdateStateAsync(
                            job.JobName,
                            BackupState.Cancelled,
                            sourceFiles.Length,
                            totalSize,
                            sourceFiles.Length - processedCount,
                            totalSize - processedSize
                        );
                        return;
                    }

                    // Check for pause
                    while (_isPaused)
                    {
                        await Task.Delay(500);
                        
                        // Also check for cancellation while paused
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await _stateManager.UpdateStateAsync(
                                job.JobName,
                                BackupState.Cancelled,
                                sourceFiles.Length,
                                totalSize,
                                sourceFiles.Length - processedCount,
                                totalSize - processedSize
                            );
                            return;
                        }
                    }

                    // Check if business software started during backup
                    if (_settings.IsBusinessSoftwareRunning())
                    {
                        // Complete the current file but then pause
                        _isPaused = true;
                        await _logger.LogApplicationEventAsync(
                            "BackupPaused",
                            $"Backup job {job.JobName} paused because business software started: {_settings.BusinessSoftwareName}");
                    }

                    // Get relative path - optimization: use Path methods instead of string operations
                    string relativePath = Path.GetRelativePath(job.SourcePath, sourceFile);
                    string targetFile = Path.Combine(job.TargetPath, relativePath);

                    // Create target directory if needed
                    string targetDirectory = Path.GetDirectoryName(targetFile);
                    if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Get file info
                    var sourceFileInfo = new FileInfo(sourceFile);
                    long fileSize = sourceFileInfo.Length;

                    // Update state
                    await _stateManager.UpdateStateAsync(
                        job.JobName,
                        BackupState.Active,
                        sourceFiles.Length,
                        totalSize,
                        sourceFiles.Length - processedCount,
                        totalSize - processedSize,
                        sourceFile,
                        targetFile
                    );

                    // Check if file needs to be copied (for differential backup)
                    bool shouldCopy = true;
                    if (job.Type == BackupType.Differential && File.Exists(targetFile))
                    {
                        var targetFileInfo = new FileInfo(targetFile);
                        shouldCopy = sourceFileInfo.LastWriteTime > targetFileInfo.LastWriteTime || 
                                     sourceFileInfo.Length != targetFileInfo.Length;
                    }

                    if (shouldCopy)
                    {
                        try
                        {
                            // Measure transfer time
                            var stopwatch = Stopwatch.StartNew();
                            
                            // Copy file
                            File.Copy(sourceFile, targetFile, true);

                            stopwatch.Stop();
                            long transferTime = stopwatch.ElapsedMilliseconds;

                            // Log transfer
                            await _logger.LogBackupOperationAsync(
                                job.JobName,
                                sourceFile,
                                targetFile,
                                fileSize,
                                transferTime);

                            Debug.WriteLine($"Copied: {relativePath}");
                        }
                        catch (Exception ex)
                        {
                            // Log error
                            await _logger.LogBackupOperationAsync(
                                job.JobName,
                                sourceFile,
                                targetFile,
                                fileSize,
                                -1);  // Negative time indicates error

                            Debug.WriteLine($"Error copying {relativePath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Skipped (unchanged): {relativePath}");
                    }

                    processedCount++;
                    processedSize += fileSize;
                }

                // Remove files that don't exist in source anymore for complete backups
                if (job.Type == BackupType.Complete)
                {
                    await RemoveDeletedFilesAsync(job.JobName, job.SourcePath, job.TargetPath);
                }

                // Mark job as completed
                await _stateManager.UpdateStateAsync(
                    job.JobName,
                    BackupState.Completed,
                    sourceFiles.Length,
                    totalSize,
                    0,
                    0
                );

                Debug.WriteLine($"Backup job completed: {job.JobName}");
            }
            catch (Exception ex)
            {
                // Update state to error
                await _stateManager.UpdateStateAsync(
                    job.JobName,
                    BackupState.Error,
                    0,
                    0,
                    0,
                    0
                );

                Debug.WriteLine($"Error executing backup job {job.JobName}: {ex.Message}");
                throw;
            }
        }

        // Removes files in the target directory that don't exist in the source directory
        private async Task RemoveDeletedFilesAsync(string backupName, string sourcePath, string targetPath)
        {
            Debug.WriteLine("Checking for files to remove...");
            
            // Get all files in the target directory
            var targetFiles = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);
            
            foreach (string targetFile in targetFiles)
            {
                // Calculate the relative path using Path.GetRelativePath for better performance
                string relativePath = Path.GetRelativePath(targetPath, targetFile);
                string sourceFile = Path.Combine(sourcePath, relativePath);
                
                // If the file doesn't exist in the source, delete it from the target
                if (!File.Exists(sourceFile))
                {
                    try
                    {
                        var fileInfo = new FileInfo(targetFile);
                        long fileSize = fileInfo.Length;
                        
                        File.Delete(targetFile);
                        
                        // Log the deletion
                        await _logger.LogBackupOperationAsync(
                            $"{backupName} (Deletion)",
                            "N/A",
                            targetFile,
                            fileSize,
                            0);
                        
                        Debug.WriteLine($"Deleted: {relativePath} (no longer exists in source)");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting {relativePath}: {ex.Message}");
                    }
                }
            }
            
            // Remove empty directories
            RemoveEmptyDirectories(targetPath);
        }

        // Recursively removes empty directories
        private void RemoveEmptyDirectories(string directory)
        {
            // Process all subdirectories
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                RemoveEmptyDirectories(subDir);
                
                // If the directory is empty after processing subdirectories, delete it
                if (!Directory.EnumerateFileSystemEntries(subDir).Any())
                {
                    try
                    {
                        Directory.Delete(subDir);
                        Debug.WriteLine($"Removed empty directory: {Path.GetFileName(subDir)}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error removing directory {Path.GetFileName(subDir)}: {ex.Message}");
                    }
                }
            }
        }

        // Pauses the current backup job
        public void PauseBackupJob()
        {
            lock (_pauseLock)
            {
                _isPaused = true;
            }
        }

        // Resumes the current backup job
        public void ResumeBackupJob()
        {
            lock (_pauseLock)
            {
                _isPaused = false;
            }
        }

        // Stops the current backup job
        public void StopBackupJob()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}