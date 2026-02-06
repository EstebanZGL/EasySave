# EasySave

## Overview

EasySave is a robust and scalable backup solution developed by ProSoft. This console application allows you to create and execute up to 5 different backup jobs, with support for both complete and differential backups.

![Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Language](https://img.shields.io/badge/language-C%23-green)

## Features

- **Backup Job Management**: Creation, execution, and monitoring of up to 5 jobs
- **Backup Types**: Support for complete and differential backups
- **Multilingual Interface**: Support for French and English
- **Logging**: Detailed logs in JSON format
- **Real-time Monitoring**: Status and progress of backup jobs
- **Command Line Execution**: Ability to execute specific jobs via arguments

## Project Structure

The project is divided into two main components:

- **EasySave**: Main console application
- **EasyLog**: Logging management library

## Prerequisites

- Windows 10/11
- .NET 8.0 Runtime
- Access rights to source and target folders

## Installation

1. Download the latest version from the releases page
2. Extract the archive to the folder of your choice
3. Launch the application via `EasySave.exe`

## Quick Start

### Via Console Interface

1. Launch `EasySave.exe`
2. Follow the on-screen instructions to create and execute backup jobs

### Via Command Line

Execute specific jobs directly:

```
EasySave.exe 1     # Executes job #1
EasySave.exe 1-3   # Executes jobs #1 to #3
EasySave.exe "1;3;5" # Executes jobs #1, #3, and #5
```

## Documentation

- [User Manual](./USER_MANUAL.md) - Guide for end users
- [Technical Documentation](./TECHNICAL_DOCUMENTATION.md) - Documentation for technical support and developers

## Roadmap

- **Version 1.1**: XML format support for logs
- **Version 2.0**: WPF graphical interface with MVVM architecture
- **Version 3.0**: Parallel backups, priority management, and Docker containerization
