using System;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    public interface IBackupService : IDisposable
    {
        Task ExecuteBackupJobAsync(BackupJob job);
        void PauseBackupJob();
        void ResumeBackupJob();
        void StopBackupJob();
    }
}