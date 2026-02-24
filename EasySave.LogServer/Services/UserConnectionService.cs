using EasySave.LogServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.LogServer.Services
{
    /// <summary>
    /// Service responsible for tracking connected users
    /// </summary>
    public class UserConnectionService
    {
        private readonly ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new ConcurrentDictionary<string, ConnectedUser>();
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _userTimeout = TimeSpan.FromMinutes(30); // Consider user disconnected after 30 minutes of inactivity
        
        /// <summary>
        /// Creates a new instance of the UserConnectionService
        /// </summary>
        public UserConnectionService()
        {
            // Start a timer to clean up inactive users
            _cleanupTimer = new Timer(CleanupInactiveUsers, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
        
        /// <summary>
        /// Registers a user connection
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="machineName">The machine name</param>
        /// <param name="ipAddress">The IP address</param>
        /// <returns>The connected user information</returns>
        public ConnectedUser RegisterUserConnection(string userName, string machineName, string ipAddress)
        {
            // Create a unique key for this user+machine combination
            string key = $"{userName}@{machineName}";
            
            // If user already exists, update the last activity time
            if (_connectedUsers.TryGetValue(key, out var existingUser))
            {
                existingUser.LastActivity = DateTime.Now;
                return existingUser;
            }
            
            // Otherwise, create a new user entry
            var user = new ConnectedUser
            {
                UserName = userName,
                MachineName = machineName,
                ConnectedAt = DateTime.Now,
                LastActivity = DateTime.Now,
                IpAddress = ipAddress
            };
            
            _connectedUsers[key] = user;
            return user;
        }
        
        /// <summary>
        /// Updates a user's last activity time
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="machineName">The machine name</param>
        public void UpdateUserActivity(string userName, string machineName)
        {
            string key = $"{userName}@{machineName}";
            
            if (_connectedUsers.TryGetValue(key, out var user))
            {
                user.LastActivity = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Gets all currently connected users
        /// </summary>
        /// <returns>A list of connected users</returns>
        public List<ConnectedUser> GetConnectedUsers()
        {
            return _connectedUsers.Values.ToList();
        }
        
        /// <summary>
        /// Gets a specific connected user
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="machineName">The machine name</param>
        /// <returns>The connected user, or null if not found</returns>
        public ConnectedUser GetConnectedUser(string userName, string machineName)
        {
            string key = $"{userName}@{machineName}";
            
            _connectedUsers.TryGetValue(key, out var user);
            return user;
        }
        
        /// <summary>
        /// Removes inactive users
        /// </summary>
        private void CleanupInactiveUsers(object state)
        {
            var now = DateTime.Now;
            var keysToRemove = new List<string>();
            
            foreach (var kvp in _connectedUsers)
            {
                if (now - kvp.Value.LastActivity > _userTimeout)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _connectedUsers.TryRemove(key, out _);
            }
        }
    }
}