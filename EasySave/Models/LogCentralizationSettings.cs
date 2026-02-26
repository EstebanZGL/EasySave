using EasyLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    /// <summary>
    /// Settings for log centralization
    /// </summary>
    public class LogCentralizationSettings : INotifyPropertyChanged
    {
        private LogDestination _logDestination = LogDestination.Local;
        private string _serverUrl = "http://localhost:5000";
        private bool _isEnabled = false;
        public string SimulatedMachineName { get; set; } = "";
        public string SimulatedUserName { get; set; } = "";

        /// <summary>
        /// Gets or sets the log destination (Local, Remote, or Both)
        /// </summary>
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

        /// <summary>
        /// Gets or sets the URL of the log server
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether log centralization is enabled
        /// </summary>
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

        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}