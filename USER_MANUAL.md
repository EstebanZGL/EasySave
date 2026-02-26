# User Manual - EasySave v3.0

## Introduction

EasySave v3.0 is a robust backup software developed by ProSoft, allowing you to create and execute an unlimited number of backup jobs with both a user-friendly graphical interface and a command-line interface. This version introduces parallel backups, priority management, bandwidth limitation, and remote monitoring capabilities.

## Installation

### Standard Installation
1. Unzip the EasySave ZIP file into the folder of your choice
2. No additional installation is required
3. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe, EasySaveRemote.exe) are located in the `output` folder
4. Ensure that CryptoSoft.exe is available if you plan to use encryption features

### Docker Installation
1. Make sure Docker Desktop is installed on your system
2. Pull the Docker image: `docker pull prosoft/easysave:3.0`
3. Run the container: `docker run -d -p 8080:8080 prosoft/easysave:3.0`
4. Access the web interface at `http://localhost:8080`

## Getting Started

### Graphical Interface
Launch the application by double-clicking on `output\EasySave.exe`. The main window will display:
- A list of existing backup jobs
- Buttons for creating, executing, and managing backup jobs
- Status information for each job
- Parallel execution controls
- Priority management options

### Command Line Interface
For automation or scripting, use `output\EasySaveCLI.exe` with the following options:
- `EasySaveCLI.exe 1,2,3`: Execute jobs 1, 2 and 3
- `EasySaveCLI.exe --all`: Execute all jobs
- `EasySaveCLI.exe --parallel 5`: Execute all jobs with max 5 parallel jobs
- `EasySaveCLI.exe --priority high`: Execute only high priority jobs
- `EasySaveCLI.exe --bandwidth 10`: Limit bandwidth to 10 MB/s
- `EasySaveCLI.exe --help`: Display available commands

### Remote Monitoring Interface
For remote monitoring and control:
1. Launch `output\EasySaveRemote.exe`
2. Enter the server IP address and port (default: 127.0.0.1:8080)
3. Click "Connect" to establish a connection
4. View and control backup jobs from the remote interface

## Creating a Backup Job

1. Click the **New Job** button in the main window
2. In the dialog that appears, enter:
   - **Name**: A unique name for your backup job
   - **Source Folder**: The folder to backup (click "Browse" to select)
   - **Destination Folder**: Where files will be saved (click "Browse" to select)
   - **Backup Type**: Choose between Complete or Differential
   - **Priority**: Set job priority (High, Medium, Low)
   - **Max Bandwidth**: Set bandwidth limit for this job (MB/s)
3. Click **Save** to create the job

## Executing Backup Jobs

### Single Job Execution

1. Select a job from the list
2. Click the **Execute** button

### Multiple Jobs Execution

1. Select multiple jobs using the checkboxes
2. Click the **Execute Selected** button
3. Choose whether to run jobs in parallel or sequentially

### Parallel Execution

1. In the main window, set the maximum number of parallel jobs
2. Select multiple jobs
3. Click **Execute in Parallel**
4. Jobs will run simultaneously up to the defined limit

## Managing Backup Jobs

### Editing a Job

1. Select the job you want to modify
2. Click the **Edit** button
3. Make your changes in the dialog
4. Click **Save** to update the job

### Setting Job Priority

1. Select a job from the list
2. Click the **Set Priority** button
3. Choose High, Medium, or Low priority
4. Higher priority jobs will execute before lower priority jobs

### Deleting a Job

1. Select the job you want to remove
2. Click the **Delete** button
3. Confirm the deletion when prompted

## Settings

Access the settings by clicking the **Settings** button in the main window.

### Business Software Detection

EasySave can detect when specific business software is running and pause backups accordingly:

1. In the Settings window, enter the name of the business software executable (e.g., "calc.exe" for Calculator)
2. When this software is running:
   - New backups will not start
   - Running backups will automatically pause
   - Backups will automatically resume when the software closes

### File Encryption

You can specify which file types should be encrypted during backup:

1. In the Settings window, enter file extensions in the "Extensions to Encrypt" field (e.g., ".txt;.doc;.pdf")
2. Specify the path to CryptoSoft.exe if it's not in the default location
3. Files with matching extensions will be automatically encrypted during backup

### Parallel Execution Settings

Configure how parallel backups are handled:

1. In the Settings window, set the "Maximum Parallel Jobs" value
2. Enable or disable "Respect Job Priority" option
3. Set "Default Job Priority" for new backup jobs

### Bandwidth Management

Control network and disk usage:

1. In the Settings window, set the "Global Bandwidth Limit" (MB/s)
2. Enable or disable "Individual Job Limits"
3. Set "Default Job Bandwidth Limit" for new backup jobs

### Remote Access Settings

Configure remote monitoring and control:

1. In the Settings window, navigate to the "Remote Access" tab
2. Enable or disable remote access
3. Set the listening port (default: 8080)
4. Configure access credentials if desired

### Log Format

Choose your preferred log format:

1. In the Settings window, select either JSON or XML format
2. Log files will be stored in the corresponding format in the Logs folder
3. Note: Changing the log format requires restarting the application

### Language

Switch between available languages:

1. In the Settings window, select your preferred language
2. The interface will update immediately

## Monitoring Backups

During backup execution, you can monitor:

- Overall progress for all jobs
- Individual job progress
- Current file being processed
- Remaining files and size
- Elapsed time
- Network bandwidth usage
- Encryption performance

If a backup is paused due to business software detection, a notification will appear.

## Remote Monitoring and Control

The EasySaveRemote application allows you to:

1. View the status of all backup jobs
2. Start, pause, resume, or stop backup jobs
3. Create new backup jobs
4. Edit existing backup jobs
5. View real-time progress and statistics
6. Receive notifications about job completion or errors

To connect to a remote EasySave instance:
1. Launch EasySaveRemote.exe
2. Enter the server IP address and port
3. Click "Connect"

## Docker Container Usage

When running EasySave in Docker:

1. Access the web interface at http://localhost:8080
2. Use volume mounts to access local files:
   ```
   docker run -d -p 8080:8080 -v /local/source:/backup/source -v /local/target:/backup/target prosoft/easysave:3.0
   ```
3. Configure settings through the web interface
4. Backup jobs will run within the container

## Log Files

EasySave generates two types of log files:

1. **Daily Logs**: Located in the `Logs/Json` or `Logs/Xml` folder (depending on your settings)
   - Format: `YYYY-MM-DD.json` or `YYYY-MM-DD.xml`
   - Contains details of each file transfer including encryption time, bandwidth usage, and priority

2. **State File**: Located in the execution folder, named `state.json`
   - Contains real-time information about backup jobs
   - Updated continuously during backup operations
   - Includes parallel execution status and resource usage

## Troubleshooting

### Business Software Detection Issues

If business software detection is not working correctly:

1. Make sure you've entered the correct executable name in Settings
2. Try using the full path to the executable
3. For modern Windows apps, you may need to use the actual process name

### Encryption Issues

If files are not being encrypted:

1. Verify that CryptoSoft.exe is accessible in the output folder
2. Check that file extensions are correctly specified (with leading dot)
3. Ensure the destination folder is writable

### Parallel Execution Issues

If parallel jobs are not working as expected:

1. Check the "Maximum Parallel Jobs" setting
2. Verify that your system has sufficient resources
3. Check for any priority conflicts
4. Ensure bandwidth limits are not too restrictive

### Remote Access Issues

If remote monitoring is not working:

1. Verify that remote access is enabled in settings
2. Check that the port is not blocked by a firewall
3. Ensure the correct IP address and port are used in the remote application
4. Verify network connectivity between the machines

### Docker Container Issues

If the Docker container is not working:

1. Verify Docker Desktop is running
2. Check container logs: `docker logs [container-id]`
3. Ensure volume mounts are correctly configured
4. Verify port forwarding is working correctly