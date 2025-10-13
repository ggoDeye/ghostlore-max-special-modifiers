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
				// Check if configuration is valid - if not, use original game behavior
				if (ModLoader.Config == null || !ModLoader.Config.IsConfigurationValid)
				{
					DebugLog("Configuration is invalid or null, using original game behavior");
					return true; // Let original method run
				}

				DebugLog($"[MaxSpecialModifiers] AddOrReplaceImplicit prefix called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");
				DebugLog($"[MaxSpecialModifiers] Item modifiers at start of prefix: {__instance.Mods?.Mods?.Count ?? 0}");
				DebugLog($"[MaxSpecialModifiers] Item ID: {__instance.InstanceID}, Item Name: {__instance.Item?.ItemName}");

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

					// Check if any tag matches our configuration
					string matchedTagName = null;
					bool hasConfiguredTag = false;
					Dictionary<string, bool> forcedAffixes = null;

					foreach (var tag in tags)
					{
						if (tag?.GameTagName == null) continue;
						DebugLog($"[MaxSpecialModifiers] Checking tag: '{tag.GameTagName}'");

						// Check for Keropok
						if (tag.GameTagName.Contains("Keropok") && ModLoader.Config?.Keropok != null)
						{
							DebugLog($"[MaxSpecialModifiers] Found Keropok match! Tag: '{tag.GameTagName}'");
							matchedTagName = "Keropok";
							forcedAffixes = ModLoader.Config.Keropok;
							hasConfiguredTag = true;
							break;
						}
						// Check for Orang Bunian
						else if (tag.GameTagName.Contains("Orang Bunian") && ModLoader.Config?.OrangBunian != null)
						{
							DebugLog($"[MaxSpecialModifiers] Found Orang Bunian match! Tag: '{tag.GameTagName}'");
							matchedTagName = "Orang Bunian";
							forcedAffixes = ModLoader.Config.OrangBunian;
							hasConfiguredTag = true;
							break;
						}
						// Check for Awakened
						else if (tag.GameTagName.Contains("Awakened") && ModLoader.Config?.Awakened != null)
						{
							DebugLog($"[MaxSpecialModifiers] Found Awakened match! Tag: '{tag.GameTagName}'");
							matchedTagName = "Awakened";
							forcedAffixes = ModLoader.Config.Awakened;
							hasConfiguredTag = true;
							break;
						}
					}

					if (hasConfiguredTag && !string.IsNullOrEmpty(matchedTagName) && forcedAffixes != null)
					{
						var enabledAffixes = forcedAffixes.Where(kvp => kvp.Value).ToList();
						if (enabledAffixes.Count > 0)
						{
							DebugLog($"[MaxSpecialModifiers] Intercepting configured item, preventing original logic");
							DebugLog($"[MaxSpecialModifiers] Item modifiers before forced affix: {__instance.Mods?.Mods?.Count ?? 0}");

							// Handle the forced affix addition ourselves
							bool affixAdded = ForceImplicitAffixes(__instance, tags);

							DebugLog($"[MaxSpecialModifiers] Item modifiers after forced affix: {__instance.Mods?.Mods?.Count ?? 0}");
							DebugLog($"[MaxSpecialModifiers] ForceImplicitAffixes returned: {affixAdded}");

							// Return false to prevent the original method from running
							DebugLog($"[MaxSpecialModifiers] Returning false to prevent original method");
							return false;
						}
						else
						{
							DebugLog($"[MaxSpecialModifiers] No enabled affixes for {matchedTagName}, letting original method run");
						}
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
				// Check if configuration is valid - if not, skip maximization
				if (ModLoader.Config == null || !ModLoader.Config.IsConfigurationValid)
				{
					DebugLog("Configuration is invalid or null, skipping maximization");
					return;
				}

				DebugLog($"[MaxSpecialModifiers] AddOrReplaceImplicit postfix called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");
				DebugLog($"[MaxSpecialModifiers] Item modifiers in postfix: {__instance.Mods?.Mods?.Count ?? 0}");

				if (!IsSpecialModifierTags(tags))
				{
					return;
				}

				DebugLog($"[MaxSpecialModifiers] Processing special modifiers for tags: {string.Join(", ", tags.Select(t => t.GameTagName))}");

				// Remove any existing implicit affixes with relevant tags
				RemoveExistingImplicitAffixes(__instance, tags);

				// Add our configured implicit affix
				bool affixAdded = ForceImplicitAffixes(__instance, tags);

				// Maximize it
				if (affixAdded)
				{
					DebugLog($"[MaxSpecialModifiers] Affix was added, maximizing implicit modifiers in postfix");
					MaximizeImplicitModifiers(__instance);
				}
				else
				{
					DebugLog($"[MaxSpecialModifiers] No affix was added, maximizing existing implicit modifiers in postfix");
					MaximizeImplicitModifiers(__instance);
				}
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
		/// Returns true if an affix was successfully added, false otherwise
		/// </summary>
		private static bool ForceImplicitAffixes(ItemInstance item, GameTag[] tags)
		{
			try
			{
				// Find matching tag configuration
				Dictionary<string, bool> forcedAffixes = null;
				string matchedTagName = null;

				foreach (var tag in tags)
				{
					if (tag?.GameTagName == null) continue;

					// Check for Keropok
					if (tag.GameTagName.Contains("Keropok") && ModLoader.Config?.Keropok != null)
					{
						forcedAffixes = ModLoader.Config.Keropok;
						matchedTagName = "Keropok";
						break;
					}
					// Check for Orang Bunian
					else if (tag.GameTagName.Contains("Orang Bunian") && ModLoader.Config?.OrangBunian != null)
					{
						forcedAffixes = ModLoader.Config.OrangBunian;
						matchedTagName = "Orang Bunian";
						break;
					}
					// Check for Awakened
					else if (tag.GameTagName.Contains("Awakened") && ModLoader.Config?.Awakened != null)
					{
						forcedAffixes = ModLoader.Config.Awakened;
						matchedTagName = "Awakened";
						break;
					}
				}

				if (forcedAffixes == null)
				{
					DebugLog($"[MaxSpecialModifiers] No enabled configuration found for tags: {string.Join(", ", tags.Select(t => t?.GameTagName))}");
					return false;
				}

				DebugLog($"[MaxSpecialModifiers] Forcing affixes for {matchedTagName} item: {item.Item?.ItemName}");

				// Get all affixes from ItemManager
				var itemManager = Singleton<ItemManager>.instance;
				if (itemManager == null)
				{
					DebugLog("ItemManager instance is null");
					return false;
				}

				// Access the affixes pool through reflection since it's private
				var affixesPoolField = typeof(ItemManager).GetField("affixesPool", BindingFlags.NonPublic | BindingFlags.Instance);
				if (affixesPoolField == null)
				{
					DebugLog("Could not find affixesPool field in ItemManager");
					return false;
				}

				var allAffixes = (ItemAffix[])affixesPoolField.GetValue(itemManager);
				if (allAffixes == null)
				{
					DebugLog("affixesPool is null");
					return false;
				}

				// Get list of enabled affixes
				var enabledAffixes = forcedAffixes.Where(kvp => kvp.Value).ToList();
				if (enabledAffixes.Count == 0)
				{
					DebugLog($"[MaxSpecialModifiers] No enabled affixes found for {matchedTagName}");
					return false;
				}

				// Randomly select one enabled affix
				var random = new System.Random();
				var selectedAffix = enabledAffixes[random.Next(enabledAffixes.Count)];
				var affixName = selectedAffix.Key;

				DebugLog($"[MaxSpecialModifiers] Randomly selected affix: {affixName} from {enabledAffixes.Count} enabled affixes");

				// Get exact ItemAffixName from static mapping
				var exactAffixName = ModConfig.GetAffixName(affixName);
				if (string.IsNullOrEmpty(exactAffixName))
				{
					DebugLog($"[MaxSpecialModifiers] Could not find mapping for affix: {affixName}");
					return false;
				}

				// Find the affix by exact name
				var targetAffix = FindAffixByName(allAffixes, exactAffixName);
				if (targetAffix == null)
				{
					DebugLog($"[MaxSpecialModifiers] Could not find affix with name: {exactAffixName}");
					return false;
				}

				// Check if the item already has this affix
				bool hasTargetAffix = item.Mods?.Mods?.Any(modifierInstance =>
					modifierInstance?.Affix?.Affix == targetAffix) == true;

				if (hasTargetAffix)
				{
					DebugLog($"[MaxSpecialModifiers] Item already has the target affix {affixName}, skipping");
					return false;
				}

				// Add the affix to the item's modifier list
				DebugLog($"[MaxSpecialModifiers] Adding forced affix: {affixName} ({targetAffix.ItemAffixName})");
				item.Mods.AddAffix(targetAffix, item.Level, 1f);

				// Maximize the newly added affixes
				DebugLog($"[MaxSpecialModifiers] Maximizing forced affixes");
				MaximizeImplicitModifiers(item);

				DebugLog($"[MaxSpecialModifiers] Successfully forced and maximized affixes for {matchedTagName}");
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in ForceImplicitAffixes: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Removes any existing implicit affixes with the given tags
		/// </summary>
		private static void RemoveExistingImplicitAffixes(ItemInstance item, GameTag[] tags)
		{
			if (item?.Mods?.Mods == null || tags == null)
			{
				return;
			}

			var modifiersToRemove = new List<ModifierInstance>();

			foreach (var modifierInstance in item.Mods.Mods)
			{
				if (modifierInstance?.Affix?.Affix == null)
				{
					continue;
				}

				var affix = modifierInstance.Affix.Affix;

				// Only check implicit modifiers
				if ((affix.Attributes & ItemModifierAttributes.Implicit) == 0)
				{
					continue;
				}

				// Check if this affix has any of the relevant tags
				if (affix.Tags != null)
				{
					foreach (var affixTag in affix.Tags)
					{
						if (affixTag?.GameTagName == null) continue;

						foreach (var itemTag in tags)
						{
							if (itemTag?.GameTagName == null) continue;

							// Check for exact match or if the affix tag contains the item tag
							if (affixTag.GameTagName == itemTag.GameTagName ||
								affixTag.GameTagName.Contains(itemTag.GameTagName))
							{
								DebugLog($"[MaxSpecialModifiers] Removing existing implicit affix: {affix.ItemAffixName} (tag: {affixTag.GameTagName})");
								modifiersToRemove.Add(modifierInstance);
								break;
							}
						}
					}
				}
			}

			// Remove the identified modifiers
			foreach (var modifier in modifiersToRemove)
			{
				item.Mods.Mods.Remove(modifier);
			}

			if (modifiersToRemove.Count > 0)
			{
				DebugLog($"[MaxSpecialModifiers] Removed {modifiersToRemove.Count} existing implicit affixes");
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
				// Check if configuration is valid - if not, use original game behavior
				if (ModLoader.Config == null || !ModLoader.Config.IsConfigurationValid)
				{
					DebugLog("Configuration is invalid or null, using original Keropok progression");
					return true; // Let original method run
				}

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