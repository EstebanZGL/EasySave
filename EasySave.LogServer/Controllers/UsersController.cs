using EasySave.LogServer.Models;
using EasySave.LogServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EasySave.LogServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserConnectionService _userConnectionService;
        private readonly LogStorageService _logStorageService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserConnectionService userConnectionService, 
            LogStorageService logStorageService, 
            ILogger<UsersController> logger)
        {
            _userConnectionService = userConnectionService ?? throw new ArgumentNullException(nameof(userConnectionService));
            _logStorageService = logStorageService ?? throw new ArgumentNullException(nameof(logStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a user connection
        /// </summary>
        /// <param name="connectionInfo">Information about the connecting user</param>
        /// <returns>A result indicating success or failure</returns>
        [HttpPost("connect")]
        public IActionResult Connect([FromBody] ConnectedUser connectionInfo)
        {
            try
            {
                if (connectionInfo == null)
                {
                    return BadRequest("Connection information cannot be null");
                }

                // Set IP address if not provided
                if (string.IsNullOrEmpty(connectionInfo.IpAddress))
                {
                    connectionInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                }

                // Register the user connection
                var user = _userConnectionService.RegisterUserConnection(
                    connectionInfo.UserName,
                    connectionInfo.MachineName,
                    connectionInfo.IpAddress);
                
                _logger.LogInformation($"User connected: {user.UserName} from {user.MachineName} ({user.IpAddress})");
                
                return Ok(new { Success = true, UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user connection");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all currently connected users
        /// </summary>
        /// <returns>A list of connected users</returns>
        [HttpGet]
        public IActionResult GetConnectedUsers()
        {
            try
            {
                var users = _userConnectionService.GetConnectedUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connected users");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets logs for a specific user
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="date">Optional date filter (format: yyyy-MM-dd)</param>
        /// <returns>A list of log entries for the user</returns>
        [HttpGet("{userName}/logs")]
        public async Task<IActionResult> GetUserLogs(string userName, [FromQuery] string date = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    return BadRequest("Username cannot be empty");
                }

                // If date is provided, filter logs for that date
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                {
                    var logs = _logStorageService.GetLogEntriesForDate(parsedDate);
                    var userLogs = logs.FindAll(log => log.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                    return Ok(userLogs);
                }
                else
                {
                    // Get logs for today
                    var logs = _logStorageService.GetLogEntriesForDate(DateTime.Today);
                    var userLogs = logs.FindAll(log => log.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                    return Ok(userLogs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting logs for user {userName}");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates a user's activity status
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="machineName">The machine name</param>
        /// <returns>A result indicating success or failure</returns>
        [HttpPost("{userName}/activity")]
        public IActionResult UpdateUserActivity(string userName, [FromBody] string machineName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    return BadRequest("Username cannot be empty");
                }

                if (string.IsNullOrEmpty(machineName))
                {
                    return BadRequest("Machine name cannot be empty");
                }

                _userConnectionService.UpdateUserActivity(userName, machineName);
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating activity for user {userName}");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }
    }
}