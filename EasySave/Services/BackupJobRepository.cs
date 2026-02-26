using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    public class BackupJobRepository
    {
        private readonly string _filePath;
        private readonly List<BackupJob> _backupJobs;
        private readonly object _fileLock = new object();
        
        public BackupJobRepository(string filePath = "backupjobs.json")
        {
            _filePath = filePath;
            _backupJobs = LoadBackupJobs();
        }
        
        public List<BackupJob> GetAllBackupJobs()
        {
            lock (_fileLock)
            {
                return _backupJobs.ToList();
            }
        }
        
        public BackupJob GetBackupJob(string jobName)
        {
            lock (_fileLock)
            {
                return _backupJobs.FirstOrDefault(j => j.JobName == jobName);
            }
        }
        
        public bool AddBackupJob(BackupJob backupJob)
        {
            lock (_fileLock)
            {
                if (_backupJobs.Any(j => j.JobName == backupJob.JobName))
                {
                    return false;
                }
                
                _backupJobs.Add(backupJob);
                SaveBackupJobs();
                return true;
            }
        }
        
        public bool UpdateBackupJob(BackupJob backupJob)
        {
            lock (_fileLock)
            {
                var existingJob = _backupJobs.FirstOrDefault(j => j.JobName == backupJob.JobName);
                if (existingJob == null)
                {
                    return false;
                }
                
                existingJob.SourcePath = backupJob.SourcePath;
                existingJob.TargetPath = backupJob.TargetPath;
                existingJob.Description = backupJob.Description;
                existingJob.Type = backupJob.Type;
                
                SaveBackupJobs();
                return true;
            }
        }
        
        public bool UpdateBackupJobLastBackupTime(string jobName, DateTime lastBackupTime)
        {
            lock (_fileLock)
            {
                var existingJob = _backupJobs.FirstOrDefault(j => j.JobName == jobName);
                if (existingJob == null)
                {
                    return false;
                }
                
                existingJob.LastBackupTime = lastBackupTime;
                
                SaveBackupJobs();
                return true;
            }
        }
        
        public bool DeleteBackupJob(string jobName)
        {
            lock (_fileLock)
            {
                var existingJob = _backupJobs.FirstOrDefault(j => j.JobName == jobName);
                if (existingJob == null)
                {
                    return false;
                }
                
                _backupJobs.Remove(existingJob);
                SaveBackupJobs();
                return true;
            }
        }
        
        private List<BackupJob> LoadBackupJobs()
        {
            if (!File.Exists(_filePath))
            {
                return new List<BackupJob>();
            }
            
            try
            {
                var json = File.ReadAllText(_filePath);
                var jobs = JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();

                bool hadBackfillChanges = false;

                // Backfill creation dates for jobs created before this field existed
                foreach (var job in jobs)
                {
                    if (job.CreatedAt == default)
                    {
                        job.CreatedAt = job.LastBackupTime ?? DateTime.Now;
                        hadBackfillChanges = true;
                    }
                }

                if (hadBackfillChanges)
                {
                    var updatedJson = JsonSerializer.Serialize(jobs);
                    WriteAllTextAtomic(_filePath, updatedJson);
                }

                return jobs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading backup jobs: {ex.Message}");
                return new List<BackupJob>();
            }
        }
        
        private void SaveBackupJobs()
        {
            try
            {
                var json = JsonSerializer.Serialize(_backupJobs);
                WriteAllTextAtomic(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving backup jobs: {ex.Message}");
            }
        }

        private static void WriteAllTextAtomic(string path, string content)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = Path.Combine(directory ?? AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(tempPath, content);
            File.Move(tempPath, path, true);
        }
    }
}