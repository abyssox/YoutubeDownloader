﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public IReadOnlyList<Video> AvailableVideos { get; set; }

        public IReadOnlyList<Video> SelectedVideos { get; set; }

        public IReadOnlyList<string> AvailableFormats { get; } = new[] {"mp4", "mp3", "ogg"};

        public string SelectedFormat { get; set; }

        public DownloadMultipleSetupViewModel(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Select last used format
            SelectedFormat = AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        public bool CanConfirm => !SelectedVideos.IsNullOrEmpty();

        public void Confirm()
        {
            var dirPath = "";

            // Check if Default Download Directory is set in settings, otherwise prompt user for output directory path
            if (_settingsService.DefaultDownloadDirectory.IsNullOrEmpty())
            { 
                dirPath = _dialogManager.PromptDirectoryPath();

                // If canceled - return
                if (dirPath.IsNullOrWhiteSpace())
                    return;
            } else
            {
                dirPath = _settingsService.DefaultDownloadDirectory;
            }

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Make sure selected videos are ordered in the same way as available videos
            var orderedSelectedVideos = AvailableVideos.Where(v => SelectedVideos.Contains(v)).ToArray();

            // Create download view models
            var downloads = new List<DownloadViewModel>();
            for (var i = 0; i < orderedSelectedVideos.Length; i++)
            {
                var video = orderedSelectedVideos[i];

                // Generate file path
                var fileNamePrefix = (i + 1).ToString().PadLeft(orderedSelectedVideos.Length.ToString().Length, '0');
                var fileName = $"{fileNamePrefix} - {FileEx.MakeSafeFileName(video.Title)}.{SelectedFormat}";
                var filePath = Path.Combine(dirPath, fileName);

                // Ensure file paths are unique because user will not be able to confirm overwrites
                filePath = FileEx.MakeUniqueFilePath(filePath);

                // Create empty file to "lock in" the file path
                FileEx.CreateDirectoriesForFile(filePath);
                FileEx.CreateEmptyFile(filePath);

                // Create download view model
                var download = _viewModelFactory.CreateDownloadViewModel(video, filePath, SelectedFormat);

                // Add to list
                downloads.Add(download);
            }

            // Close dialog
            Close(downloads);
        }

        public void CopyTitle() => Clipboard.SetText(DisplayName);
    }
}