using System;

namespace EasySave.Services
{
     
    /// Event arguments for backup job status changes
     
    public class BackupJobStatusEventArgs : EventArgs
    {
         
        /// Gets the name of the backup job
         
        public string JobName { get; }

         
        /// Gets the status of the backup job
         
        public string Status { get; }

         
        /// Gets the progress percentage (0-100)
         
        public int Progress { get; }
        
         
        /// Gets the current file being processed
         
        public string CurrentFile { get; }

         
        /// Creates a new instance of the BackupJobStatusEventArgs class
         
        public BackupJobStatusEventArgs(string jobName, string status, int progress, string currentFile = "")
        {
            JobName = jobName;
            Status = status;
            Progress = progress;
            CurrentFile = currentFile;
        }
    }
}