# EasySave

## Overview

EasySave is a robust and scalable backup solution developed by ProSoft. This application allows you to create and execute an unlimited number of backup jobs, with support for both complete and differential backups, file encryption, and business software detection. Available in both GUI and CLI versions.

![Version](https://img.shields.io/badge/version-3.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Language](https://img.shields.io/badge/language-C%23-green)
![Architecture](https://img.shields.io/badge/architecture-MVVM-orange)

## Features

### Core Functionality
- **Backup Job Management**: Creation, execution, and monitoring of unlimited backup jobs
- **Backup Types**: Support for complete and differential backups
- **Dual Interface**: Graphical WPF interface and dedicated command-line interface
- **Multilingual Interface**: Support for French and English

### Advanced Features
- **File Encryption**: Selective encryption of files using CryptoSoft
- **Business Software Detection**: Automatic detection of specified business software
- **Pause/Resume**: Automatic pausing of backups when business software is running
- **Enhanced Logging**: Detailed logs in JSON or XML format with encryption metrics
- **Real-time Monitoring**: Visual status and progress of backup jobs

### New in Version 3.0
- **Parallel Backups**: Execute multiple backup jobs simultaneously
- **Priority Management**: Set priorities for different file types
- **Bandwidth Limitation**: Control network usage during backups
- **Docker Support**: Run EasySave in containerized environments
- **Socket Communication**: Remote monitoring and control via network
- **CryptoSoft Optimization**: Single-instance encryption with queue management

## Project Structure

The project is divided into five main components:

- **EasySave**: Main WPF application with MVVM architecture
- **EasySaveCLI**: Command-line interface for automation and scripting
- **EasyLog**: Logging management library
- **CryptoSoft**: External encryption utility
- **EasySaveRemote**: Remote monitoring application

All executables are generated in a common `output` folder for easy access.

## Prerequisites

- Windows 10/11
- .NET 8.0 Runtime
- Access rights to source and target folders
- Administrator rights recommended for business software detection
- Docker Desktop (optional, for containerized deployment)

## Installation

1. Download the latest version from the releases page
2. Extract the archive to the folder of your choice
3. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe, EasySaveRemote.exe) are located in the `output` folder
4. Launch the application via `output\EasySave.exe` for GUI or `output\EasySaveCLI.exe` for CLI

### Docker Installation
1. Pull the Docker image: `docker pull prosoft/easysave:3.0`
2. Run the container: `docker run -d -p 8080:8080 prosoft/easysave:3.0`
3. Access the web interface at `http://localhost:8080`

## Quick Start

### Using the Graphical Interface
1. Launch `output\EasySave.exe`
2. Click "New Job" to create a backup job
3. Select jobs and click "Execute" to run them

### Using the Command Line Interface
- `EasySaveCLI.exe 1,2,3` - Execute jobs 1, 2 and 3
- `EasySaveCLI.exe --all` - Execute all jobs
- `EasySaveCLI.exe --parallel 5` - Execute all jobs with max 5 parallel jobs
- `EasySaveCLI.exe --priority high` - Execute only high priority jobs
- `EasySaveCLI.exe --help` - Display available commands
- Run without parameters for interactive menu mode

### Remote Monitoring
1. Launch `output\EasySaveRemote.exe`
2. Enter the server IP address and port
3. Connect to view and control backup jobs remotely

### Configuring Settings
1. Click the "Settings" button in the GUI
2. Configure business software detection, encryption settings, and preferences
3. Set bandwidth limitations and parallel execution options
4. Changes are saved automatically

## Documentation

- [User Manual](./USER_MANUAL.md) - Guide for end users
- [Technical Documentation](./TECHNICAL_DOCUMENTATION.md) - Documentation for technical support and developers
- [Release Notes](./RELEASE_NOTES_v3.0.md) - What's new in version 3.0
