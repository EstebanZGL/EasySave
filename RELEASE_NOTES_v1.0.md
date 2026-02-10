# EasySave v1.0 Release Notes

## Overview
This initial version (v1.0) provides a solid foundation for file backup operations through an intuitive console interface, supporting both complete and differential backup strategies.

## Key Features

### Backup Management
- **Multiple Backup Jobs**: Create and manage up to 5 distinct backup jobs
- **Two Backup Types**:
  - Complete backup: Full copy of all source files
  - Differential backup: Copy only files modified since the last complete backup
- **Job Persistence**: All backup configurations are automatically saved and loaded between sessions

### User Interface
- **Console Interface**: Clean, intuitive menu-driven interface
- **Multilingual Support**: Full support for English and French languages
- **Command Line Execution**: Execute specific backup jobs directly from command line
  - Single job: `EasySave.exe 1`
  - Range of jobs: `EasySave.exe 1-3`
  - Specific jobs: `EasySave.exe "1;3;5"`

### Monitoring & Logging
- **Real-time Progress**: Visual feedback during backup operations
- **Daily Log Files**: Detailed JSON logs of all file operations (stored in `logs/YYYY-MM-DD.json`)
- **State Tracking**: Real-time state information for active backups (stored in `state.json`)

### Reliability
- **Error Handling**: Robust error detection and reporting
- **Deleted File Management**: Proper handling of deleted files in complete backups

## Technical Highlights
- Developed in C# (.NET 8.0)
- Modular architecture for maintainability and future expansion
- Separate EasyLog.dll library for logging operations
- Implementation of design patterns:
  - Strategy Pattern for logging mechanisms
  - Factory Pattern for logger creation
  - Decorator Pattern for performance metrics
  - Dependency Injection for service composition
  - Template Method for backup algorithm structure
  - Command Pattern for backup operations
  - State Pattern for backup state management

## System Requirements
- Windows operating system
- .NET 8.0 Runtime
- Sufficient disk space for backup operations

## Installation
1. Extract the EasySave ZIP file to any folder
2. Run EasySave.exe to start the application

## Known Limitations
- Maximum of 5 backup jobs
- No file encryption
- No parallel backup execution
- No backup resumption after interruption
- No file compression

## Future Roadmap
- **Version 1.1**: XML format support for logs
- **Version 2.0**: WPF graphical interface with MVVM architecture
- **Version 3.0**: Parallel backups, priority management, and Docker containerization
