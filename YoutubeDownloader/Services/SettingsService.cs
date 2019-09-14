using System.ComponentModel;
using Tyrrrz.Settings;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public int MaxConcurrentDownloadCount { get; set; } = 2;

        public bool ShouldInjectTags { get; set; } = true;

        public string LastFormat { get; set; } = "mp3";

        public string DefaultDownloadDirectory { get; set; }

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}