# Release Notes - EasySave v2.0

## Overview

EasySave v2.0 represents a major evolution from the console-based v1.x versions, introducing a complete graphical user interface, a dedicated command-line interface, and several powerful new features. This version focuses on improved user experience, file encryption capabilities, and business software detection.

## New Features

### Dual Interface
- **Complete WPF Interface**: Intuitive graphical interface replacing the console application
- **Dedicated CLI Application**: New EasySaveCLI.exe for automation and scripting
- **Common Output Directory**: All executables now located in a central 'output' folder

### Graphical User Interface
- **Real-time Progress Monitoring**: Visual feedback during backup operations
- **Streamlined Job Management**: Create, edit, and delete jobs with just a few clicks
- **Visual Settings Panel**: Configure all options through a dedicated interface

### Command Line Interface
- **Job Selection**: Execute specific jobs by number (e.g., EasySaveCLI.exe 1,2,3)
- **Batch Execution**: Run all jobs with a single command (--all parameter)
- **Interactive Mode**: Menu-driven interface when run without parameters
- **Help System**: Built-in command documentation (--help parameter)

### File Encryption
- **Selective Encryption**: Encrypt specific file types during backup
- **CryptoSoft Integration**: Seamless integration with the external CryptoSoft utility
- **Performance Tracking**: Measurement and logging of encryption time
- **Fallback Mechanism**: Files are still backed up even if encryption fails

### Business Software Detection
- **Automatic Detection**: Detects when specified business software is running
- **Smart Backup Management**: 
  - Prevents backup start when business software is running
  - Automatically pauses backups when business software starts
  - Automatically resumes backups when business software closes
- **Multi-method Detection**: Uses several techniques to reliably detect running software

### Enhanced Logging
- **Encryption Metrics**: Logs now include encryption time for encrypted files
- **Structured Storage**: Logs organized in dedicated folders by format (JSON/XML)
- **Improved Format Selection**: Easily switch between JSON and XML formats

### Unlimited Backup Jobs
- **No Job Limit**: Removed the 5-job limit from v1.x
- **Improved Job Organization**: Better management of multiple jobs

## Improvements

### Architecture & Design
- **MVVM Architecture**: Complete refactoring to Model-View-ViewModel architecture
- **Enhanced Design Patterns**: Implementation of Observer, Command, and Strategy patterns
- **Improved Modularity**: Better separation of concerns for maintainability
- **Centralized Build Output**: All executables now generated in a common output folder

### User Experience
- **Intuitive Navigation**: Streamlined workflow for common operations
- **Visual Feedback**: Clear status indicators for backup jobs
- **Persistent Settings**: User preferences are saved between sessions
- **Improved Multilingual Support**: Enhanced translations and language switching
- **Flexible Usage Options**: Choose between GUI and CLI based on needs

### Performance & Stability
- **Background Processing**: Backup operations run in background threads for responsive UI
- **Improved Error Handling**: Better recovery from common error conditions
- **Enhanced File Management**: More efficient handling of large file sets

## Bug Fixes

- Fixed issue with deletion of files in complete backup mode
- Corrected handling of special characters in file paths
- Resolved language switching issues
- Fixed log format persistence problems
- Improved handling of access denied errors for system processes
- Resolved UI freezing during long backup operations
- Fixed validation issues in job creation dialog

## Technical Improvements

- Updated to .NET 8.0 framework
- Enhanced EasyLog.dll with IEncryptionLogger interface
- Improved state management system
- Added robust business software monitoring service
- Implemented enhanced process detection techniques
- Added proper resource cleanup with IDisposable implementation
- Centralized build output configuration for all projects

## Known Limitations

- Business software detection may require administrator privileges on some systems
- No backup resumption after application restart
- No file compression capabilities
- No network path validation
- Settings changes may require application restart to take full effect

## System Requirements

- Windows 10/11 or Windows Server 2019/2022
- .NET 8.0 Runtime
- 100MB free disk space for application
- Sufficient disk space for backup operations

## Installation

1. Extract all files to a directory of your choice
2. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe) are located in the output folder
3. Run EasySave.exe for the graphical interface or EasySaveCLI.exe for command-line operation

## Upgrading from v1.x

- Your existing backup jobs will be automatically imported
- Log files from v1.x are compatible with v2.0
- Command-line execution is now handled by the dedicated EasySaveCLI.exe application

## Looking Ahead

The upcoming v3.0 will focus on:
- Parallel backup execution
- Priority management for file types
- Bandwidth limitation
- Docker containerization
- CryptoSoft mono-instance with queue management