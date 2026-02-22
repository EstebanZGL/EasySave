using System;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Interface for backup service operations
    /// </summary>
    public interface IBackupService : IDisposable
    {
        /// <summary>
        /// Executes a backup job asynchronously
        /// </summary>
        /// <param name="job">The backup job to execute</param>
        Task ExecuteBackupJobAsync(BackupJob job);

        /// <summary>
        /// Pauses the current backup job
        /// </summary>
        void PauseBackupJob();

        /// <summary>
        /// Resumes the current backup job
        /// </summary>
        void ResumeBackupJob();

        /// <summary>
        /// Stops the current backup job
        /// </summary>
        void StopBackupJob();
    }
}