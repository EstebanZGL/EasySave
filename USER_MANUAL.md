# User Manual - EasySave v2.0

## Introduction

EasySave v2.0 is a robust backup software developed by ProSoft, allowing you to create and execute an unlimited number of backup jobs with both a user-friendly graphical interface and a command-line interface. This version introduces file encryption capabilities and business software detection.

## Installation

1. Unzip the EasySave ZIP file into the folder of your choice
2. No additional installation is required
3. All executables (EasySave.exe, EasySaveCLI.exe, CryptoSoft.exe) are located in the `output` folder
4. Ensure that CryptoSoft.exe is available if you plan to use encryption features

## Getting Started

### Graphical Interface
Launch the application by double-clicking on `output\EasySave.exe`. The main window will display:
- A list of existing backup jobs
- Buttons for creating, executing, and managing backup jobs
- Status information for each job

### Command Line Interface
For automation or scripting, use `output\EasySaveCLI.exe` with the following options:
- `EasySaveCLI.exe 1,2,3`: Execute jobs 1, 2 and 3
- `EasySaveCLI.exe --all`: Execute all jobs
- `EasySaveCLI.exe --help`: Display available commands

## Creating a Backup Job

1. Click the **New Job** button in the main window
2. In the dialog that appears, enter:
   - **Name**: A unique name for your backup job
   - **Source Folder**: The folder to backup (click "Browse" to select)
   - **Destination Folder**: Where files will be saved (click "Browse" to select)
   - **Backup Type**: Choose between Complete or Differential
3. Click **Save** to create the job

## Executing Backup Jobs

### Single Job Execution

1. Select a job from the list
2. Click the **Execute** button

### Multiple Jobs Execution

1. Select multiple jobs using the checkboxes
2. Click the **Execute Selected** button

## Managing Backup Jobs

### Editing a Job

1. Select the job you want to modify
2. Click the **Edit** button
3. Make your changes in the dialog
4. Click **Save** to update the job

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

- Overall progress
- Current file being processed
- Remaining files and size
- Elapsed time

If a backup is paused due to business software detection, a notification will appear.

## Log Files

EasySave generates two types of log files:

1. **Daily Logs**: Located in the `Logs/Json` or `Logs/Xml` folder (depending on your settings)
   - Format: `YYYY-MM-DD.json` or `YYYY-MM-DD.xml`
   - Contains details of each file transfer including encryption time

2. **State File**: Located in the execution folder, named `state.json`
   - Contains real-time information about backup jobs
   - Updated continuously during backup operations

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