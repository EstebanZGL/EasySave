using System;

namespace EasySave.LogServer.Models
{
    /// <summary>
    /// Represents a connected user in the system
    /// </summary>
    public class ConnectedUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user connection
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the machine name
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the time when the user connected
        /// </summary>
        public DateTime ConnectedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the last activity time
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the IP address of the client
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
    }
}