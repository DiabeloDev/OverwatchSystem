using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;
using System.Linq;

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
                Log("Initiating update check...", LogLevel.Info);
                
                var response = await HttpClient.GetAsync(RepositoryUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"Failed to check for updates. Status: {response.StatusCode}";
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            errorMessage += " - Repository not found. Please check if the repository URL is correct.";
                            break;
                        case HttpStatusCode.RequestEntityTooLarge:
                            errorMessage += " - Request too large. Please check your connection.";
                            break;
                        case HttpStatusCode.Unauthorized:
                            errorMessage += " - Unauthorized access. Please check your credentials.";
                            break;
                        case HttpStatusCode.Forbidden:
                            errorMessage += " - Access forbidden. Please check your permissions.";
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            errorMessage += " - GitHub service is temporarily unavailable. Please try again later.";
                            break;
                        default:
                            errorMessage += " - Please check your internet connection and try again.";
                            break;
                    }
                    Log(errorMessage, LogLevel.Error);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();

                var latestVersion = ExtractLatestVersion(content);
                var downloadUrl = ExtractDownloadUrl(content);

                if (latestVersion == null || downloadUrl == null)
                {
                    Log("Failed to parse update information. Please check if the release format is correct.", LogLevel.Error);
                    return;
                }

                if (IsNewerVersion(CurrentVersion, latestVersion))
                {
                    string[] updateLines =
                    {
                        $"New version available: {latestVersion}",
                        $"Current version: {CurrentVersion}",
                        autoUpdate
                            ? "Automatic update is enabled. Starting update process..."
                            : "Automatic update is disabled. Please download and install the update manually.",
                    };
                    LogInBox(updateLines, LogLevel.Warn);

                    if (autoUpdate)
                    {
                        await UpdatePluginAsync(downloadUrl);
                        Log("Update completed successfully. Please restart the server to apply changes.", LogLevel.Info);
                    }
                }
                else
                {
                    Log("You are using the latest version. No update needed.", LogLevel.Info);
                }
            }
            catch (HttpRequestException ex)
            {
                Log($"Network error while checking for updates: {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                {
                    Log($"Inner error: {ex.InnerException.Message}", LogLevel.Error);
                }
            }
            catch (TaskCanceledException)
            {
                Log("Update check was cancelled due to timeout", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Log($"Unexpected error while checking for updates: {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                {
                    Log($"Inner error: {ex.InnerException.Message}", LogLevel.Error);
                }
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
                
                foreach (var asset in assets)
                {
                    var name = asset["name"]?.ToString();
                    if (name != null && name.Equals("OverwatchSystem.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        return asset["browser_download_url"]?.ToString();
                    }
                }

                Log("No matching 'OverwatchSystem.dll' file found in the release assets.", LogLevel.Error);
                return null;
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

        private static void LogInBox(string[] lines, LogLevel level)
        {
            int maxWidth = lines.Max(line => line.Length);
            string horizontalBorder = $"╔{new string('═', maxWidth + 2)}╗";

            Log(horizontalBorder, level);
            foreach (var line in lines)
            {
                Log($"║ {line.PadRight(maxWidth)} ║", level);
            }
            Log($"╚{new string('═', maxWidth + 2)}╝", level);
        }
    }
    internal enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}