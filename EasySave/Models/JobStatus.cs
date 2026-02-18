namespace EasySave.Models
{
    /// <summary>
    /// Contains constants for job status values
    /// </summary>
    public static class JobStatus
    {
        public const string Running = "En cours";
        public const string Paused = "Paused";
        public const string Completed = "Completed";
        public const string Canceled = "Canceled";
        public const string Stopping = "Stopping";
        public const string Failed = "Failed";
    }
}