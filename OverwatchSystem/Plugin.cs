using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace OverwatchSystem
{
    public class Plugin : Plugin<Config, Translations>
    {
        public override string Author { get; } = ".Diabelo";
        public override string Name { get; } = "OverwatchSystem";
        public override Version Version => new Version(1, 0, 2);
        public override Version RequiredExiledVersion { get; } = new Version(9, 6, 0);
        public override PluginPriority Priority { get; } = PluginPriority.Higher;
        public static Plugin Instance { get; private set; }
        public async override void OnEnabled()
        {
            Instance = this;
            await Extensions.UpdateChecker.RunAsync();
            Overwatch.Register();
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Instance = null;
            Overwatch.Unregister();
            base.OnDisabled();
        }
    }
}