using System.Windows;
using System.Threading.Tasks;
using EasySave.ViewModels;
using Microsoft.Win32;

namespace EasySave.Views
{
     
    /// Interaction logic for SettingsWindow.xaml
     
    public partial class SettingsWindow : Window
    {
        private const string CopyGlyph = "\uE8C8";
        private const string CheckGlyph = "\uE73E";
        private readonly SettingsViewModel _viewModel;
        private bool _isEncryptionKeyVisible;
        private bool _isSyncingEncryptionKeyControls;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string currentKey = _viewModel.CryptoPassword ?? string.Empty;
            EncryptionKeyPasswordBox.Password = currentKey;
            EncryptionKeyTextBox.Text = currentKey;
            UpdateEncryptionKeyVisibility();
        }

        private void EncryptionKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingEncryptionKeyControls)
            {
                return;
            }

            if (sender is System.Windows.Controls.PasswordBox passwordBox)
            {
                string newValue = passwordBox.Password ?? string.Empty;
                _isSyncingEncryptionKeyControls = true;
                EncryptionKeyTextBox.Text = newValue;
                _isSyncingEncryptionKeyControls = false;
                _viewModel.CryptoPassword = newValue;
            }
        }

        private void EncryptionKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isSyncingEncryptionKeyControls)
            {
                return;
            }

            if (sender is System.Windows.Controls.TextBox textBox)
            {
                string newValue = textBox.Text ?? string.Empty;
                _isSyncingEncryptionKeyControls = true;
                EncryptionKeyPasswordBox.Password = newValue;
                _isSyncingEncryptionKeyControls = false;
                _viewModel.CryptoPassword = newValue;
            }
        }

        private void ToggleEncryptionKeyVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            _isEncryptionKeyVisible = !_isEncryptionKeyVisible;
            UpdateEncryptionKeyVisibility();
        }

        private void UpdateEncryptionKeyVisibility()
        {
            EncryptionKeyPasswordBox.Visibility = _isEncryptionKeyVisible ? Visibility.Collapsed : Visibility.Visible;
            EncryptionKeyTextBox.Visibility = _isEncryptionKeyVisible ? Visibility.Visible : Visibility.Collapsed;
            ToggleEncryptionKeyVisibilityButton.ToolTip = _isEncryptionKeyVisible
                ? "Hide key"
                : "Show key";
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

        private async void CopyBusinessSoftwarePathButton_Click(object sender, RoutedEventArgs e)
        {
            if (CopyToClipboard(_viewModel.BusinessSoftwareName))
            {
                await ShowCopyFeedbackAsync(sender as System.Windows.Controls.Button);
            }
        }

        private async void CopyCryptoSoftPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (CopyToClipboard(_viewModel.CryptoSoftPath))
            {
                await ShowCopyFeedbackAsync(sender as System.Windows.Controls.Button);
            }
        }

        private static bool CopyToClipboard(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                System.Windows.Clipboard.SetText(value);
                return true;
            }

            return false;
        }

        private static async Task ShowCopyFeedbackAsync(System.Windows.Controls.Button? button)
        {
            if (button == null)
            {
                return;
            }

            button.Content = CheckGlyph;
            await Task.Delay(2000);
            button.Content = CopyGlyph;
        }

        private void ChangeCryptoPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Get password from TextBox
            string newPassword = _viewModel.CryptoPassword;

            // Check that password is not empty
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageDialog("Password cannot be empty.", "Error");
                return;
            }

            // Change password
            if (_viewModel.ChangeCryptoPassword(newPassword))
            {
                MessageDialog("Encryption password has been successfully modified.", "Success");
            }
            else
            {
                MessageDialog("An error occurred while modifying the password.", "Error");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        // Simplified method to display a message
        private void MessageDialog(string message, string title)
        {
            // Create a new window for the message
            Window dialogWindow = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            
            // Create a container for elements
            System.Windows.Controls.StackPanel panel = new System.Windows.Controls.StackPanel();
            panel.Margin = new Thickness(10);
            
            // Add the message
            System.Windows.Controls.TextBlock messageText = new System.Windows.Controls.TextBlock();
            messageText.Text = message;
            messageText.TextWrapping = TextWrapping.Wrap;
            messageText.Margin = new Thickness(10);
            panel.Children.Add(messageText);
            
            // Add an OK button
            System.Windows.Controls.Button okButton = new System.Windows.Controls.Button();
            okButton.Content = "OK";
            okButton.Width = 80;
            okButton.Margin = new Thickness(10);
            okButton.Click += (s, e) => dialogWindow.Close();
            panel.Children.Add(okButton);
            
            // Set window content
            dialogWindow.Content = panel;
            
            // Show dialog window
            dialogWindow.ShowDialog();
        }
    }
}
