using Exiled.API.Interfaces;
using System.ComponentModel;

namespace OverwatchSystem
{
    public class Translations : ITranslation
    {
        [Description("The language of the plugin")]
        public string Language { get; set; } = "English";

        [Description("Translation for 'Brak'")]
        public string None { get; set; } = "None";

        [Description("Translation for 'System Moderacji'")]
        public string ModerationSystem { get; set; } = "Moderation System";

        [Description("Translation for 'Nick'")]
        public string Nickname { get; set; } = "Nickname";

        [Description("Translation for 'RP'")]
        public string Roleplay { get; set; } = "RP";

        [Description("Translation for 'CInfo'")]
        public string CustomInfo { get; set; } = "Custom Info";

        [Description("Translation for 'ID'")]
        public string Id { get; set; } = "ID";

        [Description("Translation for 'UCR'")]
        public string CustomRole { get; set; } = "Custom Role";

        [Description("Translation for 'Rola'")]
        public string Role { get; set; } = "Role";

        [Description("Translation for 'Ostatnia Pomoc'")]
        public string LastHelp { get; set; } = "Last Help";

        [Description("Translation for 'Inventory'")]
        public string Inventory { get; set; } = "Inventory";

        [Description("Translation for 'Empty'")]
        public string Empty { get; set; } = "Empty";

        [Description("Translation for 'Brak uprawnień!'")]
        public string NoPermissions { get; set; } = "No permissions!";

        [Description("Translation for 'Nie masz uprawnień do korzystania z systemu moderacji.'")]
        public string NoPermissionsDescription { get; set; } = "You don't have permissions to use the moderation system.";
    }
} 