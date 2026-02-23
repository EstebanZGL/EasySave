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

        private void ChangeCryptoPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer le mot de passe depuis la TextBox
            string newPassword = _viewModel.CryptoPassword;

            // Vérifier que le mot de passe n'est pas vide
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageDialog("Le mot de passe ne peut pas être vide.", "Erreur");
                return;
            }

            // Changer le mot de passe
            if (_viewModel.ChangeCryptoPassword(newPassword))
            {
                MessageDialog("Le mot de passe de cryptage a été modifié avec succès.", "Succès");
            }
            else
            {
                MessageDialog("Une erreur est survenue lors de la modification du mot de passe.", "Erreur");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        // Méthode simplifiée pour afficher un message
        private void MessageDialog(string message, string title)
        {
            // Créer une nouvelle fenêtre pour le message
            Window dialogWindow = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            
            // Créer un conteneur pour les éléments
            System.Windows.Controls.StackPanel panel = new System.Windows.Controls.StackPanel();
            panel.Margin = new Thickness(10);
            
            // Ajouter le message
            System.Windows.Controls.TextBlock messageText = new System.Windows.Controls.TextBlock();
            messageText.Text = message;
            messageText.TextWrapping = TextWrapping.Wrap;
            messageText.Margin = new Thickness(10);
            panel.Children.Add(messageText);
            
            // Ajouter un bouton OK
            System.Windows.Controls.Button okButton = new System.Windows.Controls.Button();
            okButton.Content = "OK";
            okButton.Width = 80;
            okButton.Margin = new Thickness(10);
            okButton.Click += (s, e) => dialogWindow.Close();
            panel.Children.Add(okButton);
            
            // Définir le contenu de la fenêtre
            dialogWindow.Content = panel;
            
            // Afficher la fenêtre de dialogue
            dialogWindow.ShowDialog();
        }
    }
}