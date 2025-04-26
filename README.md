
[![downloads](https://img.shields.io/github/downloads/DiabeloDev/OverwatchSystem/total?style=for-the-badge&logo=icloud&color=%233A6D8C)](https://github.com/diabelo/OverwatchSystem/releases/latest)
![Latest](https://img.shields.io/github/v/release/DiabeloDev/OverwatchSystem?style=for-the-badge&label=Latest%20Release&color=%23D91656)

# Overwatch System for EXILED

A comprehensive overwatch system plugin for SCP: Secret Laboratory servers.

## Minimum Requirements
- EXILED Framework v9.5.1 or higher

## Features
- Advanced overwatch system implementation
- Automatic update system with backup functionality
- Configurable logging system
- Customizable configuration options

## Installation
1. Download the latest release from the [releases page](https://github.com/DiabeloDev/OverwatchSystem/releases/latest)
2. Place the plugin DLL in your server's `EXILED/Plugins` directory
3. Restart your server

## Configuration
```yaml
overwatch_system:
  is_enabled: true
  debug: false
  # Auto Update:
  auto_update: true
  enable_logging_auto_update: true
  enable_backup: false
```
## Trans
'''yaml
overwatch_system:
# The language of the plugin
  language: 'English'
  none: 'None'
  moderation_system: 'Moderation System'
  nickname: 'Nickname'
  roleplay: 'RP'
  custom_info: 'Custom Info'
  id: 'ID'
  custom_role: 'Custom Role'
  role: 'Role'
  last_help: 'Last Help'
  inventory: 'Inventory'
  empty: 'Empty'
  no_permissions: 'No permissions!'
  no_permissions_description: 'You don''t have permissions to use the moderation system.'
'''

## Support
For support, please:
- Open an issue on the [GitHub repository](https://github.com/DiabeloDev/OverwatchSystem/issues)
- Contact the author directly

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
