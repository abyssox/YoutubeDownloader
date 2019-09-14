$ffmpegFilePath = "$PSScriptRoot\ffmpeg.exe"

# Check if already exists
if (Test-Path $ffmpegFilePath) {
    Write-Host "Skipped downloading ffmpeg, file already exists."
    exit
}

Write-Host "Downloading ffmpeg..."

# Download the zip archive
$url = "https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-4.2-win64-static.zip"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$wc = New-Object System.Net.WebClient
$wc.DownloadFile($url, "$ffmpegFilePath.zip")
$wc.Dispose()

# Extract ffmpeg.exe from the archive
Add-Type -Assembly System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
$zip.Entries | Where {$_.Name -like "ffmpeg.exe"} | ForEach {[System.IO.Compression.ZipFileExtensions]::ExtractToFile($_, $ffmpegFilePath, $true)}
$zip.Dispose()

# Delete the archive
Remove-Item "$ffmpegFilePath.zip" -Force

Write-Host "Done downloading ffmpeg."