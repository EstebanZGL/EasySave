using System.Windows;
using System.Windows.Controls;
using EasySave.ViewModels;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private const double CompactWidthThreshold = 1080;
        private const double CompactHeightThreshold = 640;
        private const double UltraCompactWidthThreshold = 920;
        private const double UltraCompactHeightThreshold = 560;
        private const double MicroWidthThreshold = 700;
        private const double MicroHeightThreshold = 470;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            ApplyResponsiveLayout();
        }

        private void BackupJobsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                if (item is BackupJobViewModel vm)
                {
                    vm.IsSelected = false;
                }
            }

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
                LogoHost.Margin = new Thickness(0, 0, 12, 0);

                CreateJobButton.MinWidth = 230;
                CreateJobButton.Height = 46;
                CreatePlusText.FontSize = 19;
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
                LogoHost.Margin = new Thickness(0, 0, 18, 0);

                CreateJobButton.MinWidth = 290;
                CreateJobButton.Height = 56;
                CreatePlusText.FontSize = 24;
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
            double totalWidth = BackupJobsListView.ActualWidth - 24;
            if (totalWidth < 220)
            {
                totalWidth = 220;
            }

            double selectWidth = isMicroCompact ? 26 : 34;
            SelectColumn.Width = selectWidth;
            double dataWidth = totalWidth - selectWidth;
            if (dataWidth < 180)
            {
                dataWidth = 180;
            }

            if (isMicroCompact)
            {
                NameColumn.Width = dataWidth * 0.42;
                LastBackupColumn.Width = dataWidth * 0.58;
            }
            else if (isUltraCompact)
            {
                NameColumn.Width = dataWidth * 0.44;
                LastBackupColumn.Width = dataWidth * 0.56;
            }
            else
            {
                NameColumn.Width = dataWidth * 0.46;
                LastBackupColumn.Width = dataWidth * 0.54;
            }
        }
    }
}
