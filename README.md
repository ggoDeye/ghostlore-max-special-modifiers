# Max Special Modifiers

This mod maximizes special modifier values (Keropok, Orang Bunian, Awakened) to their maximum values and allows configuration of which implicit affixes are available for each tag type.

## Features

1. **Maximizes Special Modifiers**: All implicit modifiers from Keropok, Orang Bunian, and Awakened sources are automatically set to their maximum values.

2. **Configurable Implicit Selection**: Choose which implicit affixes are available for each tag type through a JSON configuration file.

3. **Keropok System Control**: Ensures Keropok food items get exactly 6 non-implicit modifiers before the Keropok implicit is added.

## Configuration

The mod creates a configuration file at: `%USERPROFILE%\AppData\LocalLow\ATATGames\Ghostlore\Mods\MaxSpecialModifiers\config.json`

### Configuration Structure

```json
{
  "Keropok": {
    "Increased buff effect": true,
    "Increased buff duration": false,
    "HP Regen": false,
    "Damage Reflection": false,
    "Elemental Resistance": false,
    "Class Passives Multiplier": false,
    "HP Steal": false,
    "MP Steal": false,
    "Crisis Threshold": false,
    "Crisis Absorb": false,
    "Max HP": false,
    "Cold Chance Defense": false,
    "Movement Speed": false
  },
  "OrangBunian": {
    "Additional Minions": false,
    "Minion Max HP": false,
    "HP Multiplier": false,
    "Max Skill Uses": false,
    "Increased Movement Speed": false,
    "Elemental Chance": false,
    "Absorb": false,
    "Increased Projectile Radius": false,
    "Basic attack as fire": false,
    "Basic attack as ice": false,
    "Fire penetration": false,
    "Ice penetration": false,
    "Blind on hit": false,
    "Slow on hit": false,
    "Fire Resistance Cap": false,
    "Ice Resistance Cap": false,
    "Attack Damage": false,
    "Cooldown Reduction": false,
    "Skill Speed": false,
    "Class Passives Multiplier": false,
    "Triggered Chance No Charge Use": false,
    "Triggered Damage Multiplier": false,
    "Crisis Damage": false,
    "Minion Movement Speed": false
  },
  "Awakened": {
    "Minion Damage": false,
    "Minion Avoidance": false,
    "MP Multiplier": false,
    "Cooldown Reduction": false,
    "Elemental Multiplier": false,
    "Elemental Resistance": false,
    "Projectile Speed": false,
    "Armour Break": false,
    "Basic attack as lightning": false,
    "Basic attack as poison": false,
    "Lightning penetration": false,
    "Poison penetration": false,
    "Frenzy on hit": false,
    "Agility on hit": false,
    "Lightning Resistance Cap": false,
    "Poison Resistance Cap": false,
    "Skill Damage": false,
    "Critical Hit Multiplier": false,
    "Crisis Threshold": false,
    "Triggered Chance No Charge Use": false,
    "Triggered Skill Speed": false,
    "Crisis Absorb": false,
    "Movement Skill Distance Multiplier": false
  }
}
```

### How to Configure

1. Set `true` for affixes you want to be available for selection
2. Set `false` for affixes you want to disable
3. The mod will select the first enabled affix from the list when an implicit is rolled
4. If no affixes are enabled, the original random selection will be used
5. Save the file and restart the game for changes to take effect

## Default Configuration

By default, only "Increased buff effect" is enabled for Keropok items, while all other affixes are disabled. This means:

- **Keropok items**: Will always get "Increased buff effect" implicit
- **Orang Bunian items**: Will use original random selection (no affixes enabled)
- **Awakened items**: Will use original random selection (no affixes enabled)

## Usage

1. Install the mod by copying the `MaxSpecialModifiers` folder to your game's `mods` directory
2. Start the game - the configuration file will be created automatically
3. Edit the configuration file to customize which affixes you want
4. Restart the game for changes to take effect
5. Enjoy your customized implicit modifier selection!

## Technical Details

- The mod uses Harmony patching to intercept implicit modifier selection
- Configuration is loaded once when the mod starts
- Changes to the configuration file require a game restart
- The mod maintains compatibility with the original game's Keropok progression system
