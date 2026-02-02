using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Manages backup jobs
    /// </summary>
    public class BackupJobManager
    {
        private const int MaxJobs = 5; // Maximum number of backup jobs allowed
        private readonly List<BackupJob> _backupJobs;
        private readonly string _configFilePath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file</param>
        public BackupJobManager(string configFilePath)
        {
            _configFilePath = configFilePath;
            _backupJobs = new List<BackupJob>();
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Load existing jobs if available
            LoadJobs();
        }

        /// <summary>
        /// Get all backup jobs
        /// </summary>
        /// <returns>List of backup jobs</returns>
        public IReadOnlyList<BackupJob> GetJobs()
        {
            return _backupJobs.AsReadOnly();
        }

        /// <summary>
        /// Get a backup job by index
        /// </summary>
        /// <param name="index">Index of the job (0-based)</param>
        /// <returns>Backup job or null if index is invalid</returns>
        public BackupJob GetJob(int index)
        {
            if (index >= 0 && index < _backupJobs.Count)
            {
                return _backupJobs[index];
            }
            return null;
        }

        /// <summary>
        /// Create a new backup job
        /// </summary>
        /// <param name="name">Name of the backup job</param>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="targetPath">Target directory path</param>
        /// <param name="type">Type of backup</param>
        /// <returns>True if job was created, false otherwise</returns>
        public bool CreateJob(string name, string sourcePath, string targetPath, BackupType type)
        {
            // Check if maximum number of jobs has been reached
            if (_backupJobs.Count >= MaxJobs)
            {
                return false;
            }
            
            // Create and validate the job
            var job = new BackupJob(name, sourcePath, targetPath, type);
            if (!job.Validate())
            {
                return false;
            }
            
            // Add the job and save
            _backupJobs.Add(job);
            SaveJobs();
            
            return true;
        }

        /// <summary>
        /// Delete a backup job
        /// </summary>
        /// <param name="index">Index of the job to delete (0-based)</param>
        /// <returns>True if job was deleted, false otherwise</returns>
        public bool DeleteJob(int index)
        {
            if (index >= 0 && index < _backupJobs.Count)
            {
                _backupJobs.RemoveAt(index);
                SaveJobs();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Load backup jobs from the configuration file
        /// </summary>
        private void LoadJobs()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_configFilePath);
                    var jobs = JsonSerializer.Deserialize<List<BackupJob>>(json);
                    if (jobs != null)
                    {
                        _backupJobs.Clear();
                        _backupJobs.AddRange(jobs);
                        
                        // Ensure we don't exceed the maximum number of jobs
                        if (_backupJobs.Count > MaxJobs)
                        {
                            _backupJobs.RemoveRange(MaxJobs, _backupJobs.Count - MaxJobs);
                            SaveJobs(); // Save the truncated list
                        }
                    }
                }
                catch
                {
                    // If loading fails, start with an empty list
                    _backupJobs.Clear();
                }
            }
        }

        /// <summary>
        /// Save backup jobs to the configuration file
        /// </summary>
        private void SaveJobs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_backupJobs, options);
                File.WriteAllText(_configFilePath, json);
            }
            catch
            {
                // Handle serialization or file access errors
            }
        }
    }
}