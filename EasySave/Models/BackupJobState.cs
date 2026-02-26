using System;

namespace EasySave.Models
{
     
    /// Represents the real-time state of a backup job during execution
    /// Used for state.json and UI status updates
     
    public class BackupJobState
    {
        // Name of the backup job this state belongs to
        public string JobName;
        
        // Current status (Running, Paused, Completed, etc.)
        public string Status;
        
        // Total number of files to be processed
        public int TotalFiles;
        
        // Number of files still waiting to be processed
        public int TotalFilesRemaining;
        
        // Total size in bytes of all files to be processed
        public long TotalSize;
        
        // Size in bytes of files still waiting to be processed
        public long TotalSizeRemaining;
        
        // Path of the file currently being processed
        public string CurrentFile;
        
        // Destination path of the file currently being processed
        public string CurrentFileDestination;
        
        // Progress percentage (0-100)
        public int Progress;
        
        // When the backup job started execution
        public DateTime StartTime;
        
        // When the backup job finished (null if still running)
        public DateTime? EndTime;

         
        /// Creates a new backup job state with default values
         
        public BackupJobState()
        {
            JobName = string.Empty;
            Status = "En cours"; // Default status "In progress"
            TotalFiles = 0;
            TotalFilesRemaining = 0;
            TotalSize = 0;
            TotalSizeRemaining = 0;
            CurrentFile = string.Empty;
            CurrentFileDestination = string.Empty;
            Progress = 0;
            StartTime = DateTime.Now;
        }
    }
}