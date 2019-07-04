using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.Views.Dialogs
{
    public partial class SettingsView
    {
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private string DefaultDownloadDirectory;

        public SettingsView(SettingsService settingsService, DialogManager dialogManager)
        {
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            InitializeComponent();
        }

        private void OnClickBrowse(object sender, System.Windows.RoutedEventArgs e)
        {
            DefaultDownloadDirectory = _dialogManager.PromptDirectoryPath();
            if (DefaultDownloadDirectory != null)
            {
                _settingsService.DefaultDownloadDirectory = DefaultDownloadDirectory;
            }
            else return;
        }
    }
}