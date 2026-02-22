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

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void BackupJobsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackupJobsListView.SelectedItem is BackupJobViewModel selectedJob)
            {
                _viewModel.SelectedBackupJob = selectedJob;
            }
        }
    }
}
