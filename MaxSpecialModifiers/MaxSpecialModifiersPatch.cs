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
					SetModifierInstanceToMax(modifierInstance);
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
		private static void SetModifierInstanceToMax(ModifierInstance instance)
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
				float maxLower = CalculateMaxLower(modifier, instance);
				float maxUpper = CalculateMaxUpper(modifier, instance);

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
		private static float CalculateMaxLower(Modifier modifier, ModifierInstance instance)
		{
			// Base max value
			float maxValue = Mathf.Max(modifier.LowerMin, modifier.LowerMax);

			// Account for level scaling - we need to determine the item level
			// For now, we'll use a conservative approach and assume level 1 scaling
			// This could be improved by getting the actual item level
			maxValue += modifier.LowerPerLevel * 1f; // Assuming level 1 for now

			// Apply multiplier from the affix if present
			if (instance.Affix != null)
			{
				maxValue *= instance.Affix.Multiplier;
			}

			return maxValue;
		}

		/// <summary>
		/// Calculates the maximum upper value for a modifier
		/// </summary>
		private static float CalculateMaxUpper(Modifier modifier, ModifierInstance instance)
		{
			// Base max value
			float maxValue = Mathf.Max(modifier.UpperMin, modifier.UpperMax);

			// Account for level scaling
			maxValue += modifier.UpperPerLevel * 1f; // Assuming level 1 for now

			// Apply multiplier from the affix if present
			if (instance.Affix != null)
			{
				maxValue *= instance.Affix.Multiplier;
			}

			return maxValue;
		}
	}
}
