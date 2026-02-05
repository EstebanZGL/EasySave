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

### BackupJob

Represents a backup job with the following properties:
- **Name**: Job name
- **SourcePath**: Source path of files to backup
- **TargetPath**: Target path where to save files
- **Type**: Backup type (Complete or Differential)

### BackupJobManager

Manages the creation, storage, loading, and deletion of backup jobs. Jobs are persisted in a JSON file (`config.json`).

Limitations:
- Maximum 5 backup jobs
- Unique job names

### BackupService

Service responsible for executing backups:
- Complete backup: Copies all files from the source folder to the target folder
- Differential backup: Copies only files modified since the last complete backup

### StateManager

Manages the real-time state of backup jobs, stored in `state.json`. For each active job, it maintains:
- Job name
- Current state (Active, Not Active)
- Progress (files processed/total, size processed/total)
- Current file being processed

### TranslationService

Manages multilingual support (FR/EN) for the application.

### EasyLog

Library responsible for logging backup operations. Logs are stored in JSON format in daily files (`YYYY-MM-DD.json`).

## Data Files

### config.json

Stores the configuration of backup jobs:

```json
[
  {
    "Name": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents",
    "TargetPath": "D:\\Backup\\Documents",
    "Type": 1
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
    "CurrentFile": "C:\\Users\\Username\\Documents\\file.txt"
  },
  ...
]
```

### YYYY-MM-DD.json

Daily log files:

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

## Troubleshooting Guide

### Common Errors

1. **File Access Error**
   - Check permissions of source and target folders
   - Check if files are locked by other applications

2. **JSON Serialization Error**
   - Check the integrity of config.json and state.json files
   - In case of corruption, delete the files (they will be recreated)

3. **"Out of memory" Error**
   - Occurs when backing up very large files
   - Recommend the user to divide jobs into smaller units

### Error Logs

Transfer errors are indicated in logs by a negative transfer time. For example, a transfer time of `-1` indicates a file access error.

## Known Limitations

1. No file encryption
2. Maximum 5 backup jobs
3. No parallel backups
4. No resumption after interruption
5. No file compression

## Planned Evolutions

1. **Version 1.1**: XML format support for logs
2. **Version 2.0**: WPF graphical interface with MVVM architecture
3. **Version 3.0**: Parallel backups, priority management, and Docker containerization