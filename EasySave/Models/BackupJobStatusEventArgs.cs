using System;

namespace EasySave.Models
{
    /// <summary>
    /// Event arguments for backup job status changes
    /// </summary>
    public class BackupJobStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the job
        /// </summary>
        public string JobName { get; }
        
        /// <summary>
        /// Gets the status of the job
        /// </summary>
        public string Status { get; }
        
        /// <summary>
        /// Gets the progress percentage (0-100)
        /// </summary>
        public int Progress { get; }
        
        /// <summary>
        /// Gets the current file being processed
        /// </summary>
        public string CurrentFile { get; }
        
        /// <summary>
        /// Creates a new instance of the BackupJobStatusEventArgs class
        /// </summary>
        /// <param name="jobName">The name of the job</param>
        /// <param name="status">The status of the job</param>
        /// <param name="progress">The progress percentage</param>
        /// <param name="currentFile">The current file being processed</param>
        public BackupJobStatusEventArgs(string jobName, string status, int progress, string currentFile)
        {
            JobName = jobName;
            Status = status;
            Progress = progress;
            CurrentFile = currentFile;
        }
    }
}