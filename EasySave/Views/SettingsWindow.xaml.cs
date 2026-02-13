using System.Windows;
using EasySave.ViewModels;
using Microsoft.Win32;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void BrowseBusinessSoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Business Software"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.BusinessSoftwareName = dialog.FileName;
            }
        }

        private void BrowseCryptoSoftButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select CryptoSoft Application"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.CryptoSoftPath = dialog.FileName;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}