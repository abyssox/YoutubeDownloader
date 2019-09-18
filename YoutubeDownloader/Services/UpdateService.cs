﻿using System;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace YoutubeDownloader.Services
{
    public class UpdateService
    {
        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver("calis2002", "YoutubeDownloader", "YoutubeDownloader.zip"),
            new ZipPackageExtractor());

        private Version _updateVersion;
        private bool _updaterLaunched;

        public async Task<Version> CheckForUpdatesAsync()
        {
            var check = await _updateManager.CheckForUpdatesAsync();
            return check.CanUpdate ? check.LastVersion : null;
        }

        public async Task PrepareUpdateAsync(Version version)
        {
            try
            {
                await _updateManager.PrepareUpdateAsync(_updateVersion = version);
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                // Ignore race conditions
            }
            catch (LockFileNotAcquiredException)
            {
                // Ignore race conditions
            }
        }

        public void FinalizeUpdate(bool needRestart)
        {
            try
            {
                if (_updateVersion == null || _updaterLaunched)
                    return;

                _updateManager.LaunchUpdater(_updateVersion, needRestart);

                _updaterLaunched = true;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                // Ignore race conditions
            }
            catch (LockFileNotAcquiredException)
            {
                // Ignore race conditions
            }
        }

        public void Dispose() => _updateManager.Dispose();
    }
}