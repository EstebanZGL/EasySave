using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using EasySave.Services;
using EasySave.ViewModels;

namespace EasySave.Views
{
     
    /// Interaction logic for MainWindow.xaml
     
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly TranslationService _translationService;
        private readonly ParallelBackupService _backupService;
        private const string CopyGlyph = "\uE8C8";
        private const string CheckGlyph = "\uE73E";
        private const double CompactWidthThreshold = 1080;
        private const double CompactHeightThreshold = 640;
        private const double UltraCompactWidthThreshold = 920;
        private const double UltraCompactHeightThreshold = 560;
        private const double MicroWidthThreshold = 700;
        private const double MicroHeightThreshold = 470;

        public MainWindow(MainViewModel viewModel, TranslationService translationService, ParallelBackupService backupService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _translationService = translationService;
            _backupService = backupService;
            DataContext = _viewModel;
            ApplyResponsiveLayout();
            Closing += MainWindow_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RecalculateBackupJobsColumnsDeferred();
        }

        private void BackupJobsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is BackupJobViewModel vm)
                {
                    vm.IsSelected = true;
                }
            }

            if (BackupJobsListView.SelectedItem is BackupJobViewModel selectedJob)
            {
                _viewModel.SelectedBackupJob = selectedJob;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyResponsiveLayout();
            RecalculateBackupJobsColumnsDeferred();
        }

        private void BackupJobsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalculateBackupJobsColumnsDeferred();
        }

        private async void CopySourcePathButton_Click(object sender, RoutedEventArgs e)
        {
            if (CopyToClipboard(_viewModel.SelectedBackupJob?.SourcePath))
            {
                await ShowCopyFeedbackAsync(sender as System.Windows.Controls.Button);
            }
        }

        private async void CopyTargetPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (CopyToClipboard(_viewModel.SelectedBackupJob?.TargetPath))
            {
                await ShowCopyFeedbackAsync(sender as System.Windows.Controls.Button);
            }
        }

        private void ApplyResponsiveLayout()
        {
            bool isCompact = ActualWidth < CompactWidthThreshold || ActualHeight < CompactHeightThreshold;
            bool isUltraCompact = ActualWidth < UltraCompactWidthThreshold || ActualHeight < UltraCompactHeightThreshold;
            bool isMicroCompact = ActualWidth < MicroWidthThreshold || ActualHeight < MicroHeightThreshold;

            if (isCompact)
            {
                DetailsPanel.Visibility = Visibility.Collapsed;
                JobStatusPanel.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(BackupJobsPanel, 2);
                BackupJobsPanel.Margin = new Thickness(0);
            }
            else
            {
                DetailsPanel.Visibility = Visibility.Visible;
                JobStatusPanel.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(BackupJobsPanel, 1);
                BackupJobsPanel.Margin = new Thickness(0, 0, 10, 0);
            }

            if (isMicroCompact)
            {
                HeaderPanel.Visibility = Visibility.Collapsed;
                RootGrid.Margin = new Thickness(8);
                BackupJobsTitle.FontSize = 16;
                SelectAllCheckBox.FontSize = 12;
                ExecuteSelectedButton.Height = 34;
                ExecuteSelectedButton.FontSize = 12;
                ExecuteSelectedButton.Margin = new Thickness(8, 6, 8, 2);
            }
            else if (isUltraCompact)
            {
                HeaderPanel.Visibility = Visibility.Visible;
                RootGrid.Margin = new Thickness(10);
                HeaderPanel.Padding = new Thickness(10, 8, 10, 8);
                HeaderPanel.Margin = new Thickness(0, 0, 0, 8);

                LogoHost.Width = 112;
                LogoHost.Height = 56;
                LogoHost.Margin = new Thickness(0, 0, 6, 0);

                CreateJobButton.MinWidth = 230;
                CreateJobButton.Height = 46;
                CreatePlusText.FontSize = 18;
                CreateLabelText.FontSize = 14;

                SettingsButton.Width = 44;
                SettingsButton.Height = 44;
                SettingsButton.FontSize = 21;

                BackupJobsTitle.FontSize = 17;
                SelectAllCheckBox.FontSize = 13;
                ExecuteSelectedButton.Height = 36;
                ExecuteSelectedButton.FontSize = 13;
                ExecuteSelectedButton.Margin = new Thickness(8, 6, 8, 2);
            }
            else
            {
                HeaderPanel.Visibility = Visibility.Visible;
                RootGrid.Margin = new Thickness(16);
                HeaderPanel.Padding = new Thickness(14, 10, 14, 10);
                HeaderPanel.Margin = new Thickness(0, 0, 0, 10);

                LogoHost.Width = 140;
                LogoHost.Height = 70;
                LogoHost.Margin = new Thickness(0, 0, 8, 0);

                CreateJobButton.MinWidth = 290;
                CreateJobButton.Height = 56;
                CreatePlusText.FontSize = 20;
                CreateLabelText.FontSize = 18;

                SettingsButton.Width = 50;
                SettingsButton.Height = 50;
                SettingsButton.FontSize = 24;

                BackupJobsTitle.FontSize = 20;
                SelectAllCheckBox.FontSize = 14;
                ExecuteSelectedButton.Height = 40;
                ExecuteSelectedButton.FontSize = 14;
                ExecuteSelectedButton.Margin = new Thickness(8, 8, 8, 4);
            }

            ApplyBackupJobsColumnsSize(isMicroCompact, isUltraCompact);
        }

        private void ApplyBackupJobsColumnsSize(bool isMicroCompact, bool isUltraCompact)
        {
            if (BackupJobsListView.ActualWidth <= 0)
            {
                return;
            }

            double selectWidth = isMicroCompact ? 26 : 34;
            SelectColumn.Width = selectWidth;

            // Reserve space for list chrome so columns always fit in viewport.
            double viewportWidth = BackupJobsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 18;
            if (viewportWidth < 220)
            {
                viewportWidth = 220;
            }

            double dataWidth = viewportWidth - selectWidth;
            if (dataWidth < 170)
            {
                dataWidth = 170;
            }

            double minNameWidth = isMicroCompact ? 72 : isUltraCompact ? 96 : 120;
            double minLastBackupWidth = isMicroCompact ? 92 : isUltraCompact ? 118 : 150;
            double lastBackupRatio = isMicroCompact ? 0.58 : isUltraCompact ? 0.56 : 0.54;
            double preferredLastBackupWidth = dataWidth * lastBackupRatio;
            double lastBackupWidth = Math.Max(minLastBackupWidth, preferredLastBackupWidth);
            lastBackupWidth = Math.Min(lastBackupWidth, dataWidth - minNameWidth);

            if (lastBackupWidth < minLastBackupWidth)
            {
                lastBackupWidth = minLastBackupWidth;
            }

            double nameWidth = dataWidth - lastBackupWidth;
            if (nameWidth < minNameWidth)
            {
                nameWidth = minNameWidth;
                lastBackupWidth = Math.Max(minLastBackupWidth, dataWidth - nameWidth);
            }

            if (isMicroCompact)
            {
                NameColumn.Width = Math.Max(minNameWidth, Math.Floor(nameWidth));
                LastBackupColumn.Width = Math.Max(minLastBackupWidth, Math.Floor(lastBackupWidth));
            }
            else if (isUltraCompact)
            {
                NameColumn.Width = Math.Max(minNameWidth, Math.Floor(nameWidth));
                LastBackupColumn.Width = Math.Max(minLastBackupWidth, Math.Floor(lastBackupWidth));
            }
            else
            {
                NameColumn.Width = Math.Max(minNameWidth, Math.Floor(nameWidth));
                LastBackupColumn.Width = Math.Max(minLastBackupWidth, Math.Floor(lastBackupWidth));
            }
        }

        private void RecalculateBackupJobsColumnsDeferred()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() =>
                {
                    bool isUltraCompact = ActualWidth < UltraCompactWidthThreshold || ActualHeight < UltraCompactHeightThreshold;
                    bool isMicroCompact = ActualWidth < MicroWidthThreshold || ActualHeight < MicroHeightThreshold;
                    ApplyBackupJobsColumnsSize(isMicroCompact, isUltraCompact);
                }));
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

         
        /// Event handler for window closing
         
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check if backups are currently running
            var activeJobs = _backupService.GetActiveJobs();
            
            if (activeJobs.Count > 0)
            {
                // Get the confirmation message based on the current language
                string message = _translationService.CurrentLanguage == "fr"
                    ? "Des sauvegardes sont en cours d'exécution. Si vous fermez l'application maintenant, ces sauvegardes seront perdues.\n\nVoulez-vous vraiment quitter ?"
                    : "Backup jobs are currently running. If you close the application now, these backups will be lost.\n\nDo you really want to quit?";
                
                string title = _translationService.CurrentLanguage == "fr"
                    ? "Confirmation de fermeture"
                    : "Confirm Close";
                
                // Use System.Windows.MessageBox explicitly to avoid ambiguity
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );
                
                // Cancel closing if the user clicks "No"
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
