﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
    {
        private static readonly Dictionary<string, DownloadQuality> AvaliableQualities = new Dictionary<string, DownloadQuality>()
        {
            { "Maximum", DownloadQuality.Maximum },
            { "Hoch (bis zu 1080p)", DownloadQuality.High },
            { "Mittel (bis zu 720p)", DownloadQuality.Medium },
            { "Niedrig (bis zu 480p)", DownloadQuality.Low },
            { "Minimum", DownloadQuality.Minimum }
        };
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public string Title { get; set; }

        public IReadOnlyList<Video> AvailableVideos { get; set; }

        public IReadOnlyList<Video> SelectedVideos { get; set; }

        public IReadOnlyList<string> AvailableFormats { get; } = AvaliableQualities.Select(q => $@"{q.Key} / mp4").Concat(new[] { "mp3", "ogg" }).ToList();

        public string SelectedFormat { get; set; }

        public DownloadMultipleSetupViewModel(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        public void OnViewLoaded()
        {
            // Select last used format
            SelectedFormat = !string.IsNullOrWhiteSpace(_settingsService.LastFormat) && AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        public bool CanConfirm => SelectedVideos != null && SelectedVideos.Any();

        public void Confirm()
        {
            var dirPath = "";

            // Check if Default Download Directory is set in settings, otherwise prompt user for output directory path
            if (string.IsNullOrWhiteSpace(_settingsService.DefaultDownloadDirectory))
            { 
                dirPath = _dialogManager.PromptDirectoryPath();

                // If canceled - return
                if (string.IsNullOrWhiteSpace(dirPath))
                    return;
            } else
            {
                dirPath = _settingsService.DefaultDownloadDirectory;
            }

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Split quality and format
            var (quality, format) = ParseToQualityAndFormat(SelectedFormat);

            // Make sure selected videos are ordered in the same way as available videos
            var orderedSelectedVideos = AvailableVideos.Where(v => SelectedVideos.Contains(v)).ToArray();

            // Create download view models
            var downloads = new List<DownloadViewModel>();
            for (var i = 0; i < orderedSelectedVideos.Length; i++)
            {
                var video = orderedSelectedVideos[i];

                // Generate file path
                var number = (i + 1).ToString().PadLeft(orderedSelectedVideos.Length.ToString().Length, '0');
                var fileName = FileNameGenerator.GenerateFileName(_settingsService.FileNameTemplate, video, format, number);
                var filePath = Path.Combine(dirPath, fileName);

                // If file exists - either skip it or generate a unique file path, depending on user settings
                if (File.Exists(filePath))
                {
                    if (_settingsService.ShouldSkipExistingFiles)
                        continue;

                    filePath = FileEx.MakeUniqueFilePath(filePath);
                }

                // Create empty file to "lock in" the file path
                FileEx.CreateDirectoriesForFile(filePath);
                FileEx.CreateEmptyFile(filePath);

                // Create download view model
                var download = _viewModelFactory.CreateDownloadViewModel(video, filePath, format, quality);

                // Add to list
                downloads.Add(download);
            }

            // Close dialog
            Close(downloads);
        }
        private (DownloadQuality, string) ParseToQualityAndFormat(string selectedFormat)
        {
            var parts = selectedFormat.Split("/");
            var quality = parts.Length > 1 ? AvaliableQualities[parts[0].Trim()] : DownloadQuality.Maximum;
            var format = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim();
            return (quality, format);
        }

        public void CopyTitle() => Clipboard.SetText(Title);
    }
}