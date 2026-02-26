# Technical Documentation EasySave v3.0

## General Architecture

EasySave v3.0 is a backup application developed in C# (.NET 8.0) following the MVVM architecture pattern. The project is divided into several main components:

1. **EasySave**: Main WPF application with graphical user interface
2. **EasySaveCLI**: Command-line interface for automation and scripting
3. **EasyLog**: Class library for log management
4. **CryptoSoft**: External encryption utility
5. **EasySaveRemote**: Remote monitoring application
6. **EasySaveCore**: Shared core library

## Project Structure

```
EasySave/
├── Models/
│   ├── BackupJob.cs           # Data model for backup jobs
│   ├── JobPriority.cs         # Priority enumeration
│   └── BandwidthSettings.cs   # Bandwidth control model
├── ViewModels/
│   ├── MainViewModel.cs       # Main view model for application logic
│   ├── SettingsViewModel.cs   # Settings management
│   ├── ParallelExecutionViewModel.cs # Parallel execution management
│   └── RelayCommand.cs        # Command implementation
├── Views/
│   ├── MainWindow.xaml        # Main application window
│   ├── BackupJobDialog.xaml   # Dialog for creating/editing jobs
│   ├── SettingsWindow.xaml    # Settings window
│   └── ParallelMonitorWindow.xaml # Parallel execution monitoring
├── Services/
│   ├── BackupService.cs       # Backup execution
│   ├── ParallelBackupService.cs # Parallel backup management
│   ├── BusinessSoftwareMonitor.cs # Business software detection
│   ├── CryptoService.cs       # File encryption
│   ├── BandwidthManager.cs    # Bandwidth limitation
│   ├── PriorityManager.cs     # Job priority management
│   ├── SocketServer.cs        # Socket communication server
│   ├── StateManager.cs        # Real-time state management
│   └── TranslationService.cs  # Multilingual support
└── Converters/
    ├── BoolToColorConverter.cs # UI value converters
    └── PriorityToColorConverter.cs # Priority visualization

EasySaveCLI/
├── Program.cs                 # Entry point for CLI application
├── EasySaveCLIController.cs   # Command processing logic
└── ParallelExecutionManager.cs # CLI parallel execution

EasyLog/
├── ILogger.cs                 # Base logger interface
├── IEncryptionLogger.cs       # Extended logger with encryption support
├── IBandwidthLogger.cs        # Extended logger with bandwidth metrics
├── JsonLogger.cs              # JSON implementation
└── XmlLogger.cs               # XML implementation

CryptoSoft/
├── Program.cs                 # Encryption utility
├── QueueManager.cs            # Encryption request queue
└── ServerMode.cs              # Single-instance server mode

EasySaveRemote/
├── Program.cs                 # Entry point for remote application
├── RemoteViewModel.cs         # Remote monitoring view model
├── SocketClient.cs            # Socket communication client
└── Views/
    ├── MainWindow.xaml        # Remote monitoring interface
    └── ConnectionDialog.xaml  # Server connection dialog

EasySaveCore/
├── Models/                    # Shared models
├── Interfaces/                # Common interfaces
└── Utilities/                 # Shared utilities

output/                        # Common output folder for all executables
├── EasySave.exe               # GUI application
├── EasySaveCLI.exe            # Command-line application
├── CryptoSoft.exe             # Encryption utility
└── EasySaveRemote.exe         # Remote monitoring application

Docker/
├── Dockerfile                 # Docker container definition
├── docker-compose.yml         # Multi-container setup
└── entrypoint.sh              # Container startup script
```

## Main Components

### BackupJob Model

Represents a backup job with the following properties:
- **Name**: Job name (must be unique)
- **SourcePath**: Source path of files to backup
- **TargetPath**: Target path where to save files
- **Type**: Backup type (Complete or Differential)
- **Priority**: Job priority (High, Medium, Low)
- **MaxBandwidth**: Maximum bandwidth limit for this job (MB/s)
- **AllowParallel**: Whether this job can run in parallel with others

The `Validate()` method ensures that:
- Name is not empty
- Source path exists
- Target path is specified
- Priority is valid
- Bandwidth limit is valid

### MainViewModel

Central view model that manages the application's main functionality:

Key methods:
- `LoadBackupJobs()`: Loads saved backup jobs
- `ExecuteBackupJob(BackupJob job)`: Executes a specific backup job
- `ExecuteSelectedJobs(bool parallel)`: Executes selected jobs (sequentially or in parallel)
- `ExecuteParallelJobs(IEnumerable<BackupJob> jobs, int maxParallel)`: Executes jobs in parallel
- `CreateBackupJob(BackupJob job)`: Creates a new backup job
- `DeleteBackupJob(BackupJob job)`: Deletes an existing job
- `SetJobPriority(BackupJob job, JobPriority priority)`: Sets job priority
- `SetJobBandwidth(BackupJob job, int bandwidthLimit)`: Sets job bandwidth limit
- `OpenCreateBackupJobDialog()`: Opens the dialog for creating a new job
- `OpenEditBackupJobDialog(BackupJob job)`: Opens the dialog for editing a job
- `OpenSettings()`: Opens the settings window
- `StartSocketServer()`: Starts the socket server for remote connections

### ParallelBackupService

Service responsible for executing multiple backup operations in parallel:

Key methods:
- `ExecuteJobsAsync(IEnumerable<BackupJob> jobs, int maxParallel)`: Executes multiple jobs in parallel
- `QueueJobsByPriority(IEnumerable<BackupJob> jobs)`: Queues jobs based on priority
- `MonitorResourceUsage()`: Monitors system resources during parallel execution
- `AdjustParallelism(int currentParallel, SystemMetrics metrics)`: Dynamically adjusts parallelism based on system load

### BandwidthManager

Manages bandwidth limitations for backup operations:

Key methods:
- `InitializeThrottling()`: Sets up bandwidth throttling
- `ApplyGlobalLimit(int limitMBps)`: Applies a global bandwidth limit
- `ApplyJobLimit(BackupJob job)`: Applies a job-specific bandwidth limit
- `GetAvailableBandwidth()`: Gets currently available bandwidth
- `ReleaseLimit(BackupJob job)`: Releases bandwidth when a job completes

### PriorityManager

Manages job priorities and execution order:

Key methods:
- `SortJobsByPriority(IEnumerable<BackupJob> jobs)`: Sorts jobs by priority
- `GetNextJob()`: Gets the next job to execute based on priority
- `CalculateResourceAllocation(BackupJob job)`: Calculates resource allocation based on priority

### SocketServer

Manages socket communication for remote monitoring and control:

Key methods:
- `StartServer(int port)`: Starts the socket server on the specified port
- `HandleClient(TcpClient client)`: Handles client connections
- `ProcessCommand(string command, TcpClient client)`: Processes commands from clients
- `BroadcastStateUpdate()`: Broadcasts state updates to connected clients
- `StopServer()`: Stops the socket server

### EasySaveCLIController

Manages the command-line interface functionality with enhanced features:

Key methods:
- `RunWithArgsAsync(string[] args)`: Processes command-line arguments
- `ExecuteCommandLineArgsAsync(string[] args)`: Executes specified jobs
- `ExecuteParallelJobsAsync(string[] jobIndexes, int maxParallel)`: Executes jobs in parallel
- `ExecutePriorityJobsAsync(JobPriority priority)`: Executes jobs with specified priority
- `ApplyBandwidthLimit(int limitMBps)`: Applies bandwidth limitation
- `ShowMainMenuAsync()`: Displays interactive menu in console mode
- `ParseJobIndexes(string input)`: Parses job numbers from command line
- `DisplayCommandLineHelp()`: Shows available commands and syntax

### SettingsViewModel

Manages application settings and preferences with new options:

Key properties:
- `BusinessSoftwareName`: Name of the business software to detect
- `EncryptionExtensions`: List of file extensions to encrypt
- `CryptoSoftPath`: Path to the CryptoSoft executable
- `LogFormat`: Current log format (JSON/XML)
- `MaxParallelJobs`: Maximum number of parallel jobs
- `GlobalBandwidthLimit`: Global bandwidth limit in MB/s
- `RespectJobPriority`: Whether to respect job priorities
- `RemoteAccessEnabled`: Whether remote access is enabled
- `RemoteAccessPort`: Port for remote access
- `DockerMode`: Whether running in Docker container

Key methods:
- `IsBusinessSoftwareRunning()`: Checks if the specified business software is running
- `ShouldEncryptFile(string filePath)`: Determines if a file should be encrypted
- `LoadSettings()`: Loads settings from settings.json
- `SaveSettings()`: Saves settings to settings.json
- `ApplyBandwidthLimits()`: Applies configured bandwidth limits
- `ConfigureRemoteAccess()`: Configures remote access settings

### CryptoSoft Enhancements

The CryptoSoft utility has been enhanced with:

Key features:
- **Server Mode**: Runs as a single instance to handle multiple encryption requests
- **Request Queue**: Manages encryption requests efficiently
- **Priority Support**: Processes requests based on priority
- **Performance Optimization**: Improved encryption algorithms and memory usage

Key methods:
- `StartServerMode()`: Starts CryptoSoft in server mode
- `ProcessEncryptionQueue()`: Processes the encryption request queue
- `EncryptFile(string source, string target, int priority)`: Encrypts a file with priority

### RemoteViewModel

Manages the remote monitoring application:

Key methods:
- `ConnectToServer(string address, int port)`: Connects to an EasySave server
- `RequestJobList()`: Requests the list of backup jobs
- `ExecuteJob(int jobId)`: Executes a specific job remotely
- `PauseJob(int jobId)`: Pauses a specific job
- `ResumeJob(int jobId)`: Resumes a specific job
- `StopJob(int jobId)`: Stops a specific job
- `CreateJob(BackupJob job)`: Creates a new job remotely
- `ReceiveStateUpdates()`: Receives and processes state updates

## Build Configuration

All projects are configured to generate their executables in a common `output` folder at the project level:

- **EasySave.csproj**: WPF GUI application
- **EasySaveCLI.csproj**: Console application
- **CryptoSoft.csproj**: Encryption utility
- **EasySaveRemote.csproj**: Remote monitoring application

This configuration simplifies distribution and testing by keeping all executables and their dependencies in a single location.

## Docker Configuration

The Docker configuration includes:

- **Dockerfile**: Defines the container image
- **docker-compose.yml**: Defines multi-container setup
- **entrypoint.sh**: Container startup script

The Docker container:
- Runs on .NET 8.0 runtime
- Exposes port 8080 for web interface
- Provides volume mounts for source and target directories
- Includes all components (EasySave, CryptoSoft)

## Key Features in v3.0

### 1. Parallel Backup Execution

EasySave v3.0 introduces the ability to execute multiple backup jobs simultaneously:

- **Configurable Parallelism**: Set the maximum number of concurrent backup jobs
- **Dynamic Adjustment**: Automatically adjusts parallelism based on system load
- **Resource Monitoring**: Monitors CPU, memory, and disk usage during parallel execution
- **Job Coordination**: Ensures jobs don't interfere with each other

Implementation details:
- Uses `Task.WhenAll()` for parallel execution
- Implements a job queue with priority support
- Uses `SemaphoreSlim` to control the degree of parallelism
- Monitors system resources using `PerformanceCounter`

### 2. Priority Management

Job prioritization system:

- **Priority Levels**: High, Medium, Low
- **Resource Allocation**: Higher priority jobs get more resources
- **Execution Order**: Higher priority jobs execute first
- **Dynamic Adjustment**: Priority can be changed during execution

Implementation details:
- Uses a priority queue data structure
- Implements resource allocation based on priority
- Provides priority inheritance to prevent priority inversion
- Allows dynamic priority changes

### 3. Bandwidth Limitation

Bandwidth control for network and disk operations:

- **Global Limitation**: Set a global bandwidth limit for all jobs
- **Per-Job Limitation**: Set individual limits for specific jobs
- **Dynamic Adjustment**: Adjust limits during execution
- **Monitoring**: Real-time bandwidth usage monitoring

Implementation details:
- Uses token bucket algorithm for bandwidth throttling
- Implements I/O rate limiting for disk operations
- Provides real-time bandwidth usage statistics
- Allows dynamic bandwidth limit changes

### 4. Socket Communication

Socket-based communication for remote monitoring and control:

- **TCP/IP Server**: Listens for client connections
- **Command Protocol**: Simple text-based protocol for commands
- **State Broadcasting**: Broadcasts state updates to connected clients
- **Security**: Basic authentication and encryption

Implementation details:
- Uses `TcpListener` and `TcpClient` for communication
- Implements a simple command protocol
- Uses JSON for data serialization
- Provides event-based notification system

### 5. Remote Monitoring Application

Dedicated application for remote monitoring and control:

- **Real-time Monitoring**: View backup job status in real-time
- **Remote Control**: Start, pause, resume, and stop jobs remotely
- **Job Management**: Create and edit jobs remotely
- **Notifications**: Receive notifications about job completion or errors

Implementation details:
- Uses the same MVVM pattern as the main application
- Implements socket communication client
- Provides a similar UI experience as the main application
- Supports reconnection and offline mode

### 6. Docker Support

Containerized deployment option:

- **Docker Image**: Ready-to-use Docker image
- **Volume Mounts**: Mount local directories for backup
- **Web Interface**: Access the application through a web interface
- **Multi-container Setup**: Optional setup with separate containers for components

Implementation details:
- Uses .NET 8.0 runtime container
- Implements a lightweight web interface using ASP.NET Core
- Provides volume mounts for data access
- Includes health checks and monitoring

## Data Files

### settings.json

Stores application settings with new options:

```json
{
  "BusinessSoftwareName": "calc.exe",
  "EncryptionExtensions": [".txt", ".doc", ".pdf"],
  "CryptoSoftPath": "C:\\Path\\To\\CryptoSoft.exe",
  "MaxParallelJobs": 5,
  "GlobalBandwidthLimit": 100,
  "RespectJobPriority": true,
  "DefaultJobPriority": "Medium",
  "RemoteAccessEnabled": true,
  "RemoteAccessPort": 8080,
  "DockerMode": false
}
```

### state.json

Stores the real-time state of backup jobs with enhanced information:

```json
[
  {
    "Name": "Documents",
    "State": "Active",
    "Priority": "High",
    "TotalFiles": 100,
    "TotalFilesRemaining": 75,
    "TotalSize": 1048576,
    "TotalSizeRemaining": 786432,
    "Progress": 25,
    "CurrentFile": "C:\\Users\\Username\\Documents\\file.txt",
    "CurrentTargetFile": "D:\\Backup\\Documents\\file.txt",
    "BandwidthUsage": 15.7,
    "BandwidthLimit": 50,
    "ExecutionMode": "Parallel",
    "ResourceUsage": {
      "CPU": 12.5,
      "Memory": 256,
      "DiskRead": 25.3,
      "DiskWrite": 18.7
    }
  },
  ...
]
```

### Log Files (YYYY-MM-DD.json/xml)

Daily log files with enhanced metrics:

JSON format example:
```json
[
  {
    "Timestamp": "2026-02-02T12:34:56.789Z",
    "BackupName": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents\\file.txt",
    "TargetPath": "D:\\Backup\\Documents\\file.txt",
    "FileSize": 1024,
    "TransferTime": 15,
    "EncryptionTime": 5,
    "BandwidthUsage": 15.7,
    "Priority": "High",
    "ExecutionMode": "Parallel"
  },
  ...
]
```

XML format example:
```xml
<Logs>
  <Log>
    <Timestamp>2026-02-02T12:34:56.789Z</Timestamp>
    <BackupName>Documents</BackupName>
    <SourcePath>C:\Users\Username\Documents\file.txt</SourcePath>
    <TargetPath>D:\Backup\Documents\file.txt</TargetPath>
    <FileSize>1024</FileSize>
    <TransferTime>15</TransferTime>
    <EncryptionTime>5</EncryptionTime>
    <BandwidthUsage>15.7</BandwidthUsage>
    <Priority>High</Priority>
    <ExecutionMode>Parallel</ExecutionMode>
  </Log>
</Logs>
```

### Remote Communication Protocol

Socket-based communication protocol:

Command format:
```
COMMAND [parameters]
```

Example commands:
```
LIST_JOBS
EXECUTE_JOB 1
PAUSE_JOB 1
RESUME_JOB 1
STOP_JOB 1
CREATE_JOB {"Name":"Backup1","SourcePath":"C:\\Source","TargetPath":"D:\\Target","Type":"Complete","Priority":"High"}
GET_STATE
```

Response format:
```
STATUS [status_code]
[JSON data if applicable]
```

Example responses:
```
STATUS 200
{"jobs":[{"id":1,"name":"Backup1","status":"Running","progress":45},...]}

STATUS 400
{"error":"Invalid job ID"}
```

## Error Handling

### File Operations

- Source file access errors are caught and logged
- Target directory creation failures are reported to the user
- File copy exceptions are logged with a negative transfer time
- Encryption failures are logged with a negative encryption time

### Parallel Execution

- Job conflicts are detected and resolved
- Resource exhaustion is handled by dynamic adjustment
- Failed jobs are retried with configurable retry policy
- Deadlock detection and prevention mechanisms

### Bandwidth Management

- Invalid bandwidth limits are corrected automatically
- Bandwidth monitoring failures are handled gracefully
- Dynamic adjustment if system cannot maintain requested limits
- Fallback to unlimited mode if throttling causes issues

### Socket Communication

- Connection failures are handled with retry mechanism
- Protocol errors are logged and reported
- Automatic reconnection for temporary network issues
- Graceful degradation to offline mode when necessary

### Business Software Detection

- Multiple detection methods ensure reliable operation
- Access denied errors are handled gracefully
- Detection is performed periodically to catch software that starts during backup

### Docker Container

- Container startup failures are logged with detailed diagnostics
- Volume mount issues are detected and reported
- Network configuration problems are diagnosed
- Resource constraints are handled with adaptive behavior

## Design Patterns

### MVVM Pattern

- **Models**: Data structures like BackupJob
- **Views**: XAML-based UI components
- **ViewModels**: Classes that handle UI logic and data binding

### Command Pattern

- `RelayCommand` class implements `ICommand` interface
- Commands encapsulate user actions (Execute, Delete, Pause, etc.)
- Enables clean separation between UI and logic

### Observer Pattern

- `INotifyPropertyChanged` implementation for data binding
- `BusinessSoftwareMonitor` uses events to notify about software status changes
- Socket server uses observer pattern for client notifications
- State changes propagate through the application

### Strategy Pattern

- Different logging strategies (JSON/XML) through the `ILogger` interface
- Encryption strategy in `CryptoService`
- Backup strategies (Complete/Differential)
- Bandwidth throttling strategies

### Factory Pattern

- `LoggerFactory` creates appropriate logger instances based on configuration
- `BackupStrategyFactory` creates appropriate backup strategy instances

### Singleton Pattern

- `SettingsManager` as a singleton for application settings
- `CryptoSoft` server mode as a singleton
- `SocketServer` as a singleton for remote communication

### Decorator Pattern

- `BandwidthLimitedStream` decorates standard streams with throttling
- `EncryptionStream` decorates streams with encryption capabilities

### Adapter Pattern

- `RemoteBackupAdapter` adapts local backup operations for remote execution
- `DockerEnvironmentAdapter` adapts application for Docker environment

## Integration with CryptoSoft

CryptoSoft has been enhanced with a server mode:

1. **Single Instance**: Runs as a single process to handle multiple encryption requests
2. **Request Queue**: Maintains a queue of encryption requests
3. **Priority Support**: Processes requests based on priority
4. **Performance Optimization**: Improved algorithms and memory usage

Integration flow:
1. CryptoSoft starts in server mode at application startup
2. `CryptoService.ShouldEncrypt()` determines if a file needs encryption
3. If yes, `EncryptFileAsync()` sends a request to the CryptoSoft server
4. CryptoSoft processes the request and returns the result
5. Encryption time is captured and logged

Command-line interface:
- Standard mode: `CryptoSoft.exe source target`
- Server mode: `CryptoSoft.exe --server [port]`
- Queue status: `CryptoSoft.exe --status`

## Socket Communication Protocol

The socket communication protocol is designed for remote monitoring and control:

1. **Connection**: Client connects to server on specified port
2. **Authentication**: Optional authentication with username/password
3. **Command Exchange**: Client sends commands, server responds
4. **State Updates**: Server broadcasts state updates to clients

Protocol details:
- Text-based for simplicity and debugging
- Commands are single line with space-separated parameters
- Responses include status code and optional JSON data
- State updates are pushed as JSON objects

Security considerations:
- Optional TLS encryption for secure communication
- Basic authentication mechanism
- IP address filtering
- Rate limiting to prevent abuse

## Docker Implementation

Docker support enables containerized deployment:

1. **Base Image**: Uses .NET 8.0 runtime as base
2. **Application Layer**: Adds EasySave and dependencies
3. **Configuration**: Environment variables for configuration
4. **Volumes**: Mount points for source and target directories

Container features:
- Web interface accessible on port 8080
- RESTful API for automation
- Volume mounts for data access
- Health check endpoint for monitoring

Docker Compose setup:
- Main EasySave container
- Optional separate CryptoSoft container
- Optional database container for extended logging
- Optional monitoring container

## Troubleshooting Guide

### Common Errors

1. **Parallel Execution Issues**
   - Check system resource usage
   - Verify maximum parallel jobs setting
   - Look for file access conflicts
   - Check for priority inversion issues

2. **Bandwidth Limitation Issues**
   - Verify bandwidth limits are reasonable
   - Check for competing network applications
   - Ensure disk I/O is not the bottleneck
   - Monitor actual bandwidth usage

3. **Remote Communication Issues**
   - Verify network connectivity
   - Check port availability and firewall settings
   - Verify server is running and listening
   - Check for protocol errors in logs

4. **Docker Container Issues**
   - Check container logs
   - Verify volume mounts are correct
   - Ensure network ports are properly exposed
   - Check resource allocation (CPU, memory)

### Debugging Techniques

- Enable Debug output to see detailed operation logs
- Use socket communication monitoring for remote issues
- Check resource usage during parallel execution
- Monitor bandwidth usage during transfers
- Use Docker logs for container issues