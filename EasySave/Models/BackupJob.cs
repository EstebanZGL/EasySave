using System;

namespace EasySave.Models
{
    // Represents a backup job configuration
    public class BackupJob
    {
        // Name of the backup job
        public string Name { get; set; }

        // Source directory path
        public string SourcePath { get; set; }

        // Target directory path
        public string TargetPath { get; set; }

        // Type of backup (Complete or Differential)
        public BackupType Type { get; set; }

        // Constructor with all parameters
        public BackupJob(string name, string sourcePath, string targetPath, BackupType type)
        {
            Name = name;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Type = type;
        }

        // Default constructor for serialization
        public BackupJob() { }

        // Validates the backup job parameters
        // Returns: True if valid, false otherwise
        public bool Validate()
        {
            // Check if name is not empty
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // Check if source path exists
            if (string.IsNullOrWhiteSpace(SourcePath) || !System.IO.Directory.Exists(SourcePath))
                return false;

            // Check if target path is specified
            if (string.IsNullOrWhiteSpace(TargetPath))
                return false;

            return true;
        }
    }

    // Type of backup
    public enum BackupType
    {
        // Complete backup (copies all files)
        Complete,
        
        // Differential backup (copies only new or modified files)
        Differential
    }

    // State of a backup job
    public enum BackupState
    {
        // Backup job is inactive
        Inactive,
        
        // Backup job is active
        Active,
        
        // Backup job has completed successfully
        Completed,
        
        // Backup job has failed
        Error
    }
}