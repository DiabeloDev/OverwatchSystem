using Exiled.API.Interfaces;

namespace OverwatchSystem
{
    public class Translations : ITranslation
    {
        public string None { get; set; } = "None";
        public string OverwatchSystem { get; set; } = "Overwatch System";
        public string Nickname { get; set; } = "Nickname";
        public string Roleplay { get; set; } = "RP";
        public string CustomInfo { get; set; } = "Custom Info";
        public string Id { get; set; } = "ID";
        public string Role { get; set; } = "Role";
        public string Inventory { get; set; } = "Inventory";
        public string Empty { get; set; } = "Empty";
        public string NoPermissions { get; set; } = "No permissions!";
        public string NoPermissionsDescription { get; set; } = "You don't have permissions to use the moderation system.";
    }
} 