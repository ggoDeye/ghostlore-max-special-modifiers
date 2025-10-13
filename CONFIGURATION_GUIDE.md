# MaxSpecialModifiers - User Guide

## What This Mod Does

This mod automatically maximizes the special implicit affixes on items with Keropok, Orang Bunian, and Awakened tags. You can configure which specific affixes are forced onto these items, giving you control over your gear progression.

## Key Features

- **Automatic Maximization**: All special modifier affixes are set to their maximum possible values
- **Configurable Affixes**: Choose which specific affixes are forced onto your items
- **Three Item Types Supported**:
  - Keropok items (13 available affixes)
  - Orang Bunian items (17 available affixes)
  - Awakened items (22 available affixes)
- **Keropok Progression Control**: Custom 6-modifier completion system for Keropok items

## Configuration File Location

The configuration file is automatically created at:

```
%USERPROFILE%\AppData\LocalLow\ATATGames\Ghostlore\max-special-modifiers.config.json
```

## Configuration Structure

```json
{
  "DebugLogging": false,
  "Keropok": {
    "Increased buff effect": true,
    "Increased buff duration": true,
    "HP Regen": true,
    "Damage Reflection": true,
    "Elemental Resistance": true,
    "Class Passives Multiplier": true,
    "HP Steal": true,
    "MP Steal": true,
    "Crisis Threshold": true,
    "Crisis Absorb": true,
    "Max HP": true,
    "Cold Chance Defense": true,
    "Movement Speed": true
  },
  "OrangBunian": {
    "Additional Minions": true,
    "Minion Max HP": true,
    "HP Multiplier": true,
    "Max Skill Uses": true,
    "Increased Movement Speed": true,
    "Elemental Chance": true,
    "Absorb": true,
    "Increased Projectile Radius": true,
    "Basic attack as fire": true,
    "Basic attack as ice": true,
    "Fire penetration": true,
    "Ice penetration": true,
    "Blind on hit": true,
    "Slow on hit": true,
    "Fire Resistance Cap": true,
    "Ice Resistance Cap": true,
    "Attack Damage": true
  },
  "Awakened": {
    "Minion Damage": true,
    "Minion Avoidance": true,
    "MP Multiplier": true,
    "Cooldown Reduction": true,
    "Elemental Multiplier": true,
    "Elemental Resistance": true,
    "Projectile Speed": true,
    "Armour Break": true,
    "Basic attack as lightning": true,
    "Basic attack as poison": true,
    "Lightning penetration": true,
    "Poison penetration": true,
    "Frenzy on hit": true,
    "Agility on hit": true,
    "Lightning Resistance Cap": true,
    "Poison Resistance Cap": true,
    "Skill Damage": true,
    "Critical Hit Multiplier": true,
    "Crisis Threshold": true,
    "Triggered Chance No Charge Use": true,
    "Triggered Skill Speed": true,
    "Crisis Absorb": true,
    "Movement Skill Distance Multiplier": true
  }
}
```

## Configuration Options

### Global Settings

- **`DebugLogging`** (boolean): Enable detailed debug logging for troubleshooting

### Affix Configuration

Each affix is configured with a simple true/false value:

- **`true`**: Force this affix to be applied to items with this tag
- **`false`**: Do not force this affix (item may get random affixes instead)

**Note**: All technical requirements are handled automatically by the mod!

## How to Configure

1. **First Time Setup**:

   - Start the game with the mod enabled
   - The configuration file will be automatically created with default settings
   - Close the game

2. **Edit Configuration**:

   - Navigate to the configuration file location
   - Open `max-special-modifiers.config.json` in any text editor
   - Modify the settings as desired
   - Save the file

3. **Apply Changes**:
   - Restart the game
   - The new configuration will be loaded automatically

## Examples

### Enable Orang Bunian Forced Affixes

```json
{
  "OrangBunian": {
    "Additional Minions": true,
    "Minion Max HP": true,
    "HP Multiplier": true,
    "Max Skill Uses": true
  }
}
```

### Add Multiple Affixes to Keropok

```json
{
  "Keropok": {
    "Increased buff effect": true,
    "HP Regen": true,
    "MP Steal": true,
    "Movement Speed": true
  }
}
```

### Disable Specific Affixes

```json
{
  "Keropok": {
    "Increased buff effect": false,
    "Damage Reflection": true,
    "HP Steal": false,
    "Movement Speed": true
  }
}
```

### Enable All Awakened Affixes

```json
{
  "Awakened": {
    "Minion Damage": true,
    "Minion Avoidance": true,
    "MP Multiplier": true,
    "Cooldown Reduction": true,
    "Elemental Multiplier": true,
    "Elemental Resistance": true,
    "Projectile Speed": true,
    "Armour Break": true,
    "Basic attack as lightning": true,
    "Basic attack as poison": true,
    "Lightning penetration": true,
    "Poison penetration": true,
    "Frenzy on hit": true,
    "Agility on hit": true,
    "Lightning Resistance Cap": true,
    "Poison Resistance Cap": true,
    "Skill Damage": true,
    "Critical Hit Multiplier": true,
    "Crisis Threshold": true,
    "Triggered Chance No Charge Use": true,
    "Triggered Skill Speed": true,
    "Crisis Absorb": true,
    "Movement Skill Distance Multiplier": true
  }
}
```

## Available Affixes

### Keropok Affixes (13 total)

- Increased buff effect, Increased buff duration, HP Regen, Damage Reflection
- Elemental Resistance, Class Passives Multiplier, HP Steal, MP Steal
- Crisis Threshold, Crisis Absorb, Max HP, Cold Chance Defense, Movement Speed

### Orang Bunian Affixes (17 total)

- Additional Minions, Minion Max HP, HP Multiplier, Max Skill Uses
- Increased Movement Speed, Elemental Chance, Absorb, Increased Projectile Radius
- Basic attack as fire, Basic attack as ice, Fire penetration, Ice penetration
- Blind on hit, Slow on hit, Fire Resistance Cap, Ice Resistance Cap, Attack Damage

### Awakened Affixes (22 total)

- Minion Damage, Minion Avoidance, MP Multiplier, Cooldown Reduction
- Elemental Multiplier, Elemental Resistance, Projectile Speed, Armour Break
- Basic attack as lightning, Basic attack as poison, Lightning penetration, Poison penetration
- Frenzy on hit, Agility on hit, Lightning Resistance Cap, Poison Resistance Cap
- Skill Damage, Critical Hit Multiplier, Crisis Threshold, Triggered Chance No Charge Use
- Triggered Skill Speed, Crisis Absorb, Movement Skill Distance Multiplier

## Troubleshooting

### Configuration Not Loading

- Ensure the JSON file is valid (use a JSON validator)
- Check that the file is in the correct location
- Look for error messages in the game logs

### Affixes Not Being Forced

- Verify the affix is set to `true` in the configuration
- Check that the affix name matches exactly (case-sensitive)
- Enable `DebugLogging: true` to see detailed information
- Ensure the item has the correct tag

### Debug Information

Enable `DebugLogging: true` to see detailed information about:

- Which tags are being processed
- Which affixes are being found/not found
- When affixes are being added to items

## Important Notes

- **Backup your saves** before using this mod
- All affixes are enabled by default for maximum compatibility
- The mod maintains backward compatibility with existing saves
- If no configuration file exists, default settings are used (all affixes enabled)
- If the configuration file is invalid, default settings are used
