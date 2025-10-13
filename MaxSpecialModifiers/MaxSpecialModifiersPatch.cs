using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Patch for ItemInstance.AddOrReplaceImplicit to maximize special modifiers
	/// </summary>
	[HarmonyPatch(typeof(ItemInstance))]
	public class ItemInstancePatch
	{
		// Cache reflection fields for performance
		private static readonly FieldInfo lowerField = typeof(ModifierInstance).GetField("lower", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo upperField = typeof(ModifierInstance).GetField("upper", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void DebugLog(string message)
		{
			if (ModLoader.Config?.DebugLogging == true)
			{
				Debug.Log($"[MaxSpecialModifiers] {message}");
			}
		}

		/// <summary>
		/// Prefix to intercept AddOrReplaceImplicit and handle forced affixes
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("AddOrReplaceImplicit")]
		static bool Prefix(ItemInstance __instance, GameTag[] tags)
		{
			try
			{
				DebugLog($"[MaxSpecialModifiers] AddOrReplaceImplicit prefix called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");
				DebugLog($"[MaxSpecialModifiers] Item modifiers at start of prefix: {__instance.Mods?.Mods?.Count ?? 0}");

				// Log existing affixes
				if (__instance.Mods?.Mods != null)
				{
					foreach (var mod in __instance.Mods.Mods)
					{
						if (mod?.Affix?.Affix != null)
						{
							DebugLog($"[MaxSpecialModifiers] Existing affix: {mod.Affix.Affix.ItemAffixName}");
						}
					}
				}

				// Check if this is an item with configured forced affixes
				if (tags != null)
				{
					DebugLog($"[MaxSpecialModifiers] Config is null: {ModLoader.Config == null}");
					if (ModLoader.Config != null)
					{
						DebugLog($"[MaxSpecialModifiers] Available config keys: {string.Join(", ", ModLoader.Config.TagConfigurations.Keys)}");
					}

					// Check if any tag matches our configuration
					bool hasConfiguredTag = false;
					foreach (var tag in tags)
					{
						if (tag?.GameTagName == null) continue;
						DebugLog($"[MaxSpecialModifiers] Checking tag: '{tag.GameTagName}'");

						// Simple exact match check
						if (ModLoader.Config?.TagConfigurations?.ContainsKey(tag.GameTagName) == true)
						{
							DebugLog($"[MaxSpecialModifiers] Found exact match! Tag: '{tag.GameTagName}'");
							hasConfiguredTag = true;
						}
						if (hasConfiguredTag) break;
					}

					if (hasConfiguredTag)
					{
						DebugLog($"[MaxSpecialModifiers] Intercepting configured item, preventing original logic");
						DebugLog($"[MaxSpecialModifiers] Item modifiers before forced affix: {__instance.Mods?.Mods?.Count ?? 0}");

						// Handle the forced affix addition ourselves
						ForceImplicitAffixes(__instance, tags);

						DebugLog($"[MaxSpecialModifiers] Item modifiers after forced affix: {__instance.Mods?.Mods?.Count ?? 0}");

						// Return false to prevent the original method from running
						return false;
					}
				}

				// For all other cases, let the original method run
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in AddOrReplaceImplicit prefix: {ex.Message}");
				return true; // Let original method run on error
			}
		}

		/// <summary>
		/// Postfix to maximize special modifiers after they are added
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch("AddOrReplaceImplicit")]
		static void Postfix(ItemInstance __instance, GameTag[] tags)
		{
			try
			{
				DebugLog($"[MaxSpecialModifiers] AddOrReplaceImplicit postfix called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");
				DebugLog($"[MaxSpecialModifiers] Item modifiers in postfix: {__instance.Mods?.Mods?.Count ?? 0}");

				if (!IsSpecialModifierTags(tags))
				{
					return;
				}

				DebugLog($"[MaxSpecialModifiers] Processing special modifiers for tags: {string.Join(", ", tags.Select(t => t.GameTagName))}");

				// For configured items, the prefix already handled the forced affix addition
				// For other special items, handle them here if they don't have configuration
				// Force implicit affixes if applicable (for special items without config)
				ForceImplicitAffixes(__instance, tags);

				// Always maximize implicit modifiers after any affix additions
				MaximizeImplicitModifiers(__instance);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in AddOrReplaceImplicit postfix: {ex.Message}");
			}
		}

		/// <summary>
		/// Checks if the tags indicate special modifiers (Keropok, Orang Bunian, Awakened)
		/// </summary>
		private static bool IsSpecialModifierTags(GameTag[] tags)
		{
			if (tags == null || tags.Length == 0)
			{
				return false;
			}

			return tags.Any(tag =>
				tag?.GameTagName?.Contains("Keropok") == true ||
				tag?.GameTagName?.Contains("Orang Bunian") == true ||
				tag?.GameTagName?.Contains("Awakened") == true);
		}

		/// <summary>
		/// Maximizes all implicit modifiers on the item
		/// </summary>
		private static void MaximizeImplicitModifiers(ItemInstance item)
		{
			if (item?.Mods?.Mods == null)
			{
				DebugLog($"[MaxSpecialModifiers] No modifiers found on item");
				return;
			}

			DebugLog($"[MaxSpecialModifiers] Checking {item.Mods.Mods.Count} modifiers on item");

			int maximizedCount = 0;
			foreach (var modifierInstance in item.Mods.Mods)
			{
				if (modifierInstance?.Affix?.Affix == null)
				{
					DebugLog($"[MaxSpecialModifiers] Modifier has no affix tags");
					continue;
				}

				var affix = modifierInstance.Affix.Affix;

				// Only process implicit modifiers
				if ((affix.Attributes & ItemModifierAttributes.Implicit) == 0)
				{
					continue;
				}

				// Check if this is a special modifier
				if (!IsSpecialModifierTags(affix.Tags))
				{
					continue;
				}

				DebugLog($"[MaxSpecialModifiers] Checking modifier: {affix.ItemAffixName}, IsImplicit: {((affix.Attributes & ItemModifierAttributes.Implicit) != 0)}, Tags: [{string.Join(", ", affix.Tags?.Select(t => t?.GameTagName) ?? new string[0])}]");

				SetModifierInstanceToMax(modifierInstance, item);
				maximizedCount++;
			}

			DebugLog($"[MaxSpecialModifiers] Maximized {maximizedCount} special implicit modifiers");
		}

		/// <summary>
		/// Sets a ModifierInstance to its maximum values
		/// </summary>
		private static void SetModifierInstanceToMax(ModifierInstance modifierInstance, ItemInstance item)
		{
			try
			{
				var modifier = modifierInstance.Modifier;
				var affix = modifierInstance.Affix.Affix;

				// Get current values
				float currentLower = (float)lowerField.GetValue(modifierInstance);
				float currentUpper = (float)upperField.GetValue(modifierInstance);

				DebugLog($"[MaxSpecialModifiers] Original values - Lower: {currentLower:F3}, Upper: {currentUpper:F3}");
				DebugLog($"[MaxSpecialModifiers] Modifier properties - LowerMin: {modifier.LowerMin:F3}, LowerMax: {modifier.LowerMax:F3}, LowerPerLevel: {modifier.LowerPerLevel:F3}");

				// Calculate max values
				float maxLower = CalculateMaxLower(modifier, affix, item);
				float maxUpper = CalculateMaxUpper(modifier, affix, item);

				DebugLog($"[MaxSpecialModifiers] Calculated max values - Lower: {maxLower:F3}, Upper: {maxUpper:F3}");

				// Set the new values using reflection
				lowerField.SetValue(modifierInstance, maxLower);
				upperField.SetValue(modifierInstance, maxUpper);

				// Verify the values were set
				float newLower = (float)lowerField.GetValue(modifierInstance);
				float newUpper = (float)upperField.GetValue(modifierInstance);
				DebugLog($"[MaxSpecialModifiers] After setting - Lower: {newLower:F3}, Upper: {newUpper:F3}");

				DebugLog($"[MaxSpecialModifiers] Maximized modifier: {affix.ItemAffixName} from {currentLower:F2} to max value");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error maximizing modifier: {ex.Message}");
			}
		}

		/// <summary>
		/// Calculates the maximum lower value for a modifier
		/// </summary>
		private static float CalculateMaxLower(Modifier modifier, ItemAffix affix, ItemInstance item)
		{
			float baseValue = modifier.LowerMax;
			float levelScaling = modifier.LowerPerLevel * item.Level;
			float totalValue = baseValue + levelScaling;

			DebugLog($"[MaxSpecialModifiers] CalculateMaxLower - Base: {baseValue:F3}, LowerPerLevel: {modifier.LowerPerLevel:F3}, ItemLevel: {item.Level}, Scaling: {levelScaling:F3}");

			// Note: ItemAffix doesn't have a Multiplier property in this version

			return totalValue;
		}

		/// <summary>
		/// Calculates the maximum upper value for a modifier
		/// </summary>
		private static float CalculateMaxUpper(Modifier modifier, ItemAffix affix, ItemInstance item)
		{
			float baseValue = modifier.UpperMax;
			float levelScaling = modifier.UpperPerLevel * item.Level;
			float totalValue = baseValue + levelScaling;

			DebugLog($"[MaxSpecialModifiers] CalculateMaxUpper - Base: {baseValue:F3}, UpperPerLevel: {modifier.UpperPerLevel:F3}, ItemLevel: {item.Level}, Scaling: {levelScaling:F3}");

			// Note: ItemAffix doesn't have a Multiplier property in this version

			return totalValue;
		}

		/// <summary>
		/// Forces implicit affixes based on configuration for the given tags
		/// </summary>
		private static void ForceImplicitAffixes(ItemInstance item, GameTag[] tags)
		{
			try
			{

				// Find matching tag configuration
				Dictionary<string, Dictionary<string, bool>> tagConfig = null;
				string matchedTagName = null;

				foreach (var tag in tags)
				{
					if (tag?.GameTagName == null) continue;

					// Check for exact tag name match first
					if (ModLoader.Config.TagConfigurations.TryGetValue(tag.GameTagName, out tagConfig))
					{
						matchedTagName = tag.GameTagName;
						break;
					}

					// Check for partial match (e.g., "Keropok" matches "Keropok Food")
					foreach (var configEntry in ModLoader.Config.TagConfigurations)
					{
						if (tag.GameTagName.Contains(configEntry.Key))
						{
							tagConfig = configEntry.Value;
							matchedTagName = configEntry.Key;
							break;
						}
					}

					if (tagConfig != null) break;
				}

				if (tagConfig == null || !tagConfig.TryGetValue("ForcedAffixes", out var forcedAffixes))
				{
					DebugLog($"[MaxSpecialModifiers] No enabled configuration found for tags: {string.Join(", ", tags.Select(t => t?.GameTagName))}");
					return;
				}

				DebugLog($"[MaxSpecialModifiers] Forcing affixes for {matchedTagName} item: {item.Item?.ItemName}");

				// Get all affixes from ItemManager
				var itemManager = Singleton<ItemManager>.instance;
				if (itemManager == null)
				{
					Debug.LogError($"[MaxSpecialModifiers] ItemManager instance is null");
					return;
				}

				// Access the affixes pool through reflection since it's private
				var affixesPoolField = typeof(ItemManager).GetField("affixesPool", BindingFlags.NonPublic | BindingFlags.Instance);
				if (affixesPoolField == null)
				{
					Debug.LogError($"[MaxSpecialModifiers] Could not find affixesPool field in ItemManager");
					return;
				}

				var allAffixes = (ItemAffix[])affixesPoolField.GetValue(itemManager);
				if (allAffixes == null)
				{
					Debug.LogError($"[MaxSpecialModifiers] affixesPool is null");
					return;
				}

				// Process each configured forced affix
				foreach (var affixEntry in forcedAffixes)
				{
					var affixName = affixEntry.Key;
					var affixEnabled = affixEntry.Value;

					if (!affixEnabled)
					{
						continue;
					}

					// Get exact ItemAffixName from static mapping
					var exactAffixName = ModConfig.GetAffixName(affixName);
					if (string.IsNullOrEmpty(exactAffixName))
					{
						DebugLog($"[MaxSpecialModifiers] Could not find mapping for affix: {affixName}");
						continue;
					}

					// Find the affix by exact name
					var targetAffix = FindAffixByName(allAffixes, exactAffixName);
					if (targetAffix == null)
					{
						DebugLog($"[MaxSpecialModifiers] Could not find affix with name: {exactAffixName}");
						continue;
					}

					// Check if the item already has this affix
					bool hasTargetAffix = item.Mods?.Mods?.Any(modifierInstance =>
						modifierInstance?.Affix?.Affix == targetAffix) == true;

					if (hasTargetAffix)
					{
						DebugLog($"[MaxSpecialModifiers] Item already has the target affix {affixName}, skipping");
						continue;
					}

					// Add the affix to the item's modifier list
					DebugLog($"[MaxSpecialModifiers] Adding forced affix: {affixName} ({targetAffix.ItemAffixName})");
					item.Mods.AddAffix(targetAffix, item.Level, 1f);
				}

				// Maximize the newly added affixes
				DebugLog($"[MaxSpecialModifiers] Maximizing forced affixes");
				MaximizeImplicitModifiers(item);

				DebugLog($"[MaxSpecialModifiers] Successfully forced and maximized affixes for {matchedTagName}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in ForceImplicitAffixes: {ex.Message}");
			}
		}

		/// <summary>
		/// Finds an affix by its exact ItemAffixName
		/// This is much more reliable than trying to match by StatID and Multiplicative requirements
		/// </summary>
		private static ItemAffix FindAffixByName(ItemAffix[] allAffixes, string affixName)
		{
			foreach (var affix in allAffixes)
			{
				if (affix?.ItemAffixName == affixName)
				{
					return affix;
				}
			}

			return null;
		}
	}

	/// <summary>
	/// Patch for AwakenedItemManager to control Keropok completion timing
	/// This is the most targeted approach - only affects Keropok progression, zero impact on Awakened items
	/// </summary>
	[HarmonyPatch(typeof(AwakenedItemManager))]
	public class AwakenedItemManagerPatch
	{
		private static void DebugLog(string message)
		{
			if (ModLoader.Config?.DebugLogging == true)
			{
				Debug.Log($"[MaxSpecialModifiers] {message}");
			}
		}

		/// <summary>
		/// Controls Keropok progression with our custom 6-modifier completion logic
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("IncrementKeropokKillCount")]
		static bool Prefix(KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, AwakenedItemManager __instance, ref bool __result)
		{
			try
			{
				DebugLog($"[MaxSpecialModifiers] === IncrementKeropokKillCount CALLED ===");
				DebugLog($"[MaxSpecialModifiers] Item: {item.Item?.ItemName} (ID: {item.InstanceID})");
				DebugLog($"[MaxSpecialModifiers] Creature: {creature?.Creature?.CreatureName}");
				DebugLog($"[MaxSpecialModifiers] Creature State: {creature.State} (value: {(int)creature.State})");
				DebugLog($"[MaxSpecialModifiers] Hunter Flag: {CharacterContainerState.Hunter} (value: {(int)CharacterContainerState.Hunter})");
				DebugLog($"[MaxSpecialModifiers] Is Hunter: {((creature.State & CharacterContainerState.Hunter) != 0)}");
				DebugLog($"[MaxSpecialModifiers] Progress: NumKilled={progress.NumKilled}, QuestType={progress.QuestType}");

				// Check Hunter state (same as original)
				if ((creature.State & CharacterContainerState.Hunter) == 0)
				{
					DebugLog($"[MaxSpecialModifiers] Creature is not a Hunter, returning false");
					__result = false;
					return false; // Don't let original method run
				}

				// Process with our custom logic using KeropokManager
				DebugLog($"[MaxSpecialModifiers] Processing with KeropokManager...");
				__result = KeropokManager.ProcessKeropokKillCount(progress, item, creature, __instance);
				return false; // Don't let original method run
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in IncrementKeropokKillCount prefix: {ex.Message}");
				return true; // Let original method run on error
			}
		}

		/// <summary>
		/// Logs the result after the original method runs
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch("IncrementKeropokKillCount")]
		static void Postfix(KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, AwakenedItemManager __instance, bool __result)
		{
			try
			{
				DebugLog($"[MaxSpecialModifiers] === IncrementKeropokKillCount RESULT ===");
				DebugLog($"[MaxSpecialModifiers] Original method returned: {__result}");
				DebugLog($"[MaxSpecialModifiers] Progress after: NumKilled={progress.NumKilled}");
				DebugLog($"[MaxSpecialModifiers] ================================");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in IncrementKeropokKillCount postfix: {ex.Message}");
			}
		}

		/// <summary>
		/// Logs calls to the higher-level IncrementItemKillCount method
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("IncrementItemKillCount")]
		static bool LogIncrementItemKillCount(ItemInstance item, CharacterContainer creature, AwakenedItemManager __instance, ref bool __result)
		{
			try
			{
				DebugLog($"[MaxSpecialModifiers] *** IncrementItemKillCount CALLED ***");
				DebugLog($"[MaxSpecialModifiers] Item: {item.Item?.ItemName} (ID: {item.InstanceID})");
				DebugLog($"[MaxSpecialModifiers] Creature: {creature?.Creature?.CreatureName}");
				DebugLog($"[MaxSpecialModifiers] Creature State: {creature.State} (value: {(int)creature.State})");

				// Check if item has awakened progress
				var progress = __instance.GetAwakenedProgress(item);
				if (progress != null)
				{
					DebugLog($"[MaxSpecialModifiers] Item has awakened progress: QuestType={progress.QuestType}, NumKilled={progress.NumKilled}");
				}
				else
				{
					DebugLog($"[MaxSpecialModifiers] Item has NO awakened progress");
				}

				return true; // Let original method run
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in IncrementItemKillCount prefix: {ex.Message}");
				return true;
			}
		}
	}

}