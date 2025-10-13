# MaxSpecialModifiers

A Ghostlore mod that maximizes special modifier values (Keropok, Orang Bunian, Awakened) to their maximum values and provides configurable forced affix selection for each tag type.

## Architecture Overview

This mod uses Harmony patching to intercept and modify the item affix system in Ghostlore. It operates at the `ItemInstance.AddOrReplaceImplicit` level to control which affixes are applied and maximize their values.

## Core Features

1. **Automatic Maximization**: All implicit modifiers from Keropok, Orang Bunian, and Awakened sources are automatically set to their maximum values using reflection to access private modifier fields.

2. **Configurable Forced Affixes**: Choose which specific affixes are forced onto items through a simplified JSON configuration structure.

3. **Keropok Progression Control**: Custom 6-modifier completion system that ensures Keropok food items get exactly 6 non-implicit modifiers before the Keropok implicit is added.

4. **Tag-Based Affix Selection**: Supports three distinct item tag types with separate affix pools and configuration.

## Technical Implementation

### Configuration System

The mod creates a configuration file at: `%USERPROFILE%\AppData\LocalLow\ATATGames\Ghostlore\max-special-modifiers.config.json`

#### Configuration Structure (Simplified)

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

### Core Classes

#### `ModConfig.cs`

- **Purpose**: Configuration management and JSON serialization/deserialization
- **Key Features**:
  - Simplified flat structure (removed nested `TagConfigurations` and `ForcedAffixes`)
  - Static `AffixNameMapping` dictionary for display name to `ItemAffixName` conversion
  - Automatic configuration file creation with defaults
  - Error handling for invalid JSON

#### `MaxSpecialModifiersPatch.cs`

- **Purpose**: Main Harmony patching logic
- **Key Methods**:
  - `Prefix(AddOrReplaceImplicit)`: Intercepts affix addition and applies forced affixes
  - `Postfix(AddOrReplaceImplicit)`: Maximizes implicit modifiers after addition
  - `ForceImplicitAffixes()`: Handles forced affix selection and application
  - `MaximizeImplicitModifiers()`: Sets modifier values to maximum using reflection
  - `SetModifierInstanceToMax()`: Core maximization logic with level scaling

#### `KeropokManager.cs`

- **Purpose**: Custom Keropok progression control
- **Key Features**: 6-modifier completion system for Keropok items

### Harmony Patches

#### `ItemInstancePatch`

- **Target**: `ItemInstance.AddOrReplaceImplicit`
- **Prefix**: Intercepts affix addition, applies forced affixes if configured
- **Postfix**: Maximizes all implicit modifiers on the item

#### `AwakenedItemManagerPatch`

- **Target**: `AwakenedItemManager.IncrementKeropokKillCount`
- **Purpose**: Controls Keropok progression with custom completion logic

### Configuration Behavior

- **Default State**: All affixes enabled (`true`) for maximum compatibility
- **Selection Logic**: Random selection from enabled affixes per tag type
- **Fallback**: If no affixes enabled, original game logic is used
- **Hot Reload**: Configuration changes require game restart

## Development Setup

### Prerequisites

- .NET Framework 4.7.1
- Visual Studio or compatible IDE
- Ghostlore modding environment

### Build Process

```bash
cd max-special-modifiers
dotnet build
```

### Key Dependencies

- `Assembly-CSharp.dll` - Ghostlore game assembly
- `UnityEngine.dll` - Unity engine components
- `Newtonsoft.Json.dll` - JSON serialization
- Harmony library - Runtime patching

## Architecture Decisions

### Configuration Simplification

- **Before**: Complex nested structure with `TagConfigurations["TagName"]["ForcedAffixes"]["affix"]`
- **After**: Flat structure with direct `TagName["affix"]` access
- **Rationale**: Improved readability and maintainability

### Affix Name Mapping

- **Challenge**: Game uses internal `ItemAffixName` while users prefer display names
- **Solution**: Static mapping dictionary in `ModConfig` handles conversion automatically
- **Benefit**: User-friendly configuration with reliable internal name resolution

### Reflection Usage

- **Purpose**: Access private `lower` and `upper` fields in `ModifierInstance`
- **Risk**: Brittle if game updates change field names
- **Mitigation**: Cached `FieldInfo` objects for performance

### Tag Matching Strategy

- **Approach**: `Contains()` matching for tag names (e.g., "Keropok" matches "Keropok Food")
- **Rationale**: Flexible matching handles variations in game tag naming
- **Priority**: Exact matches first, then partial matches

## Performance Considerations

- **Cached Reflection**: `FieldInfo` objects cached at class level
- **Efficient Tag Matching**: Early exit on first match found
- **Minimal Allocations**: Reuse of collections and objects where possible
- **Conditional Logging**: Debug logging only when enabled

## Error Handling

- **Configuration Loading**: Graceful fallback to defaults on JSON errors
- **Reflection Failures**: Logged errors with fallback to original game behavior
- **Affix Resolution**: Detailed logging for missing affixes with graceful degradation

## Testing Strategy

- **Unit Testing**: Configuration serialization/deserialization
- **Integration Testing**: Harmony patch application and affix resolution
- **Game Testing**: End-to-end validation with actual Ghostlore items

## Future Enhancements

- **In-Game UI**: Unity-based configuration interface
- **Preset System**: Predefined configurations for different playstyles
- **Performance Profiling**: Detailed metrics for affix processing
- **Additional Tags**: Support for more item tag types beyond the current three
