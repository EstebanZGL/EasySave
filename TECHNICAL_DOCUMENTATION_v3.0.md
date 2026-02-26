# Technical Documentation EasySave v3.0

## General Architecture

EasySave v3.0 is a backup application developed in C# (.NET 8.0) following the MVVM architecture pattern. The project is divided into several main components:

1. **EasySave**: Main WPF application with graphical user interface
2. **EasySaveCLI**: Command-line interface for automation and scripting
3. **EasyLog**: Class library for log management
4. **CryptoSoft**: External encryption utility
5. **EasySave.LogServer**: Centralized logging service for Docker environments

## Project Structure

```
EasySave/
├── Models/
│   ├── BackupJob.cs           # Data model for backup jobs
│   ├── BackupJobState.cs      # State tracking model
│   ├── JobStatus.cs           # Status enumeration
│   └── LogCentralizationSettings.cs # Log centralization configuration
├── ViewModels/
│   ├── MainViewModel.cs       # Main view model for application logic
│   ├── SettingsViewModel.cs   # Settings management
│   ├── ActiveBackupJobViewModel.cs # Active job monitoring
│   └── ViewModelBase.cs       # Base class for view models
├── Views/
│   ├── MainWindow.xaml        # Main application window
│   ├── BackupJobDialog.xaml   # Dialog for creating/editing jobs
│   └── SettingsWindow.xaml    # Settings window
├── Services/
│   ├── BackupService.cs       # Sequential backup execution
│   ├── ParallelBackupService.cs # Parallel backup management
│   ├── BusinessSoftwareMonitor.cs # Business software detection
│   ├── CryptoService.cs       # File encryption
│   ├── StateManager.cs        # Real-time state management
│   └── BackupJobRepository.cs # Job persistence
├── Commands/
│   └── RelayCommand.cs        # Command implementation
└── Converters/
    ├── BoolToColorConverter.cs # UI value converters
    ├── BoolToVisibilityConverter.cs # Visibility converters
    └── NullToBoolConverter.cs # Null handling converters

EasySaveCLI/
├── Program.cs                 # Entry point for CLI application
└── EasySaveCLIController.cs   # Command processing logic

EasyLog/
├── ILogger.cs                 # Base logger interface
├── IEncryptionLogger.cs       # Extended logger with encryption support
├── JsonLogger.cs              # JSON implementation
├── XmlLogger.cs               # XML implementation
└── RemoteLogger.cs            # Remote logging implementation

CryptoSoft/
└── Program.cs                 # Encryption utility

EasySave.LogServer/
├── Controllers/
│   └── LogsController.cs      # API endpoints for log management
├── Models/
│   └── LogEntry.cs            # Log entry data model
├── Services/
│   └── LogStorageService.cs   # Log storage and retrieval
└── Program.cs                 # Entry point for log server
```

## Main Components

### BackupJob Model

Represents a backup job with the following properties:
- **Name**: Job name (must be unique)
- **SourcePath**: Source path of files to backup
- **TargetPath**: Target path where to save files
- **Type**: Backup type (Complete or Differential)
- **LastBackupTime**: Timestamp of the last successful backup

The `Validate()` method ensures that:
- Name is not empty
- Source path exists
- Target path is specified

### ParallelBackupService

Service responsible for executing multiple backup operations in parallel:

Key methods:
- `ExecuteBackupJobAsync(BackupJob job)`: Executes a backup job asynchronously
- `OnBackupJobStatusChanged(object sender, BackupJobStatusEventArgs e)`: Handles job status changes
- `GetActiveJobs()`: Returns currently active jobs
- `StartBackupJobAsync(BackupJob job)`: Starts a new backup job
- `PauseJobAsync(string jobName)`: Pauses a running job
- `ResumeJobAsync(string jobName)`: Resumes a paused job
- `StopJob(string jobName)`: Stops a running job
- `SetPriorityExtensions(List<string> extensions)`: Sets priority file extensions
- `SetLargeFileThreshold(long sizeInBytes)`: Sets threshold for large file handling

Implementation details:
- Uses `SemaphoreSlim` to control the degree of parallelism
- Implements a job queue with priority support
- Uses `CancellationTokenSource` for job control
- Monitors system resources during execution

### CryptoService

Service responsible for file encryption:

Key methods:
- `ShouldEncrypt(string filePath)`: Determines if a file should be encrypted
- `EncryptFileAsync(string sourcePath, string targetPath)`: Encrypts a file asynchronously

Implementation details:
- Uses a shared `SemaphoreSlim` to ensure single-instance access to CryptoSoft
- Implements extension-based encryption selection
- Handles encryption errors gracefully

### BusinessSoftwareMonitor

Service that monitors for business software:

Key methods:
- `StartMonitoring()`: Starts monitoring for business software
- `StopMonitoring()`: Stops monitoring
- `IsBusinessSoftwareRunning()`: Checks if specified business software is running
- `OnBusinessSoftwareStatusChanged()`: Raises event when status changes

Implementation details:
- Uses timer-based polling to check for business software
- Implements multiple detection methods (process name, window title)
- Raises events to notify subscribers of status changes

### StateManager

Manages the real-time state of backup jobs:

Key methods:
- `UpdateStateAsync(string jobName, BackupJobState state)`: Updates job state
- `SaveStateAsync()`: Persists state to disk
- `LoadState()`: Loads state from disk

Implementation details:
- Uses JSON serialization for state persistence
- Implements thread-safe state updates
- Provides real-time state information for UI and logging

### EasySaveCLIController

Manages the command-line interface functionality:

Key methods:
- `RunWithArgsAsync(string[] args)`: Processes command-line arguments
- `ExecuteCommandLineArgsAsync(string[] args)`: Executes specified jobs
- `ParseJobIndexes(string input)`: Parses job numbers from command line
- `DisplayCommandLineHelp()`: Shows available commands and syntax

Implementation details:
- Supports both interactive and non-interactive modes
- Implements parallel execution options
- Provides comprehensive help system

## Key Features in v3.0

### 1. Parallel Backup Execution

EasySave v3.0 introduces the ability to execute multiple backup jobs simultaneously:

- **Configurable Parallelism**: Set the maximum number of concurrent backup jobs
- **Job Control**: Individual control for each parallel job
- **Resource Management**: Ensures efficient use of system resources

Implementation details:
- Uses `Task.Run()` for asynchronous execution
- Implements `SemaphoreSlim` to control the degree of parallelism
- Uses `ConcurrentDictionary<string, JobInfo>` to track active jobs
- Implements individual `CancellationTokenSource` for each job

### 2. CryptoSoft Mono-Instance

Enhanced CryptoSoft integration to run as a single instance:

- **Shared Access**: Multiple backup jobs share a single CryptoSoft instance
- **Queue Management**: Requests are processed in order
- **Resource Optimization**: Reduced memory footprint

Implementation details:
- Uses `SemaphoreSlim` with `maxCount: 1` to ensure single access
- Implements asynchronous waiting with timeout
- Provides fallback mechanism for encryption failures

### 3. Job Control System

Enhanced job control capabilities:

- **Individual Controls**: Each job can be paused, resumed, or stopped
- **Status Tracking**: Real-time status updates
- **Graceful Termination**: Clean shutdown of jobs

Implementation details:
- Uses `CancellationTokenSource` for job control
- Implements state machine for job status management
- Provides event-based notification system

### 4. Priority Management

Job prioritization system:

- **Priority Extensions**: Configure file extensions that should be prioritized
- **Strict Priority Rule**: Non-priority transfers are blocked when priority files are waiting

Implementation details:
- Uses file extension checking to determine priority
- Implements queue management based on priority
- Provides configuration options for priority settings

### 5. Large File Handling

Special handling for large files:

- **Size Threshold**: Configure what constitutes a "large" file
- **Concurrent Limitation**: Prevents multiple large files from transferring simultaneously

Implementation details:
- Uses file size checking before transfer
- Implements additional semaphore for large file control
- Provides configuration options for threshold settings

### 6. Docker Support

Containerized deployment option:

- **Log Server**: Dedicated service for centralized logging
- **Volume Mounts**: Mount local directories for backup
- **Multi-container Setup**: Separate containers for components

Implementation details:
- Uses ASP.NET Core for log server
- Implements REST API for log access
- Provides Docker configuration files

## Data Files

### settings.json

Stores application settings:

```json
{
  "BusinessSoftwareName": "calc.exe",
  "EncryptionExtensions": [".txt", ".doc", ".pdf"],
  "CryptoSoftPath": "C:\\Path\\To\\CryptoSoft.exe",
  "MaxParallelJobs": 5,
  "PriorityExtensions": [".exe", ".dll"],
  "LargeFileThreshold": 104857600,
  "LogCentralization": {
    "Enabled": false,
    "ServerUrl": "http://localhost:5000",
    "Mode": "Hybrid"
  }
}
```

### state.json

Stores the real-time state of backup jobs:

```json
[
  {
    "Name": "Documents",
    "State": "Active",
    "TotalFiles": 100,
    "TotalFilesRemaining": 75,
    "TotalSize": 1048576,
    "TotalSizeRemaining": 786432,
    "Progress": 25,
    "CurrentFile": "C:\\Users\\Username\\Documents\\file.txt",
    "CurrentTargetFile": "D:\\Backup\\Documents\\file.txt"
  },
  ...
]
```

### Log Files (YYYY-MM-DD.json/xml)

Daily log files with encryption metrics:

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
    "EncryptionTime": 5
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
  </Log>
</Logs>
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
- Failed jobs are logged with appropriate error codes
- Deadlock detection and prevention mechanisms

### Business Software Detection

- Multiple detection methods ensure reliable operation
- Access denied errors are handled gracefully
- Detection is performed periodically to catch software that starts during backup

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
- Job status changes propagate through the application

### Strategy Pattern

- Different logging strategies (JSON/XML) through the `ILogger` interface
- Encryption strategy in `CryptoService`
- Backup strategies (Complete/Differential)

### Factory Pattern

- `LoggerFactory` creates appropriate logger instances based on configuration

### Singleton Pattern

- `StateManager` as a singleton for application settings

## Integration with CryptoSoft

CryptoSoft has been enhanced with mono-instance support:

1. **Single Access**: Only one backup job can access CryptoSoft at a time
2. **Queue Management**: Jobs wait for access to CryptoSoft
3. **Performance Optimization**: Reduced process creation overhead

Integration flow:
1. `CryptoService.ShouldEncrypt()` determines if a file needs encryption
2. If yes, `EncryptFileAsync()` attempts to acquire the semaphore
3. Once acquired, CryptoSoft is executed to encrypt the file
4. Encryption time is captured and logged
5. The semaphore is released for the next job

## Troubleshooting Guide

### Common Errors

1. **Parallel Execution Issues**
   - Check system resource usage
   - Verify maximum parallel jobs setting
   - Look for file access conflicts

2. **CryptoSoft Issues**
   - Verify CryptoSoft path is correct
   - Check for adequate permissions
   - Ensure no process is blocking CryptoSoft execution

3. **Business Software Detection Issues**
   - Verify process name is correct
   - Check for adequate permissions to query processes
   - Try alternative detection methods

### Debugging Techniques

- Enable Debug output to see detailed operation logs
- Check state.json for current job status
- Monitor system resource usage during parallel execution
- Check log files for encryption errors