using EasyLog;
using EasySave.ViewModels;
using System;
using System.Diagnostics;

namespace EasySave.Services
{
    /// <summary>
    /// Implementation of ILogIdentityProvider that uses SettingsViewModel values
    /// </summary>
    public class SettingsLogIdentityProvider : ILogIdentityProvider
    {
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// Constructor that takes a SettingsViewModel
        /// </summary>
        public SettingsLogIdentityProvider(SettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
            Debug.WriteLine($"SettingsLogIdentityProvider initialized with SettingsViewModel: {settingsViewModel != null}");
            if (settingsViewModel != null)
            {
                Debug.WriteLine($"SimulatedMachineName: '{settingsViewModel.LogCentralization?.SimulatedMachineName}'");
                Debug.WriteLine($"SimulatedUserName: '{settingsViewModel.LogCentralization?.SimulatedUserName}'");
            }
        }

        /// <summary>
        /// Gets the machine name from settings or falls back to system environment
        /// </summary>
        public string GetMachineName()
        {
            if (_settingsViewModel?.LogCentralization != null && 
                !string.IsNullOrWhiteSpace(_settingsViewModel.LogCentralization.SimulatedMachineName))
            {
                Debug.WriteLine($"Using simulated machine name: {_settingsViewModel.LogCentralization.SimulatedMachineName}");
                return _settingsViewModel.LogCentralization.SimulatedMachineName;
            }
            Debug.WriteLine($"Using system machine name: {Environment.MachineName}");
            return Environment.MachineName;
        }

        /// <summary>
        /// Gets the user name from settings or falls back to system environment
        /// </summary>
        public string GetUserName()
        {
            if (_settingsViewModel?.LogCentralization != null && 
                !string.IsNullOrWhiteSpace(_settingsViewModel.LogCentralization.SimulatedUserName))
            {
                Debug.WriteLine($"Using simulated user name: {_settingsViewModel.LogCentralization.SimulatedUserName}");
                return _settingsViewModel.LogCentralization.SimulatedUserName;
            }
            Debug.WriteLine($"Using system user name: {Environment.UserName}");
            return Environment.UserName;
        }
    }
}