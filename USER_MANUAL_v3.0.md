# User Manual - EasySave v3.0

## Introduction

EasySave v3.0 is a robust backup software developed by ProSoft, featuring parallel backups, priority management, bandwidth limitation, and a dual interface system.

## Installation

### Standard Installation
1. Unzip the EasySave ZIP file into your preferred folder
2. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe) are in the `output` folder
3. Ensure CryptoSoft.exe is available for encryption features

### Docker Installation
1. Install Docker Desktop
2. Pull the image: `docker pull prosoft/easysave:3.0`
3. Run: `docker run -d -p 8080:8080 prosoft/easysave:3.0`
4. Access web interface at `http://localhost:8080`

## Getting Started

### Graphical Interface
Launch `output\EasySave.exe` to access:
- Backup job list
- Job management buttons
- Status information
- Parallel execution controls

### Command Line Interface
Use `output\EasySaveCLI.exe` with options:
- `EasySaveCLI.exe 1,2,3`: Execute specific jobs
- `EasySaveCLI.exe --all`: Execute all jobs
- `EasySaveCLI.exe --parallel 5`: Run with max 5 parallel jobs
- `EasySaveCLI.exe --priority high`: Run high priority jobs only
- `EasySaveCLI.exe --help`: Show available commands

## Creating a Backup Job

1. Click **New Job**
2. Enter job details:
   - Name (unique)
   - Source and destination folders
   - Backup type (Complete/Differential)
   - Priority level
   - Bandwidth limit
3. Click **Save**

## Executing Backup Jobs

### Single Job
1. Select a job
2. Click **Execute**

### Multiple Jobs
1. Select jobs using checkboxes
2. Click **Execute Selected**
3. Choose parallel or sequential execution

### Parallel Execution
1. Set maximum parallel jobs
2. Select multiple jobs
3. Click **Execute in Parallel**

## Job Control

Each job has individual controls:
- **Play**: Start job
- **Pause/Resume**: Pause or continue job
- **Stop**: Cancel job

A warning appears when closing with active jobs.

## Managing Backup Jobs

- **Edit**: Select job → Click Edit → Make changes → Save
- **Set Priority**: Select job → Set Priority → Choose level
- **Delete**: Select job → Delete → Confirm

## Settings

Access via the **Settings** button.

### Business Software Detection
Enter executable name to automatically pause backups when running

### File Encryption
Specify extensions to encrypt and CryptoSoft path

### Parallel Execution Settings
Configure maximum parallel jobs and priority options

### Priority File Extensions
Set extensions that receive processing priority

### Large File Threshold
Configure special handling for large files

### Log Format
Choose JSON or XML format

### Language
Select interface language (English/French)

## Monitoring Backups

Monitor in real-time:
- Overall and individual job progress
- Current file being processed
- Remaining files and size
- Elapsed time
- Encryption performance

## Log Files

1. **Daily Logs**: In `Logs/Json` or `Logs/Xml` folders
   - Format: `YYYY-MM-DD.json/xml`
   - Contains transfer and encryption details

2. **State File**: In execution folder as `state.json`
   - Real-time job information
   - Updated continuously

## Docker Container Usage

1. Access web interface at http://localhost:8080
2. Mount volumes: `docker run -d -p 8080:8080 -v /local/source:/backup/source -v /local/target:/backup/target prosoft/easysave:3.0`

## Troubleshooting

### Business Software Detection
- Verify executable name
- Try using full path
- Check process name for modern apps

### Encryption Issues
- Verify CryptoSoft.exe accessibility
- Check extension format (include leading dot)
- Ensure writable destination

### Parallel Execution Issues
- Check maximum parallel jobs setting
- Verify system resources
- Check for priority conflicts

### Job Control Issues
- Allow processing time
- Check for business software interference
- Verify job state
- Restart if unresponsive

### Docker Issues
- Verify Docker Desktop is running
- Check container logs
- Verify volume mounts and port forwarding