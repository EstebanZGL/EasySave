using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Repository for backup jobs
    /// </summary>
    public class BackupJobRepository
    {
        private readonly string _filePath;
        private readonly List<BackupJob> _backupJobs;
        
        /// <summary>
        /// Creates a new instance of the BackupJobRepository class
        /// </summary>
        /// <param name="filePath">The path to the file where backup jobs are stored</param>
        public BackupJobRepository(string filePath = "backupjobs.json")
        {
            _filePath = filePath;
            _backupJobs = LoadBackupJobs();
        }
        
        /// <summary>
        /// Gets all backup jobs
        /// </summary>
        /// <returns>A list of all backup jobs</returns>
        public List<BackupJob> GetAllBackupJobs()
        {
            return _backupJobs.ToList();
        }
        
        /// <summary>
        /// Gets a backup job by name
        /// </summary>
        /// <param name="jobName">The name of the job to get</param>
        /// <returns>The backup job, or null if not found</returns>
        public BackupJob GetBackupJob(string jobName)
        {
            return _backupJobs.FirstOrDefault(j => j.JobName == jobName);
        }
        
        /// <summary>
        /// Adds a new backup job
        /// </summary>
        /// <param name="backupJob">The backup job to add</param>
        /// <returns>True if the job was added, false if a job with the same name already exists</returns>
        public bool AddBackupJob(BackupJob backupJob)
        {
            if (_backupJobs.Any(j => j.JobName == backupJob.JobName))
            {
                return false;
            }
            
            _backupJobs.Add(backupJob);
            SaveBackupJobs();
            return true;
        }
        
        /// <summary>
        /// Updates an existing backup job
        /// </summary>
        /// <param name="backupJob">The backup job to update</param>
        /// <returns>True if the job was updated, false if the job was not found</returns>
        public bool UpdateBackupJob(BackupJob backupJob)
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
        
        /// <summary>
        /// Met à jour la date de dernière sauvegarde d'un travail de sauvegarde
        /// </summary>
        /// <param name="jobName">Le nom du travail à mettre à jour</param>
        /// <param name="lastBackupTime">La nouvelle date de dernière sauvegarde</param>
        /// <returns>True si le travail a été mis à jour, false si le travail n'a pas été trouvé</returns>
        public bool UpdateBackupJobLastBackupTime(string jobName, DateTime lastBackupTime)
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
        
        /// <summary>
        /// Deletes a backup job
        /// </summary>
        /// <param name="jobName">The name of the job to delete</param>
        /// <returns>True if the job was deleted, false if the job was not found</returns>
        public bool DeleteBackupJob(string jobName)
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
        
        /// <summary>
        /// Loads backup jobs from the file
        /// </summary>
        /// <returns>A list of backup jobs</returns>
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

                // Backfill creation dates for jobs created before this field existed.
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
                    File.WriteAllText(_filePath, updatedJson);
                }

                return jobs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading backup jobs: {ex.Message}");
                return new List<BackupJob>();
            }
        }
        
        /// <summary>
        /// Saves backup jobs to the file
        /// </summary>
        private void SaveBackupJobs()
        {
            try
            {
                var json = JsonSerializer.Serialize(_backupJobs);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving backup jobs: {ex.Message}");
            }
        }
    }
}
