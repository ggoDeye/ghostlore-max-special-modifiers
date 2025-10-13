# MaxSpecialModifiers Configuration Guide

## Overview

The mod now supports a modular configuration system that allows you to configure which implicit affixes are forced for different item tag types (Keropok, Orang Bunian, Awakened, etc.).

## Configuration File Location

The configuration file is automatically created at:

```
%USERPROFILE%\AppData\LocalLow\ATATGames\Ghostlore\max-special-modifiers.config.json
```

## Configuration Structure

```json
{
  "DebugLogging": false,
  "TagConfigurations": {
    "Keropok": {
      "ForcedAffixes": {
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
      }
    },
    "Orang Bunian": {
      "ForcedAffixes": {
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
      }
    },
    "Awakened": {
      "ForcedAffixes": {
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
  }
}
```

## Configuration Options

### Global Settings

- **`DebugLogging`** (boolean): Enable detailed debug logging for troubleshooting

### Tag Configuration

Each tag type (Keropok, Orang Bunian, Awakened, etc.) contains:

- **`ForcedAffixes`** (object): Dictionary of affixes to force for this tag type

### Affix Configuration

Each affix is configured with a simple boolean value:

- **`true`**: Force this affix to be applied to items with this tag
- **`false`**: Do not force this affix (item may get random affixes instead)

**Note**: StatID and Multiplicative requirements are handled automatically by the mod - users don't need to worry about these technical details!

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
  "TagConfigurations": {
    "Orang Bunian": {
      "ForcedAffixes": {
        "Additional Minions": true,
        "Minion Max HP": true,
        "HP Multiplier": true,
        "Max Skill Uses": true
      }
    }
  }
}
```

### Add Multiple Affixes to Keropok

```json
{
  "TagConfigurations": {
    "Keropok": {
      "ForcedAffixes": {
        "Increased buff effect": true,
        "HP Regen": true,
        "MP Steal": true,
        "Movement Speed": true
      }
    }
  }
}
```

### Disable Specific Affixes

```json
{
  "TagConfigurations": {
    "Keropok": {
      "ForcedAffixes": {
        "Increased buff effect": false,
        "Damage Reflection": true,
        "HP Steal": false,
        "Movement Speed": true
      }
    }
  }
}
```

### Enable All Awakened Affixes

```json
{
  "TagConfigurations": {
    "Awakened": {
      "ForcedAffixes": {
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
  }
}
```

## Available Affixes

The mod includes all possible implicit affixes for each tag type. Here are the complete lists:

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

**Note**: All StatID mappings and Multiplicative requirements are handled automatically by the mod.

## Troubleshooting

### Configuration Not Loading

- Ensure the JSON file is valid (use a JSON validator)
- Check that the file is in the correct location
- Look for error messages in the game logs

### Affixes Not Being Forced

- Verify `Enabled: true` for both the tag configuration and specific affix
- Check that the `StatID` matches exactly
- Enable `DebugLogging: true` to see detailed information
- Ensure the item has the correct tag

### Debug Information

Enable `DebugLogging: true` to see detailed information about:

- Which tags are being processed
- Which affixes are being found/not found
- When affixes are being added to items

## Future Enhancements

The configuration system is designed to be extensible. Future versions may include:

- Unity UI for in-game configuration
- Preset configurations for different playstyles
- Import/export functionality for sharing configurations
- Additional tag types beyond Keropok, Orang Bunian, and Awakened

## Backward Compatibility

The mod maintains backward compatibility:

- If no configuration file exists, default settings are used (all affixes enabled)
- If the configuration file is invalid, default settings are used
- The original Keropok behavior is preserved by default
- StatID and Multiplicative requirements are handled automatically
