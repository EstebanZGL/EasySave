using System;

namespace EasySave.Models
{
    /// <summary>
    /// Represents a backup job
    /// </summary>
    public class BackupJob
    {
        /// <summary>
        /// Name of the backup job
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Source directory path
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Target directory path
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Type of backup (Complete or Differential)
        /// </summary>
        public BackupType Type { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the backup job</param>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="targetPath">Target directory path</param>
        /// <param name="type">Type of backup</param>
        public BackupJob(string name, string sourcePath, string targetPath, BackupType type)
        {
            Name = name;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Type = type;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public BackupJob() { }

        /// <summary>
        /// Validates the backup job parameters
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Check if name is not empty
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // Check if source path is not empty and exists
            if (string.IsNullOrWhiteSpace(SourcePath) || !System.IO.Directory.Exists(SourcePath))
                return false;

            // Check if target path is not empty
            if (string.IsNullOrWhiteSpace(TargetPath))
                return false;

            return true;
        }
    }

    /// <summary>
    /// Type of backup
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Complete backup (copies all files)
        /// </summary>
        Complete,
        
        /// <summary>
        /// Differential backup (copies only new or modified files)
        /// </summary>
        Differential
    }

    /// <summary>
    /// State of a backup job
    /// </summary>
    public enum BackupState
    {
        /// <summary>
        /// Backup job is inactive
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Backup job is active
        /// </summary>
        Active,
        
        /// <summary>
        /// Backup job has completed successfully
        /// </summary>
        Completed,
        
        /// <summary>
        /// Backup job has failed
        /// </summary>
        Error
    }
}