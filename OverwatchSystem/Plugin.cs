using System;
using System.Linq;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using MEC;

namespace OverwatchSystem
{
    public class Plugin : Plugin<Config>
    {
        public override string Author { get; } = ".Diabelo";
        public override string Name { get; } = "OverwatchSystem";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 5, 1);
        public override PluginPriority Priority { get; } = PluginPriority.Higher;
        public static Plugin Instance { get; private set; }
        public override void OnEnabled()
        {
            Instance = this;
            Dependency();
            Overwatch.Register();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Overwatch.Unregister();
            base.OnDisabled();
            
        }
        private bool CheckForDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "Newtonsoft.Json");
        public void Dependency()
        {
            if (!CheckForDependency())
                Timing.CallContinuously(20f, () => Log.Error("You don't have the dependency Newtonsoft.Json installed!\nPlease install it AS SOON AS POSSIBLE!"));
        }
    }
}