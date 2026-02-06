# Technical Documentation EasySave v1.0

## General Architecture

EasySave is a backup application developed in C# (.NET 8.0) following a modular architecture. The project is divided into two main components:

1. **EasySave**: Main console application
2. **EasyLog**: Class library for log management

## Project Structure

```
EasySave/
├── Models/
│   └── BackupJob.cs           # Data model for backup jobs
├── Services/
│   ├── BackupJobManager.cs    # Job management (CRUD, persistence)
│   ├── BackupService.cs       # Backup execution
│   ├── StateManager.cs        # Real-time state management
│   └── TranslationService.cs  # Multilingual support
├── EasySaveController.cs      # Main application controller
└── Program.cs                 # Application entry point

EasyLog/
├── ILogger.cs                 # Interface for loggers
└── JsonLogger.cs              # JSON implementation of the logger
```

## Main Components

### BackupJob Model

Represents a backup job with the following properties:
- **Name**: Job name (must be unique)
- **SourcePath**: Source path of files to backup
- **TargetPath**: Target path where to save files
- **Type**: Backup type (Complete or Differential)

The `Validate()` method ensures that:
- Name is not empty
- Source path exists
- Target path is specified

### BackupJobManager Service

Manages the creation, storage, loading, and deletion of backup jobs. Jobs are persisted in a JSON file (`config.json`).

Key methods:
- `LoadJobs()`: Loads jobs from the configuration file
- `SaveJobs()`: Saves jobs to the configuration file
- `GetJob(int index)`: Retrieves a specific job by index
- `GetJobs()`: Retrieves all jobs
- `CreateJob(string name, string sourcePath, string targetPath, BackupType type)`: Creates a new job
- `DeleteJob(int index)`: Deletes a job by index

Limitations:
- Maximum 5 backup jobs
- Unique job names

### BackupService

Service responsible for executing backup operations:

Key methods:
- `ExecuteBackupJobAsync(BackupJob job)`: Executes a backup job asynchronously
- `RemoveDeletedFilesAsync(string backupName, string sourcePath, string targetPath)`: Removes files in the target that don't exist in the source
- `RemoveEmptyDirectories(string directory)`: Recursively removes empty directories

Backup types:
- **Complete backup**: Copies all files from the source folder to the target folder and removes files in the target that don't exist in the source
- **Differential backup**: Copies only files that are new or have been modified since the last backup

### StateManager

Manages the real-time state of backup jobs, stored in `state.json`. 

Key methods:
- `SaveStateAsync()`: Saves the current state to the state file
- `LoadState()`: Loads the state from the state file
- `UpdateStateAsync(...)`: Updates the state of a specific job

For each active job, it maintains:
- Job name
- Current state (Active, Inactive, Completed, Error)
- Progress metrics:
  - Total files and remaining files
  - Total size and remaining size
  - Current file being processed
  - Current target file

### TranslationService

Manages multilingual support (FR/EN) for the application.

Key methods:
- `GetTranslation(string key)`: Gets the translation for a specific key
- `ChangeLanguage(string language)`: Changes the current language
- `ToggleLanguage()`: Toggles between available languages

### EasySaveController

Main controller that coordinates all operations:

- Initializes all services
- Handles command-line arguments
- Displays the interactive menu
- Manages backup job execution
- Tracks last backup times for each job

Key methods:
- `RunWithArgsAsync(string[] args)`: Entry point for application execution
- `ShowMainMenuAsync()`: Displays the main menu
- `CreateBackupJobAsync()`: Creates a new backup job
- `ExecuteBackupJobsAsync()`: Executes selected backup jobs
- `ManageBackupJobsAsync()`: Displays and manages existing backup jobs
- `DeleteBackupJobAsync()`: Deletes a selected backup job
- `ExecuteCommandLineArgsAsync(string[] args)`: Processes command-line arguments
- `ParseJobIndexes(string input)`: Parses job indexes from user input

### EasyLog Library

Library responsible for logging backup operations.

Components:
- `ILogger`: Interface defining logging operations
- `JsonLogger`: Implementation that logs to JSON files
- `LogEntry`: Data structure for log entries

Key methods:
- `LogTransferAsync(string backupName, string sourcePath, string targetPath, long fileSize, long transferTime)`: Logs a file transfer operation

## Data Files

### config.json

Stores the configuration of backup jobs:

```json
[
  {
    "Name": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents",
    "TargetPath": "D:\\Backup\\Documents",
    "Type": 0  // 0 = Complete, 1 = Differential
  },
  ...
]
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

### YYYY-MM-DD.json

Daily log files stored in the `logs` directory:

```json
[
  {
    "Timestamp": "2026-02-02T12:34:56.789Z",
    "BackupName": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents\\file.txt",
    "TargetPath": "D:\\Backup\\Documents\\file.txt",
    "FileSize": 1024,
    "TransferTime": 15
  },
  ...
]
```

### lastbackups.json

Stores the timestamp of the last successful backup for each job:

```json
{
  "Documents": "2026-02-02T12:34:56.789Z",
  "Pictures": "2026-02-01T10:15:30.123Z"
}
```

## Concurrency Management

Only one backup process runs at a time, preventing potential conflicts

## Command Line Interface

The application supports various command-line arguments:

- `-h`, `--help`, `/?`: Display help information
- `all`, `0`: Execute all backup jobs sequentially
- `1`: Execute backup job #1
- `1-3`: Execute backup jobs #1, #2, and #3
- `1;3;5`: Execute backup jobs #1, #3, and #5

PowerShell syntax variations:
- `.\EasySave.exe 1 3 5`: Execute backup jobs #1, #3, and #5
- `.\EasySave.exe "1-3"`: Execute backup jobs #1, #2, and #3
- `.\EasySave.exe "1;3;5"`: Execute backup jobs #1, #3, and #5

## Error Handling

### File Operations

- Source file access errors are caught and logged
- Target directory creation failures are reported to the user
- File copy exceptions are logged with a negative transfer time

### State Management

- Jobs that encounter errors are marked with the `Error` state
- The application attempts to gracefully handle JSON serialization errors

### Backup Job Validation

- Source directories are verified before backup starts
- Invalid job configurations are rejected with appropriate error messages

## Troubleshooting Guide

### Common Errors

1. **File Access Error**
   - Check permissions of source and target folders
   - Check if files are locked by other applications
   - Verify that the user has read/write access to the logs directory

2. **JSON Serialization Error**
   - Check the integrity of config.json, state.json, and lastbackups.json files
   - In case of corruption, delete the files (they will be recreated)

3. **"Out of memory" Error**
   - Occurs when backing up very large files
   - Recommend the user to divide jobs into smaller units

### Error Logs

- Transfer errors are indicated in logs by a negative transfer time
- A transfer time of `-1` indicates a file access error

## Known Limitations

1. No file encryption
2. Maximum 5 backup jobs
3. No parallel backups
4. No resumption after interruption
5. No file compression
6. No network path validation
7. No priority system for backup jobs

## Planned Evolutions

1. **Version 1.1**: XML format support for logs
2. **Version 2.0**: WPF graphical interface with MVVM architecture
3. **Version 3.0**: Parallel backups, priority management, and Docker containerization