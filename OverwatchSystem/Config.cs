using System.ComponentModel;
using Exiled.API.Interfaces;

namespace OverwatchSystem
{
    public class Config : IConfig
    {
        [Description("Whether or not the plugin is enabled")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether or not debug messages should be shown")]
        public bool Debug { get; set; } = false;
        
        [Description("Auto Update:")]
        public bool AutoUpdate { get; set; } = true;
        public bool EnableLoggingAutoUpdate { get; set; } = true;
        public bool EnableBackup { get; set; } = false;
    }
}