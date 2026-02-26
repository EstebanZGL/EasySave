using EasyLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
     
    /// Settings for log centralization
     
    public class LogCentralizationSettings : INotifyPropertyChanged
    {
        private LogDestination _logDestination = LogDestination.Local;
        private string _serverUrl = "http://localhost:5000";
        private bool _isEnabled = false;
        private string _simulatedMachineName = "";
        private string _simulatedUserName = "";

         
        /// Gets or sets the log destination (Local, Remote, or Both)
         
        public LogDestination LogDestination
        {
            get => _logDestination;
            set
            {
                if (_logDestination != value)
                {
                    _logDestination = value;
                    OnPropertyChanged();
                }
            }
        }

         
        /// Gets or sets the URL of the log server
         
        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl != value)
                {
                    _serverUrl = value;
                    OnPropertyChanged();
                }
            }
        }

         
        /// Gets or sets whether log centralization is enabled
         
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SimulatedMachineName
        {
            get => _simulatedMachineName;
            set
            {
                if (_simulatedMachineName != value)
                {
                    _simulatedMachineName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SimulatedUserName
        {
            get => _simulatedUserName;
            set
            {
                if (_simulatedUserName != value)
                {
                    _simulatedUserName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// Event raised when a property changes
         
        public event PropertyChangedEventHandler PropertyChanged;

         
        /// Raises the PropertyChanged event
         
         protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
