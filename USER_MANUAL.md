# User Manual - EasySave v1.0

## Introduction

EasySave is a robust backup software developed by ProSoft, allowing you to create and execute up to 5 different backup jobs. This manual will guide you through the main features of version 1.0.

## Installation

1. Unzip the EasySave ZIP file into the folder of your choice
2. No additional installation is required

## Getting Started

Launch the application by double-clicking on `EasySave.exe`. You will see the main menu:

```
===== EasySave v1.0 =====
1. Create a backup job
2. Execute a backup job
3. Display backup jobs
4. Change language (FR/EN)
0. Exit
Your choice: 
```

## Creating a Backup Job

1. Select option **1** from the main menu
2. Enter a unique name for your backup job
3. Specify the source folder to backup (full path)
4. Specify the destination folder (full path)
5. Choose the backup type:
   - **1** for a complete backup (full copy of files)
   - **2** for a differential backup (copies only files modified since the last complete backup)

## Executing a Backup Job

### Via the Menu

1. Select option **2** from the main menu
2. Enter the number of the job to execute (visible in the job list)
3. The backup progress will be displayed on screen

### Via Command Line

To execute specific jobs without going through the menu:

- For a single job: `EasySave.exe 1` (executes job #1)
- For a range of jobs: `EasySave.exe 1-3` (executes jobs #1, 2 and 3)
- For specific jobs: `EasySave.exe 1;3;5` (executes jobs #1, 3 and 5)

## Displaying Backup Jobs

1. Select option **3** from the main menu
2. The list of configured jobs will be displayed with their details:
   - Job number and name
   - Source folder
   - Target folder
   - Backup type (Complete/Differential)

## Changing Language

1. Select option **4** from the main menu
2. Choose the desired language:
   - **1** for French
   - **2** for English

## Backup Monitoring

EasySave automatically generates two types of files to track activity:

1. **Daily log files**: Located in the `logs` folder next to the executable, in `YYYY-MM-DD.json` format
2. **State file**: Located in the execution folder, named `state.json`


