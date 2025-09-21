![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/DiabeloDev/OverwatchSystem/total?style=for-the-badge) <br>
![Latest](https://img.shields.io/github/v/release/DiabeloDev/OverwatchSystem?style=for-the-badge&label=Latest%20Release&color=%23D91656)

# Overwatch System for EXILED

A comprehensive overwatch system plugin for SCP: Secret Laboratory.

## Requirements
- [EXILED](https://github.com/ExMod-Team/EXILED) v9.9.2 or higher
- [HintServiceMeow](https://github.com/MeowServer/HintServiceMeow) v5.4.0 or higher
- Newtonsoft.Json

## Installation
1. Download the latest release from the [releases page](https://github.com/DiabeloDev/OverwatchSystem/releases/latest)
2. Place the plugin `OverwatchSystem.dll` file into the `EXILED/Plugins` folder on your server.
3. Place the dependency `Newtonsoft.Json.dll` file into the `EXILED/Plugins/dependencies` folder on your server.
4. Restart your server

## Configuration
```yaml
overwatch_system:
  is_enabled: true
  debug: false
  # Auto Update:
  auto_update: true
```

## Translations
```yaml
overwatch_system:
  none: 'None'
  overwatch_system: 'Overwatch System'
  nickname: 'Nickname'
  roleplay: 'RP'
  custom_info: 'Custom Info'
  id: 'ID'
  role: 'Role'
  inventory: 'Inventory'
  empty: 'Empty'
  no_permissions: 'No permissions!'
  no_permissions_description: 'You don''t have permissions to use the moderation system.'
```

## Adding ICustomItemInfoProvider to Custom Items
To add custom item information to your items, implement the `ICustomItemInfoProvider` interface. Here's a complete example:

```cs
public class Test : ICustomItemInfoProvider
{
    public string AdditionalInfo { get; set; } = "TEST";
    public string CustomIcon { get; set; } = "⚔️";
}
```

## Showcase
![image](https://github.com/user-attachments/assets/05fe800a-8fc1-4a02-8de5-56e5a2ae3ea6)

## Support
For support, please:
- [Open an issue on the](https://github.com/DiabeloDev/OverwatchSystem/issues)
- Contact the author directly

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
