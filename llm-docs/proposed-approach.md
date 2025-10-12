# ModifierInstance Max Value Approaches

## Overview

This document outlines potential approaches for modifying `ModifierInstance.Lower` values to be set to `ModifierInstance.Modifier.LowerMax` when created, specifically for special modifiers with tags like "Keropok", "Orang Bunian", and "Awakened".

## Key Findings from Decompiled Code Analysis

### ModifierInstance Creation Flow

1. **Primary Creation Point**: `Modifier.CreateInstance()` method in `Modifier.cs` (lines 115-126)

   - This is where `ModifierInstance` objects are created with calculated `lower` and `upper` values
   - Uses random roll (`UnityEngine.Random.value`) to determine values within min/max ranges
   - Called from `ModifierList.AddAffix()` → `ModifierList.AddMod()` → `modifier.CreateInstance()`

2. **Special Modifier Systems**:

   - **Awakened Items**: Handled by `AwakenedItemManager.cs`
     - Uses `item.AddOrReplaceImplicit(tags)` to add awakened modifiers
     - Tags defined in `AwakenedItemManager.tags` array
   - **Keropok Items**: Also handled by `AwakenedItemManager.cs`
     - Uses `KeropokTags` and `KeropokCurseTags` arrays
     - Special logic in `IncrementKeropokKillCount()` method
   - **Orang Bunian**: Handled by `OrangBunianManager.cs`
     - Primarily deals with realm portals and blessings
     - Less direct modifier creation compared to Awakened/Keropok

3. **Tag System**:
   - `ItemAffix` class contains `GameTag[] Tags` property
   - Tags are used to identify special affixes (Keropok, Orang Bunian, Awakened)
   - `ItemAffix.MatchesRequirements()` method checks tag compatibility

## Potential Implementation Approaches

### Approach 1: Patch Modifier.CreateInstance() (Recommended)

**Target**: `Modifier.CreateInstance(int level, ModifierInstanceAttributes attributes, float modifier, ItemAffix affix)`

**Pros**:

- Direct control over the creation process
- Can check `ItemAffix.Tags` to identify special modifiers
- Clean, centralized approach
- Preserves all existing functionality

**Implementation Strategy**:

```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(Modifier), "CreateInstance")]
static ModifierInstance Postfix(ModifierInstance __result, Modifier __instance, int level, ModifierInstanceAttributes attributes, float modifier, ItemAffix affix)
{
    if (affix != null && IsSpecialModifier(affix.Tags))
    {
        // Set Lower to LowerMax for special modifiers
        // Note: Would need to use reflection or create new instance
    }
    return __result;
}
```

**Challenges**:

- `ModifierInstance` fields are private (`lower`, `upper`)
- Would need reflection or create new instance with modified values
- Need to identify the correct tags for Keropok, Orang Bunian, Awakened

### Approach 2: Patch ModifierList.AddAffix()

**Target**: `ModifierList.AddAffix(ItemAffix foundAffix, int level, float multiplier)`

**Pros**:

- Has access to `ItemAffix` object with tags
- Called after `ModifierInstance` creation but before adding to list
- Can modify the instance before it's added

**Implementation Strategy**:

```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(ModifierList), "AddAffix")]
static void Postfix(ModifierList __instance, ItemAffix foundAffix, int level, float multiplier)
{
    if (IsSpecialModifier(foundAffix.Tags))
    {
        // Find and modify the newly created ModifierInstance
        // Set Lower to LowerMax for all modifiers in this affix
    }
}
```

**Challenges**:

- Need to identify which `ModifierInstance` objects were just created
- Multiple modifiers can be created per affix
- Still need to access private fields

### Approach 3: Patch ItemInstance.AddOrReplaceImplicit()

**Target**: `ItemInstance.AddOrReplaceImplicit(GameTag[] tags)`

**Pros**:

- Directly targets the method used for special modifier application
- Has access to the tags being applied
- Called specifically for Awakened and Keropok items

**Implementation Strategy**:

```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(ItemInstance), "AddOrReplaceImplicit")]
static void Postfix(ItemInstance __instance, GameTag[] tags)
{
    if (IsSpecialModifierTags(tags))
    {
        // Find ModifierInstances with these tags and set Lower to LowerMax
        // Use reflection or custom logic to modify values
    }
}
```

**Challenges**:

- Only covers implicit modifiers, not all special modifiers
- Need to identify which modifiers were just added
- May not cover all special modifier creation scenarios

### Approach 4: Reflection-Based Field Modification

**Target**: Any of the above approaches combined with reflection

**Implementation Strategy**:

```csharp
private static void SetModifierInstanceToMax(ModifierInstance instance)
{
    var lowerField = typeof(ModifierInstance).GetField("lower", BindingFlags.NonPublic | BindingFlags.Instance);
    var upperField = typeof(ModifierInstance).GetField("upper", BindingFlags.NonPublic | BindingFlags.Instance);

    // Calculate max values based on modifier properties
    float maxLower = CalculateMaxLower(instance.Modifier);
    float maxUpper = CalculateMaxUpper(instance.Modifier);

    lowerField.SetValue(instance, maxLower);
    upperField.SetValue(instance, maxUpper);
}
```

**Pros**:

- Direct access to private fields
- Can modify existing instances
- Works with any creation approach

**Challenges**:

- Uses reflection (performance impact)
- Fragile if internal structure changes
- Need to calculate max values correctly

## Recommended Implementation Plan

### Phase 1: Research and Setup

1. **Identify Exact Tags**: Determine the specific `GameTag` values for:

   - Keropok modifiers
   - Orang Bunian modifiers
   - Awakened modifiers

2. **Test Tag Detection**: Create helper methods to identify special modifiers:

   ```csharp
   private static bool IsSpecialModifier(GameTag[] tags)
   {
       if (tags == null) return false;

       return tags.Any(tag =>
           tag.Name.Contains("Keropok") ||
           tag.Name.Contains("Orang") ||
           tag.Name.Contains("Awakened"));
   }
   ```

### Phase 2: Core Implementation

1. **Implement Approach 1** (Patch `Modifier.CreateInstance()`)
2. **Use reflection** to modify private fields
3. **Add comprehensive logging** for debugging

### Phase 3: Testing and Refinement

1. **Test with each special modifier type**
2. **Verify values are set correctly**
3. **Ensure no side effects on normal modifiers**

## Technical Considerations

### Value Calculation

- `LowerMax` may need to account for level scaling (`LowerMax + LowerPerLevel * level`)
- Consider `ModifierInstanceAttributes` and multiplier effects
- Ensure compatibility with existing game balance

### Performance

- Reflection has minimal impact for modifier creation (infrequent operation)
- Consider caching reflection info if performance becomes an issue

### Compatibility

- Ensure mod works with existing save games
- Test with different item levels and modifier types
- Verify compatibility with other mods

## Next Steps

1. Research the exact `GameTag` names/IDs for special modifiers
2. Implement basic patch structure with logging
3. Test with one modifier type first (e.g., Awakened)
4. Expand to cover all special modifier types
5. Add configuration options for enabling/disabling per modifier type

## Conclusion

The most robust approach is **Approach 1** (patching `Modifier.CreateInstance()`) combined with **Approach 4** (reflection-based field modification). This provides:

- Centralized control over all modifier creation
- Ability to identify special modifiers via tags
- Direct modification of private fields
- Coverage of all special modifier scenarios

The key challenges are:

1. Identifying the correct `GameTag` values for each special modifier type
2. Implementing proper reflection-based field modification
3. Calculating the correct max values accounting for level scaling and attributes

This approach should successfully set `ModifierInstance.Lower` to `ModifierInstance.Modifier.LowerMax` for all special modifiers while preserving normal modifier behavior.

## Additional Approach: Keropok Buff System Modification

### Overview

The Keropok buff system allows food items to gain special modifiers by killing cursed monsters (Hunters). Currently, the system randomly adds either a normal affix or a Keropok-tagged implicit affix each time a Hunter is killed. **The cursed food item will always ultimately get a Keropok implicit**, but can end up with 1-6 other modifiers before that happens. This approach modifies the system to ensure that exactly 6 other modifiers are added before the Keropok implicit is applied.

### Current Keropok System Analysis

#### How It Works:

1. **Item Cursing**: Items get Keropok curse tags via `AwakenedItemManager.KeropokItem()`
2. **Hunter Killing**: When a Hunter is killed, `IncrementKeropokKillCount()` is called
3. **Affix Addition**: `ModifierList.FixKeropokModifier()` is called to add a new affix
4. **Chance Calculation**: Base chance is `0.4f + (numKilled * 0.1f)` (40% + 10% per kill)
5. **Affix Selection**: Either adds a normal affix (Prefix/Suffix) OR a Keropok-tagged implicit
6. **Completion**: After 5 kills OR chance roll succeeds, item gets Keropok tags and curse is removed
7. **Guaranteed Outcome**: The item will always end up with a Keropok implicit, but may have 1-6 other modifiers first

#### Key Methods:

- `AwakenedItemManager.IncrementKeropokKillCount()` - Main entry point
- `ModifierList.FixKeropokModifier()` - Handles affix addition logic
- `ItemManager.GetAffix()` - Adds the actual affix to the item

### Proposed Modification Approaches

#### Approach A: Patch ModifierList.FixKeropokModifier() (Recommended)

**Target**: `ModifierList.FixKeropokModifier(ItemInstance item, GameTag[] keropokCurseTag, int numKilled)`

**Strategy**:

1. **Check Affix Count**: Determine if item has exactly 6 non-implicit modifiers
2. **Prevent Keropok**: If fewer than 6 non-implicit modifiers, prevent Keropok implicit from being added
3. **Allow Keropok**: If exactly 6 non-implicit modifiers, allow normal system to add Keropok implicit
4. **Let Maximization Handle**: Once Keropok implicit is added, existing maximization logic will maximize it

**Implementation Strategy**:

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(ModifierList), "FixKeropokModifier")]
static bool Prefix(ModifierList __instance, ItemInstance item, GameTag[] keropokCurseTag, int numKilled, ref float __result)
{
    try
    {
        // Check if item has fewer than 6 non-implicit modifiers
        if (HasFewerThanSixNonImplicitModifiers(__instance, item))
        {
            // Prevent Keropok implicit - force normal affix addition
            // This ensures we keep adding normal affixes until we reach 6
            __result = 0.0f; // Return low chance to prevent Keropok completion
            return true; // Let original method run but with modified chance
        }

        // If exactly 6 non-implicit modifiers, let normal system handle Keropok implicit
        return true; // Let original method run normally
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[MaxSpecialModifiers] Error in FixKeropokModifier prefix: {ex.Message}");
        return true; // Let original method run on error
    }
}
```

**Pros**:

- Direct control over Keropok affix addition logic
- Can check affix count before deciding behavior
- Preserves original system once 6 non-implicit modifiers are reached
- Clean separation of concerns
- Lets existing maximization logic handle the Keropok implicit

**Challenges**:

- Need to accurately count non-implicit modifiers
- Must understand the chance calculation system to prevent premature Keropok completion
- Need to ensure normal affix addition continues until 6 non-implicit modifiers

#### Approach B: Patch ItemManager.GetAffix()

**Target**: `ItemManager.GetAffix(ModifierList mods, int level, ItemModifierAttributes modAttributes, GameTag[] tags, float multiplier, ItemAffix bannedAffix)`

**Strategy**:

1. **Detect Keropok Context**: Check if this GetAffix call is from Keropok system
2. **Check Affix Limit**: Determine if item is at maximum affixes
3. **Redirect to Keropok**: Force Keropok-tagged affix instead of normal affix

**Implementation Strategy**:

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(ItemManager), "GetAffix")]
static bool Prefix(ItemManager __instance, ModifierList mods, int level, ItemModifierAttributes modAttributes, GameTag[] tags, float multiplier, ItemAffix bannedAffix)
{
    try
    {
        // Check if this is a Keropok context and item is at max affixes
        if (IsKeropokContext(mods) && HasMaxNormalAffixes(mods))
        {
            // Force Keropok-tagged implicit affix
            ForceKeropokImplicitAffix(__instance, mods, level, tags);
            return false; // Skip original method
        }

        return true; // Let original method run
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[MaxSpecialModifiers] Error in GetAffix prefix: {ex.Message}");
        return true;
    }
}
```

**Pros**:

- Intercepts at the affix selection level
- Can modify affix type before creation
- Works for all Keropok affix additions

**Challenges**:

- Harder to detect Keropok context
- May interfere with other affix additions
- More complex logic to avoid false positives

#### Approach C: Patch AwakenedItemManager.IncrementKeropokKillCount()

**Target**: `AwakenedItemManager.IncrementKeropokKillCount(KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature)`

**Strategy**:

1. **Check Affix Count**: Before calling FixKeropokModifier, check if at max
2. **Modify Behavior**: If at max, force Keropok completion instead of normal affix addition

**Implementation Strategy**:

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(AwakenedItemManager), "IncrementKeropokKillCount")]
static bool Prefix(AwakenedItemManager __instance, KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, ref bool __result)
{
    try
    {
        // Check if item has reached maximum normal affixes
        if (HasMaxNormalAffixes(item.Mods, item))
        {
            // Force Keropok completion
            item.AddOrReplaceImplicit(__instance.KeropokTags);
            __instance.awakenedItems.Remove(item.InstanceID);
            __result = true;
            return false; // Skip original method
        }

        return true; // Let original method run
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[MaxSpecialModifiers] Error in IncrementKeropokKillCount prefix: {ex.Message}");
        return true;
    }
}
```

**Pros**:

- High-level control over Keropok process
- Can bypass normal affix addition entirely
- Direct access to Keropok completion logic

**Challenges**:

- Need access to private fields (`awakenedItems`, `KeropokTags`)
- May interfere with normal Keropok progression
- Requires understanding of KillQuestItemProgress lifecycle

### Helper Methods Needed

#### Determine Non-Implicit Modifier Count

```csharp
private static bool HasFewerThanSixNonImplicitModifiers(ModifierList mods, ItemInstance item)
{
    // Count current modifiers that are NOT implicit (excluding Keropok-tagged affixes)
    int currentNonImplicitModifiers = mods.Mods.Count(m =>
        m.Affix != null &&
        m.Affix.Affix != null &&
        (m.Affix.Affix.Attributes & ItemModifierAttributes.Implicit) == 0 &&
        !IsKeropokAffix(m.Affix.Affix));

    return currentNonImplicitModifiers < 6;
}

private static bool IsKeropokAffix(ItemAffix affix)
{
    return affix.Tags != null &&
           affix.Tags.Any(tag => tag.GameTagName.Contains("Keropok"));
}
```

#### Understanding the Chance System

The key insight is that `FixKeropokModifier()` returns a chance value that determines whether the Keropok process completes. By returning `0.0f` when we have fewer than 6 non-implicit modifiers, we prevent the Keropok implicit from being added prematurely, forcing the system to continue adding normal affixes.

```csharp
// Original chance calculation in FixKeropokModifier:
// float num = 0.4f + (float)numKilled * 0.1f;
// This gets reduced by 0.5f if a Keropok curse tag is found and removed

// Our modification:
// If fewer than 6 non-implicit modifiers: return 0.0f to prevent completion
// If exactly 6 non-implicit modifiers: let original logic run normally
```

### Recommended Implementation Plan

#### Phase 1: Research and Setup

1. **Determine Max Affix Logic**: Analyze how the game calculates maximum affixes per rarity
2. **Identify Keropok Affixes**: Find all Keropok-tagged implicit affixes in the game
3. **Test Current Behavior**: Understand the exact flow of Keropok affix addition

#### Phase 2: Core Implementation

1. **Implement Approach A** (Patch `FixKeropokModifier`)
2. **Add helper methods** for affix counting and Keropok forcing
3. **Add comprehensive logging** for debugging

#### Phase 3: Testing and Refinement

1. **Test with different item rarities** to ensure max affix logic is correct
2. **Verify Keropok affixes are added** only after max normal affixes
3. **Ensure normal Keropok progression** still works for non-max items

### Technical Considerations

#### Affix Count Logic

- Need to understand how `ItemRarity.RequiredMods` determines maximum affixes
- Must distinguish between normal affixes and implicit affixes
- Should handle edge cases where items have mixed affix types

#### Keropok Affix Selection

- Need to identify all Keropok-tagged implicit affixes
- Should maintain randomness in Keropok affix selection
- Must preserve affix level scaling and other properties

#### Compatibility

- Ensure modification doesn't break existing Keropok system
- Test with different item types and rarities
- Verify compatibility with other mods that might affect affixes

### Expected Behavior After Implementation

1. **Normal Progression**: Items with fewer than 6 non-implicit modifiers continue to get normal affixes when Hunters are killed
2. **Prevention Logic**: Keropok implicit addition is prevented until exactly 6 non-implicit modifiers are present
3. **Keropok Addition**: Once 6 non-implicit modifiers are reached, normal system adds Keropok implicit
4. **Guaranteed Outcome**: All cursed food items will end up with exactly 6 non-implicit modifiers + 1 Keropok implicit
5. **Maximization**: Keropok implicit will be maximized by the existing ModifierInstance maximization logic
6. **Completion**: Keropok process completes normally after Keropok implicit is added
