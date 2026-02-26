# Release Notes - EasySave v3.0

## Overview

EasySave v3.0 represents a significant advancement from the previous versions, introducing parallel backup execution, priority management, bandwidth control, and enhanced remote capabilities. This version focuses on performance optimization, resource management, and improved user experience for enterprise environments.

## New Features

### Parallel Backup Execution
- **Simultaneous Processing**: Execute multiple backup jobs in parallel
- **Configurable Parallelism**: Set the maximum number of concurrent backup jobs
- **Dynamic Resource Management**: Automatic adjustment based on system load
- **Progress Monitoring**: Real-time tracking of all parallel operations

### Job Control System
- **Individual Controls**: Play/Pause/Stop buttons for each backup job
- **Graceful Termination**: Proper cleanup when stopping jobs
- **Exit Verification**: Warning when closing application with active backups
- **Status Tracking**: Enhanced real-time status monitoring

### CryptoSoft Mono-Instance
- **Single Process**: CryptoSoft now runs as a single instance
- **Request Queue**: Global queue for all encryption requests
- **Resource Optimization**: Reduced memory footprint and improved performance
- **Mutex Implementation**: SemaphoreSlim for controlled access

### Command Line Interface Enhancements
- **Dedicated CLI Application**: Completely rewritten EasySaveCLI
- **Parallel Execution Support**: Run multiple jobs simultaneously from command line
- **Enhanced Parameter System**: More flexible command options
- **Consistent Output Directory**: All executables in a common output folder

### Bandwidth Management
- **Global Limitation**: Set maximum bandwidth for all backup operations
- **Per-Job Control**: Individual bandwidth limits for specific jobs
- **Large File Handling**: Special handling for files exceeding size threshold
- **Real-time Monitoring**: Track bandwidth usage during operations

### Priority Management
- **Job Prioritization**: Assign High/Medium/Low priority to backup jobs
- **Priority-based Execution**: Higher priority jobs execute first
- **Priority File Extensions**: Configure extensions to receive priority treatment
- **Strict Priority Rule**: No non-priority transfers when priority files are waiting

### Docker Integration
- **Containerized Deployment**: Run EasySave in Docker containers
- **Volume Mounting**: Easy access to source and target directories
- **Multi-container Setup**: Separate containers for different components
- **Centralized Logging**: Docker-optimized log management

## Improvements

### Architecture & Design
- **Enhanced MVVM Implementation**: Clearer separation of concerns
- **Improved Thread Management**: Better handling of background operations
- **Expanded Design Patterns**: Additional patterns for complex scenarios
- **Code Optimization**: Performance improvements throughout the codebase

### User Experience
- **Responsive Interface**: UI remains responsive during intensive operations
- **Enhanced Progress Visualization**: Better visual feedback for parallel jobs
- **Improved Error Reporting**: More detailed and actionable error messages
- **Consistent Status Updates**: Real-time updates across all interfaces

### Performance & Stability
- **Optimized File Handling**: More efficient processing of large file sets
- **Reduced Memory Usage**: Better memory management during operations
- **Enhanced Error Recovery**: Improved handling of network and I/O errors
- **Stability Improvements**: Reduced crashes and hangs in edge cases

### Security
- **Enhanced Encryption Integration**: Better handling of encryption processes
- **Improved Error Handling**: More robust recovery from encryption failures
- **Process Isolation**: Better separation between critical components

## Bug Fixes

- Fixed issue with parallel job execution causing occasional deadlocks
- Resolved memory leak during long-running backup operations
- Corrected handling of network path validation
- Fixed UI freezing when monitoring multiple simultaneous backups
- Resolved issue with job state persistence after application restart
- Fixed CryptoSoft path detection problems
- Corrected priority handling in mixed job scenarios
- Resolved issues with bandwidth limitation accuracy

## Technical Improvements

- Updated thread synchronization mechanisms
- Implemented SemaphoreSlim for controlled resource access
- Enhanced job scheduling algorithm
- Improved file system monitoring efficiency
- Added support for Docker containerization
- Enhanced logging with additional metrics
- Implemented priority queue for job management
- Added bandwidth throttling mechanisms

## Known Limitations

- Docker centralized logging requires additional configuration
- Priority file extensions limited to 10 extensions maximum
- Bandwidth limitation may not be precise on some network configurations
- Remote monitoring requires firewall configuration
- CryptoSoft mono-instance requires all components to be on same machine

## System Requirements

- Windows 10/11 or Windows Server 2019/2022
- .NET 8.0 Runtime
- 150MB free disk space for application
- Sufficient disk space for backup operations
- Docker Desktop (for containerized deployment)

## Installation

1. Extract all files to a directory of your choice
2. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe) are located in the output folder
3. For Docker deployment, use the provided Dockerfile and docker-compose.yml

## Upgrading from v2.0

- Your existing backup jobs will be automatically imported
- Settings will be preserved but may require review for new options
- Log files from v2.0 are compatible with v3.0
- CryptoSoft now runs in mono-instance mode by default
