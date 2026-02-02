using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Models;
using EasyLog;

namespace EasySave.Services
{
    /// <summary>
    /// Service for executing backup operations
    /// </summary>
    public class BackupService
    {
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger for recording backup operations</param>
        /// <param name="stateManager">State manager for tracking backup progress</param>
        public BackupService(ILogger logger, StateManager stateManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        /// <summary>
        /// Executes a backup job
        /// </summary>
        /// <param name="job">Backup job to execute</param>
        /// <exception cref="ArgumentException">Thrown when job is invalid</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when source directory doesn't exist</exception>
        public async Task ExecuteBackupJobAsync(BackupJob job)
        {
            // Validate job
            if (job == null || !job.Validate())
            {
                throw new ArgumentException("Invalid backup job", nameof(job));
            }

            Console.WriteLine($"Starting backup job: {job.Name}");

            try
            {
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
                    job.Name,
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
                    // Get relative path
                    string relativePath = sourceFile.Substring(job.SourcePath.Length).TrimStart(Path.DirectorySeparatorChar);
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
                        job.Name,
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
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                            
                            // Copy file
                            File.Copy(sourceFile, targetFile, true);

                            stopwatch.Stop();
                            long transferTime = stopwatch.ElapsedMilliseconds;

                            // Log transfer
                            await _logger.LogTransferAsync(
                                job.Name,
                                sourceFile,
                                targetFile,
                                fileSize,
                                transferTime);

                            Console.WriteLine($"Copied: {relativePath}");
                        }
                        catch (Exception ex)
                        {
                            // Log error
                            await _logger.LogTransferAsync(
                                job.Name,
                                sourceFile,
                                targetFile,
                                fileSize,
                                -1); // Negative time indicates error

                            Console.WriteLine($"Error copying {relativePath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipped (unchanged): {relativePath}");
                    }

                    processedCount++;
                    processedSize += fileSize;
                }

                // Remove files that don't exist in source anymore
                if (job.Type == BackupType.Complete)
                {
                    // For complete backups, remove files that don't exist in source
                    await RemoveDeletedFilesAsync(job.Name, job.SourcePath, job.TargetPath);
                }

                // Mark job as completed
                await _stateManager.UpdateStateAsync(
                    job.Name,
                    BackupState.Completed,
                    sourceFiles.Length,
                    totalSize,
                    0,
                    0
                );

                Console.WriteLine($"Backup job completed: {job.Name}");
            }
            catch (Exception ex)
            {
                // Update state to error
                await _stateManager.UpdateStateAsync(
                    job.Name,
                    BackupState.Error,
                    0,
                    0,
                    0,
                    0
                );

                Console.WriteLine($"Error executing backup job {job.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Removes files in the target directory that don't exist in the source directory
        /// </summary>
        /// <param name="backupName">Name of the backup job</param>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="targetPath">Target directory path</param>
        private async Task RemoveDeletedFilesAsync(string backupName, string sourcePath, string targetPath)
        {
            Console.WriteLine("Checking for files to remove...");
            
            // Get all files in the target directory
            var targetFiles = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);
            
            foreach (string targetFile in targetFiles)
            {
                // Calculate the relative path
                string relativePath = targetFile.Substring(targetPath.Length).TrimStart(Path.DirectorySeparatorChar);
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
                        await _logger.LogTransferAsync(
                            $"{backupName} (Deletion)",
                            "N/A",
                            targetFile,
                            fileSize,
                            0);
                        
                        Console.WriteLine($"Deleted: {relativePath} (no longer exists in source)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting {relativePath}: {ex.Message}");
                    }
                }
            }
            
            // Remove empty directories
            RemoveEmptyDirectories(targetPath);
        }

        /// <summary>
        /// Recursively removes empty directories
        /// </summary>
        /// <param name="directory">Directory to check</param>
        private void RemoveEmptyDirectories(string directory)
        {
            // Process all subdirectories
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                RemoveEmptyDirectories(subDir);
                
                // If the directory is empty after processing subdirectories, delete it
                if (Directory.GetFiles(subDir).Length == 0 && 
                    Directory.GetDirectories(subDir).Length == 0)
                {
                    try
                    {
                        Directory.Delete(subDir);
                        Console.WriteLine($"Removed empty directory: {Path.GetFileName(subDir)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error removing directory {Path.GetFileName(subDir)}: {ex.Message}");
                    }
                }
            }
        }
    }
}