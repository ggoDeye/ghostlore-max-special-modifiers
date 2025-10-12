using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Patch for ItemInstance.AddOrReplaceImplicit to maximize special modifier values
	/// </summary>
	[HarmonyPatch(typeof(ItemInstance))]
	public class ItemInstancePatch
	{
		// Cache reflection fields for performance
		private static readonly FieldInfo lowerField = typeof(ModifierInstance).GetField("lower", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo upperField = typeof(ModifierInstance).GetField("upper", BindingFlags.NonPublic | BindingFlags.Instance);

		/// <summary>
		/// Postfix patch for AddOrReplaceImplicit - maximizes special modifier values after they're added
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch("AddOrReplaceImplicit")]
		static void Postfix(ItemInstance __instance, GameTag[] tags)
		{
			try
			{
				Debug.Log($"[MaxSpecialModifiers] AddOrReplaceImplicit called with tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");

				// Check if these are special modifier tags
				if (!IsSpecialModifierTags(tags))
				{
					Debug.Log($"[MaxSpecialModifiers] No special modifier tags found, skipping");
					return;
				}

				Debug.Log($"[MaxSpecialModifiers] Processing special modifiers for tags: {string.Join(", ", tags?.Select(t => t?.GameTagName) ?? new string[0])}");

				// Maximize all implicit modifiers with special tags
				MaximizeImplicitModifiers(__instance, tags);

			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in AddOrReplaceImplicit postfix: {ex.Message}");
			}
		}

		/// <summary>
		/// Checks if the provided tags indicate special modifiers (Keropok, Orang Bunian, Awakened)
		/// </summary>
		private static bool IsSpecialModifierTags(GameTag[] tags)
		{
			if (tags == null || tags.Length == 0)
			{
				Debug.Log($"[MaxSpecialModifiers] IsSpecialModifierTags: tags is null or empty");
				return false;
			}

			Debug.Log($"[MaxSpecialModifiers] IsSpecialModifierTags: checking tags: [{string.Join(", ", tags.Select(t => $"'{t?.GameTagName}'"))}]");

			bool isSpecial = tags.Any(tag =>
				tag?.GameTagName?.Contains("Keropok") == true ||
				tag?.GameTagName?.Contains("Orang Bunian") == true ||
				tag?.GameTagName?.Contains("Awakened") == true);

			Debug.Log($"[MaxSpecialModifiers] IsSpecialModifierTags: result = {isSpecial}");
			return isSpecial;
		}

		/// <summary>
		/// Maximizes all implicit modifiers on the item that have special tags
		/// </summary>
		private static void MaximizeImplicitModifiers(ItemInstance item, GameTag[] targetTags)
		{
			if (item?.Mods?.Mods == null)
			{
				Debug.Log($"[MaxSpecialModifiers] No modifiers found on item");
				return;
			}

			Debug.Log($"[MaxSpecialModifiers] Checking {item.Mods.Mods.Count} modifiers on item");
			int modifiedCount = 0;

			// Find all implicit modifiers with the target tags
			foreach (var modifierInstance in item.Mods.Mods)
			{
				if (modifierInstance?.Affix?.Affix?.Tags == null)
				{
					Debug.Log($"[MaxSpecialModifiers] Modifier has no affix tags");
					continue;
				}

				var modifierTags = modifierInstance.Affix.Affix.Tags.Select(t => t?.GameTagName).ToArray();
				Debug.Log($"[MaxSpecialModifiers] Checking modifier: {modifierInstance.Modifier.Stat?.StatDisplayName}, " +
					$"IsImplicit: {modifierInstance.IsImplicit}, Tags: [{string.Join(", ", modifierTags)}]");

				// Check if this modifier has any of the target tags and is implicit
				if (modifierInstance.IsImplicit &&
					modifierInstance.Affix.Affix.Tags.Intersect(targetTags).Any())
				{
					// Maximize this modifier's values
					SetModifierInstanceToMax(modifierInstance, item);
					modifiedCount++;

					Debug.Log($"[MaxSpecialModifiers] Maximized modifier: {modifierInstance.Modifier.Stat?.StatDisplayName} " +
						$"from {modifierInstance.Lower:F2} to max value");
				}
			}

			if (modifiedCount > 0)
			{
				Debug.Log($"[MaxSpecialModifiers] Maximized {modifiedCount} special implicit modifiers");

				// Refresh the fast lookup after modifications
				item.Mods.RefreshFastLookup();
			}
			else
			{
				Debug.Log($"[MaxSpecialModifiers] No modifiers were maximized");
			}
		}

		/// <summary>
		/// Sets a ModifierInstance to its maximum values using reflection
		/// </summary>
		private static void SetModifierInstanceToMax(ModifierInstance instance, ItemInstance item)
		{
			if (instance?.Modifier == null || lowerField == null || upperField == null)
			{
				Debug.Log($"[MaxSpecialModifiers] SetModifierInstanceToMax: invalid instance or fields");
				return;
			}

			try
			{
				var modifier = instance.Modifier;
				float originalLower = instance.Lower;
				float originalUpper = instance.Upper;

				Debug.Log($"[MaxSpecialModifiers] Original values - Lower: {originalLower:F3}, Upper: {originalUpper:F3}");
				Debug.Log($"[MaxSpecialModifiers] Modifier properties - LowerMin: {modifier.LowerMin:F3}, LowerMax: {modifier.LowerMax:F3}, LowerPerLevel: {modifier.LowerPerLevel:F3}");

				// Calculate max values accounting for level scaling and attributes
				float maxLower = CalculateMaxLower(modifier, instance, item);
				float maxUpper = CalculateMaxUpper(modifier, instance, item);

				Debug.Log($"[MaxSpecialModifiers] Calculated max values - Lower: {maxLower:F3}, Upper: {maxUpper:F3}");

				// Set the private fields using reflection
				lowerField.SetValue(instance, maxLower);
				upperField.SetValue(instance, maxUpper);

				// Verify the values were set
				float newLower = instance.Lower;
				float newUpper = instance.Upper;
				Debug.Log($"[MaxSpecialModifiers] After setting - Lower: {newLower:F3}, Upper: {newUpper:F3}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error setting modifier to max: {ex.Message}");
			}
		}

		/// <summary>
		/// Calculates the maximum lower value for a modifier
		/// </summary>
		private static float CalculateMaxLower(Modifier modifier, ModifierInstance instance, ItemInstance item)
		{
			// Base max value
			float maxValue = Mathf.Max(modifier.LowerMin, modifier.LowerMax);

			// Get the actual item level from the item that contains this modifier
			int itemLevel = item?.Level ?? 1;

			// Account for level scaling using the actual item level
			// Handle invalid LowerPerLevel values (NaN, infinity) by defaulting to 0
			float lowerPerLevel = float.IsNaN(modifier.LowerPerLevel) || float.IsInfinity(modifier.LowerPerLevel) ? 0f : modifier.LowerPerLevel;
			maxValue += lowerPerLevel * itemLevel;

			Debug.Log($"[MaxSpecialModifiers] CalculateMaxLower - Base: {Mathf.Max(modifier.LowerMin, modifier.LowerMax):F3}, LowerPerLevel: {lowerPerLevel:F3}, ItemLevel: {itemLevel}, Scaling: {lowerPerLevel * itemLevel:F3}");

			// Apply multiplier from the affix if present
			if (instance.Affix != null)
			{
				maxValue *= instance.Affix.Multiplier;
				Debug.Log($"[MaxSpecialModifiers] Applied affix multiplier: {instance.Affix.Multiplier:F3}, Final: {maxValue:F3}");
			}

			return maxValue;
		}

		/// <summary>
		/// Calculates the maximum upper value for a modifier
		/// </summary>
		private static float CalculateMaxUpper(Modifier modifier, ModifierInstance instance, ItemInstance item)
		{
			// Base max value
			float maxValue = Mathf.Max(modifier.UpperMin, modifier.UpperMax);

			// Get the actual item level from the item that contains this modifier
			int itemLevel = item?.Level ?? 1;

			// Account for level scaling using the actual item level
			// Handle invalid UpperPerLevel values (NaN, infinity) by defaulting to 0
			float upperPerLevel = float.IsNaN(modifier.UpperPerLevel) || float.IsInfinity(modifier.UpperPerLevel) ? 0f : modifier.UpperPerLevel;
			maxValue += upperPerLevel * itemLevel;

			Debug.Log($"[MaxSpecialModifiers] CalculateMaxUpper - Base: {Mathf.Max(modifier.UpperMin, modifier.UpperMax):F3}, UpperPerLevel: {upperPerLevel:F3}, ItemLevel: {itemLevel}, Scaling: {upperPerLevel * itemLevel:F3}");

			// Apply multiplier from the affix if present
			if (instance.Affix != null)
			{
				maxValue *= instance.Affix.Multiplier;
				Debug.Log($"[MaxSpecialModifiers] Applied affix multiplier: {instance.Affix.Multiplier:F3}, Final: {maxValue:F3}");
			}

			return maxValue;
		}
	}

	/// <summary>
	/// Patch for AwakenedItemManager.IncrementKeropokKillCount to control Keropok completion timing
	/// </summary>
	[HarmonyPatch(typeof(AwakenedItemManager))]
	public class AwakenedItemManagerPatch
	{
		// Cache reflection field for performance
		private static readonly FieldInfo awakenedItemsField = typeof(AwakenedItemManager).GetField("awakenedItems", BindingFlags.NonPublic | BindingFlags.Instance);
		/// <summary>
		/// Controls when Keropok completion happens - only after exactly 6 non-implicit modifiers
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("IncrementKeropokKillCount")]
		static bool Prefix(AwakenedItemManager __instance, KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, ref bool __result)
		{
			try
			{
				Debug.Log($"[MaxSpecialModifiers] IncrementKeropokKillCount called - Item: {item?.Item?.ItemName}, Kills: {progress.NumKilled + 1}");

				if ((creature.State & CharacterContainerState.Hunter) == 0)
				{
					__result = false;
					return false; // Skip original method
				}

				progress.NumKilled++;

				// Let FixKeropokModifier run normally (adds normal affix)
				float chance = item.Mods.FixKeropokModifier(item, __instance.KeropokCurseTags, progress.NumKilled);
				Debug.Log($"[MaxSpecialModifiers] FixKeropokModifier returned chance: {chance:F3}");

				// Count non-implicit modifiers (excluding Keropok-tagged affixes)
				int nonImplicitCount = CountNonImplicitModifiers(item.Mods, item);
				Debug.Log($"[MaxSpecialModifiers] Current non-implicit modifiers: {nonImplicitCount}");

				if (nonImplicitCount < 6)
				{
					Debug.Log($"[MaxSpecialModifiers] Item has {nonImplicitCount} non-implicit modifiers (need 6), preventing Keropok completion");
					// Don't add Keropok implicit yet, keep the process going
					__result = true; // Success, but no Keropok completion
				}
				else if (nonImplicitCount == 6)
				{
					Debug.Log($"[MaxSpecialModifiers] Item has exactly 6 non-implicit modifiers, allowing Keropok completion");
					// We have 6 normal affixes, now allow Keropok completion
					if (progress.NumKilled > 5 || Helpers.PassedPercentage(chance))
					{
						Debug.Log($"[MaxSpecialModifiers] Keropok completion triggered! Adding implicit.");
						item.AddOrReplaceImplicit(__instance.KeropokTags);
						RemoveFromAwakenedItems(__instance, item.InstanceID);
					}
					__result = true;
				}
				else
				{
					Debug.Log($"[MaxSpecialModifiers] Item has more than 6 non-implicit modifiers ({nonImplicitCount}), allowing normal completion");
					// More than 6? Something went wrong, allow normal completion
					if (progress.NumKilled > 5 || Helpers.PassedPercentage(chance))
					{
						Debug.Log($"[MaxSpecialModifiers] Normal Keropok completion triggered.");
						item.AddOrReplaceImplicit(__instance.KeropokTags);
						RemoveFromAwakenedItems(__instance, item.InstanceID);
					}
					__result = true;
				}

				return false; // Skip original method
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in IncrementKeropokKillCount prefix: {ex.Message}");
				// Let original method run on error
				return true;
			}
		}

		/// <summary>
		/// Counts current modifiers that are NOT implicit (excluding Keropok-tagged affixes)
		/// </summary>
		private static int CountNonImplicitModifiers(ModifierList mods, ItemInstance item)
		{
			if (mods?.Mods == null)
			{
				Debug.Log($"[MaxSpecialModifiers] CountNonImplicitModifiers: No modifiers found");
				return 0;
			}

			int count = 0;
			foreach (var modifierInstance in mods.Mods)
			{
				if (modifierInstance?.Affix?.Affix == null)
				{
					continue;
				}

				var affix = modifierInstance.Affix.Affix;

				// Skip implicit modifiers
				if ((affix.Attributes & ItemModifierAttributes.Implicit) != 0)
				{
					continue;
				}

				// Skip Keropok-tagged affixes
				if (IsKeropokAffix(affix))
				{
					continue;
				}

				count++;
			}

			return count;
		}

		/// <summary>
		/// Checks if an affix is Keropok-tagged
		/// </summary>
		private static bool IsKeropokAffix(ItemAffix affix)
		{
			if (affix?.Tags == null)
			{
				return false;
			}

			return affix.Tags.Any(tag => tag?.GameTagName?.Contains("Keropok") == true);
		}

		/// <summary>
		/// Removes an item from the awakenedItems list using reflection
		/// </summary>
		private static void RemoveFromAwakenedItems(AwakenedItemManager instance, int itemInstanceID)
		{
			try
			{
				if (awakenedItemsField != null)
				{
					var awakenedItems = (System.Collections.Generic.Dictionary<int, KillQuestItemProgress>)awakenedItemsField.GetValue(instance);
					if (awakenedItems != null)
					{
						awakenedItems.Remove(itemInstanceID);
						Debug.Log($"[MaxSpecialModifiers] Removed item {itemInstanceID} from awakenedItems");
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error removing from awakenedItems: {ex.Message}");
			}
		}
	}

}
