using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Models;
using EasyLog;

namespace EasySave.Services
{
    // Service for executing backup operations
    public class BackupService
    {
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;

        // Constructor that initializes the logger and state manager
        public BackupService(ILogger logger, StateManager stateManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
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

                // Remove files that don't exist in source anymore for complete backups
                if (job.Type == BackupType.Complete)
                {
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
                Console.WriteLine();
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

        // Removes files in the target directory that don't exist in the source directory
        private async Task RemoveDeletedFilesAsync(string backupName, string sourcePath, string targetPath)
        {
            Console.WriteLine("Checking for files to remove...");
            
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