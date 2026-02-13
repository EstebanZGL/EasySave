# Technical Documentation EasySave v2.0

## General Architecture

EasySave v2.0 is a backup application developed in C# (.NET 8.0) following the MVVM architecture pattern. The project is divided into several main components:

1. **EasySave**: Main WPF application with graphical user interface
2. **EasyLog**: Class library for log management
3. **CryptoSoft**: External encryption utility

## Project Structure

```
EasySave/
├── Models/
│   └── BackupJob.cs           # Data model for backup jobs
├── ViewModels/
│   ├── MainViewModel.cs       # Main view model for application logic
│   ├── SettingsViewModel.cs   # Settings management
│   └── RelayCommand.cs        # Command implementation
├── Views/
│   ├── MainWindow.xaml        # Main application window
│   ├── BackupJobDialog.xaml   # Dialog for creating/editing jobs
│   └── SettingsWindow.xaml    # Settings window
├── Services/
│   ├── BackupService.cs       # Backup execution
│   ├── BusinessSoftwareMonitor.cs # Business software detection
│   ├── CryptoService.cs       # File encryption
│   ├── StateManager.cs        # Real-time state management
│   └── TranslationService.cs  # Multilingual support
└── Converters/
    ├── BoolToColorConverter.cs # UI value converters
    └── NullToBoolConverter.cs  # UI value converters

EasyLog/
├── ILogger.cs                 # Base logger interface
├── IEncryptionLogger.cs       # Extended logger with encryption support
├── JsonLogger.cs              # JSON implementation
└── XmlLogger.cs               # XML implementation

CryptoSoft/
└── Program.cs                 # Encryption utility
```

## Main Components

### BackupJob Model

Represents a backup job with the following properties:
- **Name**: Job name (must be unique)
- **SourcePath**: Source path of files to backup
- **TargetPath**: Target path where to save files
- **Type**: Backup type (Complete or Differential)

The `Validate()` method ensures that:
- Name is not empty
- Source path exists
- Target path is specified

### MainViewModel

Central view model that manages the application's main functionality:

Key methods:
- `LoadBackupJobs()`: Loads saved backup jobs
- `ExecuteBackupJob(BackupJob job)`: Executes a specific backup job
- `ExecuteSelectedJobs()`: Executes all selected jobs
- `CreateBackupJob(BackupJob job)`: Creates a new backup job
- `DeleteBackupJob(BackupJob job)`: Deletes an existing job
- `OpenCreateBackupJobDialog()`: Opens the dialog for creating a new job
- `OpenEditBackupJobDialog(BackupJob job)`: Opens the dialog for editing a job
- `OpenSettings()`: Opens the settings window

### SettingsViewModel

Manages application settings and preferences:

Key properties:
- `BusinessSoftwareName`: Name of the business software to detect
- `EncryptionExtensions`: List of file extensions to encrypt
- `CryptoSoftPath`: Path to the CryptoSoft executable
- `LogFormat`: Current log format (JSON/XML)

Key methods:
- `IsBusinessSoftwareRunning()`: Checks if the specified business software is running
- `ShouldEncryptFile(string filePath)`: Determines if a file should be encrypted
- `LoadSettings()`: Loads settings from settings.json
- `SaveSettings()`: Saves settings to settings.json

### BackupService

Service responsible for executing backup operations:

Key methods:
- `ExecuteBackupJobAsync(BackupJob job)`: Executes a backup job asynchronously
- `PauseBackupJob()`: Pauses the current backup job
- `ResumeBackupJob()`: Resumes a paused backup job
- `StopBackupJob()`: Stops the current backup job

### BusinessSoftwareMonitor

Monitors the execution of business software and controls backup operations accordingly:

Key methods:
- `StartMonitoring()`: Begins monitoring for business software
- `StopMonitoring()`: Stops monitoring
- `CheckBusinessSoftwareStatus()`: Checks if business software is running and takes appropriate action

### CryptoService

Manages file encryption using the external CryptoSoft utility:

Key methods:
- `ShouldEncrypt(string filePath)`: Determines if a file should be encrypted
- `EncryptFileAsync(string sourceFile, string targetFile)`: Encrypts a file

### StateManager

Manages the real-time state of backup jobs, stored in `state.json`:

Key methods:
- `UpdateStateAsync(...)`: Updates the state of a specific job
- `SaveStateAsync()`: Saves the current state to the state file
- `LoadState()`: Loads the state from the state file

### TranslationService

Manages multilingual support (FR/EN) for the application:

Key methods:
- `GetTranslation(string key)`: Gets the translation for a specific key
- `SetLanguage(string language)`: Changes the current language

## Key Features in v2.0

### 1. Graphical User Interface

EasySave v2.0 introduces a complete graphical interface using WPF and the MVVM pattern, allowing for:
- Visual management of backup jobs
- Real-time progress monitoring
- Settings configuration through a dedicated interface
- Improved user experience with visual feedback

### 2. Business Software Detection

The application can detect if specific business software is running and manage backups accordingly:

- **Multi-method detection**: Uses several techniques to reliably detect running software
  - Process name matching
  - Window title detection
  - Special handling for modern applications (UWP)
  - PowerShell-based detection as fallback

- **Automatic backup management**:
  - Prevents backup start if business software is running
  - Automatically pauses backup when business software starts
  - Automatically resumes backup when business software stops

### 3. File Encryption

Integration with CryptoSoft for selective file encryption:

- **Selective encryption**: Only encrypts files with specified extensions
- **Performance tracking**: Measures and logs encryption time
- **Fallback mechanism**: If encryption fails, files are still backed up (unencrypted)

### 4. Enhanced Logging

Improved logging system with:

- **Format selection**: Choose between JSON and XML formats
- **Encryption metrics**: Logs include encryption time for encrypted files
- **Structured storage**: Logs are organized in dedicated folders by format

## Data Files

### settings.json

Stores application settings:

```json
{
  "BusinessSoftwareName": "calc.exe",
  "EncryptionExtensions": [".txt", ".doc", ".pdf"],
  "CryptoSoftPath": "C:\\Path\\To\\CryptoSoft.exe",
  "MaxParallelJobs": 5
}
```

### state.json

Stores the real-time state of backup jobs:

```json
[
  {
    "Name": "Documents",
    "State": "Active",
    "TotalFiles": 100,
    "TotalFilesRemaining": 75,
    "TotalSize": 1048576,
    "TotalSizeRemaining": 786432,
    "Progress": 25,
    "CurrentFile": "C:\\Users\\Username\\Documents\\file.txt",
    "CurrentTargetFile": "D:\\Backup\\Documents\\file.txt"
  },
  ...
]
```

### Log Files (YYYY-MM-DD.json/xml)

Daily log files stored in the `Logs/Json` or `Logs/Xml` directories:

JSON format example:
```json
[
  {
    "Timestamp": "2026-02-02T12:34:56.789Z",
    "BackupName": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents\\file.txt",
    "TargetPath": "D:\\Backup\\Documents\\file.txt",
    "FileSize": 1024,
    "TransferTime": 15,
    "EncryptionTime": 5
  },
  ...
]
```

XML format example:
```xml
<Logs>
  <Log>
    <Timestamp>2026-02-02T12:34:56.789Z</Timestamp>
    <BackupName>Documents</BackupName>
    <SourcePath>C:\Users\Username\Documents\file.txt</SourcePath>
    <TargetPath>D:\Backup\Documents\file.txt</TargetPath>
    <FileSize>1024</FileSize>
    <TransferTime>15</TransferTime>
    <EncryptionTime>5</EncryptionTime>
  </Log>
</Logs>
```

### Preference Files

- **language.txt**: Stores the language preference (en/fr)
- **logformat.txt**: Stores the log format preference (JSON/XML)

## Error Handling

### File Operations

- Source file access errors are caught and logged
- Target directory creation failures are reported to the user
- File copy exceptions are logged with a negative transfer time
- Encryption failures are logged with a negative encryption time

### Business Software Detection

- Multiple detection methods ensure reliable operation
- Access denied errors are handled gracefully
- Detection is performed periodically to catch software that starts during backup

### Settings Management

- Default settings are used if settings file is missing or corrupted
- Settings changes are saved immediately
- Input validation prevents invalid settings

## Design Patterns

### MVVM Pattern

- **Models**: Data structures like BackupJob
- **Views**: XAML-based UI components
- **ViewModels**: Classes that handle UI logic and data binding

### Command Pattern

- `RelayCommand` class implements `ICommand` interface
- Commands encapsulate user actions (Execute, Delete, Pause, etc.)
- Enables clean separation between UI and logic

### Observer Pattern

- `INotifyPropertyChanged` implementation for data binding
- `BusinessSoftwareMonitor` uses events to notify about software status changes
- State changes propagate through the application

### Strategy Pattern

- Different logging strategies (JSON/XML) through the `ILogger` interface
- Encryption strategy in `CryptoService`

### Factory Pattern

- `LoggerFactory` creates appropriate logger instances based on configuration

## Integration with CryptoSoft

CryptoSoft is an external encryption utility that:

1. Takes source and target file paths as command-line arguments
2. Encrypts the source file and saves it to the target path
3. Returns the encryption time in milliseconds

Integration flow:
1. `CryptoService.ShouldEncrypt()` determines if a file needs encryption
2. If yes, `EncryptFileAsync()` launches CryptoSoft as a separate process
3. Encryption time is captured and logged
4. If CryptoSoft fails, a standard copy is performed as fallback

## Troubleshooting Guide

### Common Errors

1. **Business Software Detection Issues**
   - Ensure the correct process name is specified in settings
   - For modern Windows apps, try using the executable name from Task Manager
   - Run EasySave with administrator privileges for better process detection

2. **Encryption Failures**
   - Verify CryptoSoft path in settings
   - Check if target directories are writable
   - Ensure file extensions are correctly specified (with leading dot)

3. **UI Responsiveness**
   - Large backup operations run on background threads
   - Progress updates may be delayed for very fast operations

### Debugging Techniques

- Enable Debug output to see detailed operation logs
- Check log files for error indicators (negative transfer/encryption times)
- Use the settings dialog to verify configuration values

## Known Limitations

1. No file compression
2. No network path validation
3. No priority system for backup jobs
4. Business software detection may require administrator privileges
5. No backup resumption after application restart

## Planned Evolutions

1. **Version 3.0**: 
   - Parallel backups
   - Priority management for file types
   - Bandwidth limitation
   - Docker containerization
   - CryptoSoft mono-instance with queue management