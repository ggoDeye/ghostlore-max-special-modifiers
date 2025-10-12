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

		/// <summary>
		/// Postfix to maximize special modifiers after they are added
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch("AddOrReplaceImplicit")]
		static void Postfix(ItemInstance __instance, GameTag[] tags)
		{
			try
			{
				Debug.Log($"[MaxSpecialModifiers] AddOrReplaceImplicit called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");

				if (!IsSpecialModifierTags(tags))
				{
					return;
				}

				Debug.Log($"[MaxSpecialModifiers] Processing special modifiers for tags: {string.Join(", ", tags.Select(t => t.GameTagName))}");
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
				Debug.Log($"[MaxSpecialModifiers] No modifiers found on item");
				return;
			}

			Debug.Log($"[MaxSpecialModifiers] Checking {item.Mods.Mods.Count} modifiers on item");

			int maximizedCount = 0;
			foreach (var modifierInstance in item.Mods.Mods)
			{
				if (modifierInstance?.Affix?.Affix == null)
				{
					Debug.Log($"[MaxSpecialModifiers] Modifier has no affix tags");
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

				Debug.Log($"[MaxSpecialModifiers] Checking modifier: {affix.ItemAffixName}, IsImplicit: {((affix.Attributes & ItemModifierAttributes.Implicit) != 0)}, Tags: [{string.Join(", ", affix.Tags?.Select(t => t?.GameTagName) ?? new string[0])}]");

				SetModifierInstanceToMax(modifierInstance, item);
				maximizedCount++;
			}

			Debug.Log($"[MaxSpecialModifiers] Maximized {maximizedCount} special implicit modifiers");
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

				Debug.Log($"[MaxSpecialModifiers] Original values - Lower: {currentLower:F3}, Upper: {currentUpper:F3}");
				Debug.Log($"[MaxSpecialModifiers] Modifier properties - LowerMin: {modifier.LowerMin:F3}, LowerMax: {modifier.LowerMax:F3}, LowerPerLevel: {modifier.LowerPerLevel:F3}");

				// Calculate max values
				float maxLower = CalculateMaxLower(modifier, affix, item);
				float maxUpper = CalculateMaxUpper(modifier, affix, item);

				Debug.Log($"[MaxSpecialModifiers] Calculated max values - Lower: {maxLower:F3}, Upper: {maxUpper:F3}");

				// Set the new values using reflection
				lowerField.SetValue(modifierInstance, maxLower);
				upperField.SetValue(modifierInstance, maxUpper);

				// Verify the values were set
				float newLower = (float)lowerField.GetValue(modifierInstance);
				float newUpper = (float)upperField.GetValue(modifierInstance);
				Debug.Log($"[MaxSpecialModifiers] After setting - Lower: {newLower:F3}, Upper: {newUpper:F3}");

				Debug.Log($"[MaxSpecialModifiers] Maximized modifier: {affix.ItemAffixName} from {currentLower:F2} to max value");
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

			Debug.Log($"[MaxSpecialModifiers] CalculateMaxLower - Base: {baseValue:F3}, LowerPerLevel: {modifier.LowerPerLevel:F3}, ItemLevel: {item.Level}, Scaling: {levelScaling:F3}");

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

			Debug.Log($"[MaxSpecialModifiers] CalculateMaxUpper - Base: {baseValue:F3}, UpperPerLevel: {modifier.UpperPerLevel:F3}, ItemLevel: {item.Level}, Scaling: {levelScaling:F3}");

			// Note: ItemAffix doesn't have a Multiplier property in this version

			return totalValue;
		}
	}

	/// <summary>
	/// Patch for AwakenedItemManager to control Keropok completion timing
	/// This is the most targeted approach - only affects Keropok progression, zero impact on Awakened items
	/// </summary>
	[HarmonyPatch(typeof(AwakenedItemManager))]
	public class AwakenedItemManagerPatch
	{
		/// <summary>
		/// Controls Keropok progression with our custom 6-modifier completion logic
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("IncrementKeropokKillCount")]
		static bool Prefix(KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, AwakenedItemManager __instance, ref bool __result)
		{
			try
			{
				Debug.Log($"[MaxSpecialModifiers] === IncrementKeropokKillCount CALLED ===");
				Debug.Log($"[MaxSpecialModifiers] Item: {item.Item?.ItemName} (ID: {item.InstanceID})");
				Debug.Log($"[MaxSpecialModifiers] Creature: {creature?.Creature?.CreatureName}");
				Debug.Log($"[MaxSpecialModifiers] Creature State: {creature.State} (value: {(int)creature.State})");
				Debug.Log($"[MaxSpecialModifiers] Hunter Flag: {CharacterContainerState.Hunter} (value: {(int)CharacterContainerState.Hunter})");
				Debug.Log($"[MaxSpecialModifiers] Is Hunter: {((creature.State & CharacterContainerState.Hunter) != 0)}");
				Debug.Log($"[MaxSpecialModifiers] Progress: NumKilled={progress.NumKilled}, QuestType={progress.QuestType}");

				// Check Hunter state (same as original)
				if ((creature.State & CharacterContainerState.Hunter) == 0)
				{
					Debug.Log($"[MaxSpecialModifiers] Creature is not a Hunter, returning false");
					__result = false;
					return false; // Don't let original method run
				}

				// Process with our custom logic using KeropokManager
				Debug.Log($"[MaxSpecialModifiers] Processing with KeropokManager...");
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
				Debug.Log($"[MaxSpecialModifiers] === IncrementKeropokKillCount RESULT ===");
				Debug.Log($"[MaxSpecialModifiers] Original method returned: {__result}");
				Debug.Log($"[MaxSpecialModifiers] Progress after: NumKilled={progress.NumKilled}");
				Debug.Log($"[MaxSpecialModifiers] ================================");
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
				Debug.Log($"[MaxSpecialModifiers] *** IncrementItemKillCount CALLED ***");
				Debug.Log($"[MaxSpecialModifiers] Item: {item.Item?.ItemName} (ID: {item.InstanceID})");
				Debug.Log($"[MaxSpecialModifiers] Creature: {creature?.Creature?.CreatureName}");
				Debug.Log($"[MaxSpecialModifiers] Creature State: {creature.State} (value: {(int)creature.State})");

				// Check if item has awakened progress
				var progress = __instance.GetAwakenedProgress(item);
				if (progress != null)
				{
					Debug.Log($"[MaxSpecialModifiers] Item has awakened progress: QuestType={progress.QuestType}, NumKilled={progress.NumKilled}");
				}
				else
				{
					Debug.Log($"[MaxSpecialModifiers] Item has NO awakened progress");
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