# EasySave

## Overview

EasySave is a robust and scalable backup solution developed by ProSoft. This WPF application allows you to create and execute an unlimited number of backup jobs, with support for both complete and differential backups, file encryption, and business software detection.

![Version](https://img.shields.io/badge/version-2.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Language](https://img.shields.io/badge/language-C%23-green)
![Architecture](https://img.shields.io/badge/architecture-MVVM-orange)

## Features

### Core Functionality
- **Backup Job Management**: Creation, execution, and monitoring of unlimited backup jobs
- **Backup Types**: Support for complete and differential backups
- **Graphical User Interface**: Intuitive WPF interface with real-time feedback
- **Multilingual Interface**: Support for French and English

### Advanced Features
- **File Encryption**: Selective encryption of files using CryptoSoft
- **Business Software Detection**: Automatic detection of specified business software
- **Pause/Resume**: Automatic pausing of backups when business software is running
- **Enhanced Logging**: Detailed logs in JSON or XML format with encryption metrics
- **Real-time Monitoring**: Visual status and progress of backup jobs

## Project Structure

The project is divided into three main components:

- **EasySave**: Main WPF application with MVVM architecture
- **EasyLog**: Logging management library
- **CryptoSoft**: External encryption utility

## Prerequisites

- Windows 10/11
- .NET 8.0 Runtime
- Access rights to source and target folders
- Administrator rights recommended for business software detection

## Installation

1. Download the latest version from the releases page
2. Extract the archive to the folder of your choice
3. Ensure CryptoSoft.exe is in the same directory as EasySave.exe
4. Launch the application via `EasySave.exe`

## Quick Start

### Creating a Backup Job
1. Click the "New Job" button
2. Enter a name, select source and destination folders
3. Choose the backup type (Complete or Differential)
4. Click "Save"

### Executing Backup Jobs
- Select a job and click "Execute" to run a single job
- Select multiple jobs and click "Execute Selected" to run multiple jobs

### Configuring Settings
1. Click the "Settings" button
2. Configure business software detection, encryption settings, and preferences
3. Changes are saved automatically

## Documentation

- [User Manual](./USER_MANUAL.md) - Guide for end users
- [Technical Documentation](./TECHNICAL_DOCUMENTATION.md) - Documentation for technical support and developers
- [Release Notes](./RELEASE_NOTES_v2.0.md) - What's new in version 2.0

## Roadmap

- **Version 3.0**: Parallel backups, priority management, bandwidth limitation, and Docker containerization

## Legacy Features

Command line execution is still supported for backward compatibility:

```
EasySave.exe 1     # Executes job #1
EasySave.exe 1-3   # Executes jobs #1 to #3
EasySave.exe "1;3;5" # Executes jobs #1, #3, and #5
```