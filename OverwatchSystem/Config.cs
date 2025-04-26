using System.ComponentModel;
using Exiled.API.Interfaces;

namespace OverwatchSystem
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        
        [Description("Auto Update:")]
        public bool AutoUpdate { get; set; } = true;
        public bool EnableLoggingAutoUpdate { get; set; } = true;
        public bool EnableBackup { get; set; } = false;
    }
}