using System;

namespace EasySave.Models
{
    /// <summary>
    /// Represents the state of a backup job during execution
    /// </summary>
    public class BackupJobState
    {
        /// <summary>
        /// The name of the backup job
        /// </summary>
        public string JobName;

        /// <summary>
        /// The status of the backup job
        /// </summary>
        public string Status;

        /// <summary>
        /// The total number of files to process
        /// </summary>
        public int TotalFiles;

        /// <summary>
        /// The number of files remaining to process
        /// </summary>
        public int TotalFilesRemaining;

        /// <summary>
        /// The total size of all files to process in bytes
        /// </summary>
        public long TotalSize;

        /// <summary>
        /// The size remaining to process in bytes
        /// </summary>
        public long TotalSizeRemaining;

        /// <summary>
        /// The path of the file currently being processed
        /// </summary>
        public string CurrentFile;

        /// <summary>
        /// The destination path of the file currently being processed
        /// </summary>
        public string CurrentFileDestination;

        /// <summary>
        /// The progress percentage (0-100)
        /// </summary>
        public int Progress;

        /// <summary>
        /// The start time of the backup job
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// The end time of the backup job
        /// </summary>
        public DateTime? EndTime;

        /// <summary>
        /// Creates a new instance of the BackupJobState class
        /// </summary>
        public BackupJobState()
        {
            JobName = string.Empty;
            Status = "Not Started";
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