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
    }
}