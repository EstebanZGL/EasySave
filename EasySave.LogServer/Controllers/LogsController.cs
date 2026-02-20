using EasySave.LogServer.Models;
using EasySave.LogServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasySave.LogServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LogStorageService _logStorageService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(LogStorageService logStorageService, ILogger<LogsController> logger)
        {
            _logStorageService = logStorageService ?? throw new ArgumentNullException(nameof(logStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a new log entry to the centralized log system
        /// </summary>
        /// <param name="logEntry">The log entry to add</param>
        /// <returns>A result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> AddLogEntry([FromBody] LogEntry logEntry)
        {
            try
            {
                if (logEntry == null)
                {
                    return BadRequest("Log entry cannot be null");
                }

                // Set machine name and username if not provided
                if (string.IsNullOrEmpty(logEntry.MachineName))
                {
                    logEntry.MachineName = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                }

                if (string.IsNullOrEmpty(logEntry.UserName))
                {
                    logEntry.UserName = "Anonymous";
                }

                await _logStorageService.AddLogEntryAsync(logEntry);
                
                _logger.LogInformation($"Log entry added: {logEntry.Id} from {logEntry.MachineName}");
                
                return Ok(new { Success = true, LogId = logEntry.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding log entry");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all log entries for a specific date
        /// </summary>
        /// <param name="date">The date to get logs for (format: yyyy-MM-dd)</param>
        /// <returns>A list of log entries</returns>
        [HttpGet("date/{date}")]
        public IActionResult GetLogsByDate(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return BadRequest("Invalid date format. Use yyyy-MM-dd");
                }

                var logs = _logStorageService.GetLogEntriesForDate(parsedDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting logs for date {date}");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all log entries for a specific date range
        /// </summary>
        /// <param name="startDate">The start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">The end date (format: yyyy-MM-dd)</param>
        /// <returns>A list of log entries</returns>
        [HttpGet("range")]
        public async Task<IActionResult> GetLogsByDateRange([FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var parsedStartDate))
                {
                    return BadRequest("Invalid start date format. Use yyyy-MM-dd");
                }

                if (!DateTime.TryParse(endDate, out var parsedEndDate))
                {
                    return BadRequest("Invalid end date format. Use yyyy-MM-dd");
                }

                if (parsedEndDate < parsedStartDate)
                {
                    return BadRequest("End date cannot be earlier than start date");
                }

                var logs = await _logStorageService.GetLogEntriesForDateRangeAsync(parsedStartDate, parsedEndDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting logs for date range {startDate} to {endDate}");
                return StatusCode(500, new { Error = "An error occurred while processing the request", Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets the health status of the log server
        /// </summary>
        /// <returns>A status message</returns>
        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            return Ok(new { 
                Status = "Healthy", 
                Timestamp = DateTime.Now,
                Version = "3.0",
                ServerName = Environment.MachineName
            });
        }
    }
}