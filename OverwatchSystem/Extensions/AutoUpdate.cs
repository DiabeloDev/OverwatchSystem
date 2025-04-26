using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;

namespace OverwatchSystem.Extensions
{
    public static class UpdateChecker
    {
        private static readonly string RepositoryUrl = "https://api.github.com/repos/DiabeloDev/OverwatchSystem/releases/latest";
        private static readonly string PluginPath = Path.Combine(Paths.Plugins, "OverwatchSystem.dll");
        private static readonly string CurrentVersion = Plugin.Instance.Version.ToString();
        private static readonly HttpClient HttpClient = new HttpClient();
        
        static UpdateChecker()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("OverwatchSystem-UpdateChecker");
        }
        
        public static async Task RunAsync()
        {
            if (!Plugin.Instance.Config.AutoUpdate)
            {
                Log("Automatic update is disabled. Please check for updates manually.", LogLevel.Info);
                return;
            }

            Log("Checking for updates...", LogLevel.Info);
            await CheckForUpdatesAsync(Plugin.Instance.Config.AutoUpdate);
        }

        private static void Log(string message, LogLevel level)
        {
            if (!Plugin.Instance.Config.EnableLoggingAutoUpdate)
                return;

            switch (level)
            {
                case LogLevel.Info:
                    Exiled.API.Features.Log.Info(message);
                    break;
                case LogLevel.Warn:
                    Exiled.API.Features.Log.Warn(message);
                    break;
                case LogLevel.Error:
                    Exiled.API.Features.Log.Error(message);
                    break;
            }
        }

        private static async Task CheckForUpdatesAsync(bool autoUpdate)
        {
            try
            {
                var response = await HttpClient.GetAsync(RepositoryUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Log($"Failed to check for updates: {response.StatusCode} - {response.ReasonPhrase}", LogLevel.Error);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var latestVersion = ExtractLatestVersion(content);
                var downloadUrl = ExtractDownloadUrl(content);

                if (latestVersion == null || downloadUrl == null)
                {
                    Log("Failed to parse update information.", LogLevel.Error);
                    return;
                }

                if (IsNewerVersion(CurrentVersion, latestVersion))
                {
                    Log($"A new version is available: {latestVersion} (current: {CurrentVersion})", LogLevel.Warn);

                    if (autoUpdate)
                    {
                        Log("Automatic update is enabled. Downloading and applying the update...", LogLevel.Info);
                        await UpdatePluginAsync(downloadUrl);
                        Log("Plugin updated successfully. Please restart the server to apply changes.", LogLevel.Info);
                    }
                    else
                    {
                        Log("Automatic update is disabled. Please download the update manually.", LogLevel.Warn);
                    }
                }
                else
                {
                    Log("You are using the latest version.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Log($"Error while checking for updates: {ex.Message}", LogLevel.Error);
            }
        }

        private static string ExtractLatestVersion(string json)
        {
            try
            {
                var obj = JObject.Parse(json);
                return obj["tag_name"]?.ToString();
            }
            catch (Exception ex)
            {
                Log($"Failed to extract the latest version: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        private static string ExtractDownloadUrl(string json)
        {
            try
            {
                var obj = JObject.Parse(json);
                var assets = obj["assets"] as JArray;
                
                if (assets == null || assets.Count == 0)
                {
                    Log("No assets found in the release", LogLevel.Error);
                    return null;
                }

                return assets[0]["browser_download_url"]?.ToString();
            }
            catch (Exception ex)
            {
                Log($"Failed to extract download URL: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        private static bool IsNewerVersion(string currentVersion, string latestVersion)
        {
            if (Version.TryParse(currentVersion, out var current) && 
                Version.TryParse(latestVersion, out var latest))
            {
                return latest > current;
            }

            Log("Failed to compare versions. Using current version as the latest.", LogLevel.Warn);
            return false;
        }

        private static async Task UpdatePluginAsync(string downloadUrl)
        {
            try
            {
                var pluginData = await HttpClient.GetByteArrayAsync(downloadUrl);
                await BackupAndWritePluginAsync(pluginData);
            }
            catch (Exception ex)
            {
                Log($"Error during plugin update: {ex.Message}", LogLevel.Error);
            }
        }

        private static async Task BackupAndWritePluginAsync(byte[] pluginData)
        {
            if (Plugin.Instance.Config.EnableBackup && File.Exists(PluginPath))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string backupPath = $"{PluginPath}.{timestamp}.backup";
                
                try
                {
                    File.Copy(PluginPath, backupPath, true);
                    Log($"Backup created: {backupPath}", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Log($"Failed to create backup: {ex.Message}", LogLevel.Warn);
                }
            }

            File.WriteAllBytes(PluginPath, pluginData);
        }
    }

    internal enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}