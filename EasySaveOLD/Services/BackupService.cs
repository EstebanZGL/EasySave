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
    /// Service for executing backup jobs
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
            _logger = logger;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Execute a backup job
        /// </summary>
        /// <param name="job">Backup job to execute</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task ExecuteBackupJobAsync(BackupJob job)
        {
            if (job == null || !job.Validate())
            {
                throw new ArgumentException("Invalid backup job");
            }

            Console.WriteLine($"Starting backup job: {job.Name}");

            try
            {
                // Check if source directory exists
                if (!Directory.Exists(job.SourcePath))
                {
                    throw new DirectoryNotFoundException($"Source directory not found: {job.SourcePath}");
                }

                // Create target directory if it doesn't exist
                if (!Directory.Exists(job.TargetPath))
                {
                    Directory.CreateDirectory(job.TargetPath);
                }

                // Get all files in the source directory (including subdirectories)
                var sourceFiles = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);

                // Update state to Active
                await _stateManager.UpdateStateAsync(
                    job.Name,
                    BackupState.Active,
                    sourceFiles.Length,
                    sourceFiles.Sum(f => new FileInfo(f).Length),
                    sourceFiles.Length,
                    sourceFiles.Sum(f => new FileInfo(f).Length));

                // Process each file
                int processedCount = 0;
                foreach (string sourceFile in sourceFiles)
                {
                    // Get relative path
                    string relativePath = sourceFile.Substring(job.SourcePath.Length).TrimStart(Path.DirectorySeparatorChar);

                    // Construct target file path
                    string targetFile = Path.Combine(job.TargetPath, relativePath);

                    // Create target directory if it doesn't exist
                    string targetDirectory = Path.GetDirectoryName(targetFile);
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Get file info for size calculation
                    var sourceFileInfo = new FileInfo(sourceFile);
                    long fileSize = sourceFileInfo.Length;

                    // Update state with current file
                    await _stateManager.UpdateStateAsync(
                        job.Name,
                        BackupState.Active,
                        sourceFiles.Length,
                        sourceFiles.Sum(f => new FileInfo(f).Length),
                        sourceFiles.Length - processedCount,
                        sourceFiles.Sum(f => new FileInfo(f).Length) - (processedCount > 0 ? processedCount * fileSize : 0),
                        sourceFile,
                        targetFile);

                    // Check if file needs to be copied (based on backup type)
                    bool shouldCopy = true;
                    if (job.Type == BackupType.Differential && File.Exists(targetFile))
                    {
                        var targetFileInfo = new FileInfo(targetFile);

                        // Only copy if source file is newer or different size
                        shouldCopy = sourceFileInfo.LastWriteTime > targetFileInfo.LastWriteTime || 
                                     sourceFileInfo.Length != targetFileInfo.Length;
                    }

                    if (shouldCopy)
                    {
                        try
                        {
                            // Measure transfer time
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            // Copy the file
                            File.Copy(sourceFile, targetFile, true);

                            stopwatch.Stop();
                            long transferTime = stopwatch.ElapsedMilliseconds;

                            // Log the transfer
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
                            // Log the error
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
                }

                // Update state to Completed
                await _stateManager.UpdateStateAsync(
                    job.Name,
                    BackupState.Completed,
                    sourceFiles.Length,
                    sourceFiles.Sum(f => new FileInfo(f).Length),
                    0,
                    0);

                Console.WriteLine($"Backup job completed: {job.Name}");
            }
            catch (Exception ex)
            {
                // Update state to Error
                await _stateManager.UpdateStateAsync(
                    job.Name,
                    BackupState.Error,
                    0,
                    0,
                    0,
                    0);

                Console.WriteLine($"Error executing backup job {job.Name}: {ex.Message}");
                throw;
            }
        }
    }
}