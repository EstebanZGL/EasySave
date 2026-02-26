using System;

namespace EasySave.Models
{
    public class BackupJobStatusEventArgs : EventArgs
    {
        public string JobName { get; }
        public string Status { get; }
        public int Progress { get; }
        public string CurrentFile { get; }
        
        public BackupJobStatusEventArgs(string jobName, string status, int progress, string currentFile)
        {
            JobName = jobName;
            Status = status;
            Progress = progress;
            CurrentFile = currentFile;
        }
    }
}