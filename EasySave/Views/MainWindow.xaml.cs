using System.Linq;
using System.Windows;
using EasySave.Services;
using EasySave.ViewModels;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly TranslationService _translationService;
        private readonly ParallelBackupService _backupService;

        public MainWindow(MainViewModel viewModel, TranslationService translationService, ParallelBackupService backupService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _translationService = translationService;
            _backupService = backupService;
            DataContext = viewModel;
            
            // Ajouter un gestionnaire d'événement pour la fermeture de la fenêtre
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Gestionnaire d'événement pour la fermeture de la fenêtre
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Vérifier si des sauvegardes sont en cours
            var activeJobs = _backupService.GetActiveJobs();
            
            if (activeJobs.Count > 0)
            {
                // Obtenir le message de confirmation en fonction de la langue actuelle
                string message = _translationService.CurrentLanguage == "fr"
                    ? "Des sauvegardes sont en cours d'exécution. Si vous fermez l'application maintenant, ces sauvegardes seront perdues.\n\nVoulez-vous vraiment quitter ?"
                    : "Backup jobs are currently running. If you close the application now, these backups will be lost.\n\nDo you really want to quit?";
                
                string title = _translationService.CurrentLanguage == "fr"
                    ? "Confirmation de fermeture"
                    : "Confirm Close";
                
                // Utiliser System.Windows.MessageBox explicitement pour éviter l'ambiguïté
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );
                
                // Annuler la fermeture si l'utilisateur clique sur "Non"
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}