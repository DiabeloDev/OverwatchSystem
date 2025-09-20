using System;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace OverwatchSystem
{
    public class Plugin : Plugin<Config, Translations>
    {
        public override string Author { get; } = ".Diabelo";
        public override string Name { get; } = "OverwatchSystem";
        public override Version Version => new Version(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 9, 2);
        public override PluginPriority Priority { get; } = PluginPriority.Higher;
        public static Plugin Instance { get; private set; }
        public override void OnEnabled()
        {
            Instance = this;
            Overwatch.Register();
            Update();            
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Instance = null;
            Overwatch.Unregister();
            base.OnDisabled();
        }
        private void Update()
        {
            try
            {
                if (Exiled.Loader.Loader.GetPlugin("AutoUpdate") == null)
                {
                    if (!Config.AutoUpdate)
                    {
                        Log.Warn("AutoUpdate plugin not found. Skipping integration. If you want AutoUpdate, download it from https://github.com/DiabeloDev/AutoUpdate");
                    }
                    return;
                }

                Type updaterType = Type.GetType("AutoUpdate.Updater, AutoUpdate");
                if (updaterType == null)
                {
                    Log.Warn("AutoUpdate plugin was found, but its 'Updater' class could not be loaded. Integration failed.");
                    return;
                }

                MethodInfo registerMethod = updaterType.GetMethod("RegisterPluginForUpdates", BindingFlags.Public | BindingFlags.Static);
                if (registerMethod == null)
                {
                    Log.Warn("Found AutoUpdate's 'Updater' class, but the 'RegisterPluginForUpdates' method is missing. Integration failed.");
                    return;
                }

                object[] parameters = new object[]
                {
                    Instance.Name,
                    "DiabeloDev",
                    "OverwatchSystem",
                    "OverwatchSystem.dll"
                };

                registerMethod.Invoke(null, parameters);
                
                Log.Debug("Successfully registered with AutoUpdate for automatic updates!");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred while trying to integrate with AutoUpdate: {ex.Message}");
            } 
        }
    }
}
