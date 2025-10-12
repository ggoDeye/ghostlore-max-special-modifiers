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
	/// Patch for GiveExperience.GiveEXP to control Keropok completion timing at a higher level
	/// </summary>
	[HarmonyPatch(typeof(GiveExperience))]
	public class GiveExperiencePatch
	{
		// Cache reflection field for performance
		private static readonly FieldInfo awakenedItemsField = typeof(AwakenedItemManager).GetField("awakenedItems", BindingFlags.NonPublic | BindingFlags.Instance);

		/// <summary>
		/// Controls the entire Keropok process by intercepting at the GiveEXP level
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("GiveEXP")]
		static bool Prefix(CharacterContainer attacker, CharacterContainer defender)
		{
			try
			{
				Debug.Log($"[MaxSpecialModifiers] GiveEXP called - Attacker: {attacker?.Creature?.CreatureName}, Defender: {defender?.Creature?.CreatureName}");
				Debug.Log($"[MaxSpecialModifiers] Defender State: {defender.State} (value: {(int)defender.State}), Hunter flag: {CharacterContainerState.Hunter} (value: {(int)CharacterContainerState.Hunter})");
				Debug.Log($"[MaxSpecialModifiers] Hunter check: {((defender.State & CharacterContainerState.Hunter) == 0)} (bitwise result: {((int)(defender.State & CharacterContainerState.Hunter))})");

				// Always run the original GiveEXP logic first for non-Hunter creatures
				if ((defender.State & CharacterContainerState.Hunter) == 0)
				{
					Debug.Log($"[MaxSpecialModifiers] Defender is not a Hunter, letting original method run");
					return true; // Let original method run
				}

				Debug.Log($"[MaxSpecialModifiers] Defender is a Hunter, processing with custom logic");

				// Handle Hunter creatures with our custom logic
				ProcessHunterKill(attacker, defender);

				// Don't let the original method run for Hunter creatures
				return false;
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in GiveEXP prefix: {ex.Message}");
				// Let original method run on error
				return true;
			}
		}

		/// <summary>
		/// Processes a Hunter kill with our custom logic (only processes one item at a time, like the original)
		/// </summary>
		private static void ProcessHunterKill(CharacterContainer attacker, CharacterContainer defender)
		{
			try
			{
				// Get all worn items from the attacker
				var inventories = attacker.Inventories;
				if (inventories == null) return;

				bool processedAnyItem = false; // Track if we've processed any Keropok item

				foreach (var inventory in inventories)
				{
					if (!inventory.IsWorn) continue;

					var items = inventory.Items;
					if (items == null) continue;

					foreach (var item in items)
					{
						// Give experience to the item (like the original method does)
						item.GiveExp();

						// Only process Keropok progression if we haven't processed any item yet
						// This replicates the original behavior where only one item is processed per kill
						if (!processedAnyItem)
						{
							bool itemWasProcessed = ProcessKeropokItem(item, defender);
							if (itemWasProcessed)
							{
								processedAnyItem = true;
								Debug.Log($"[MaxSpecialModifiers] Processed Keropok item, stopping processing for this kill");
							}
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error processing Hunter kill: {ex.Message}");
			}
		}

		/// <summary>
		/// Processes a single item for Keropok progression
		/// Returns true if the item was processed (was a Keropok item), false otherwise
		/// </summary>
		private static bool ProcessKeropokItem(ItemInstance item, CharacterContainer defender)
		{
			try
			{
				var awakenedItemManager = Singleton<AwakenedItemManager>.instance;
				if (awakenedItemManager == null) return false;

				// Check if this item is in the Keropok system
				var progress = awakenedItemManager.GetAwakenedProgress(item);
				if (progress == null)
				{
					return false; // Not a Keropok item
				}

				Debug.Log($"[MaxSpecialModifiers] Processing Keropok item: {item.Item?.ItemName}, Current kills: {progress.NumKilled}");

				// Count current non-implicit modifiers (excluding Keropok-tagged affixes)
				int nonImplicitCount = CountNonImplicitModifiers(item.Mods, item);
				Debug.Log($"[MaxSpecialModifiers] Current non-implicit modifiers: {nonImplicitCount}");

				// Increment kill count
				progress.NumKilled++;

				// Let FixKeropokModifier run to add a normal affix
				float chance = item.Mods.FixKeropokModifier(item, awakenedItemManager.KeropokCurseTags, progress.NumKilled);
				Debug.Log($"[MaxSpecialModifiers] FixKeropokModifier returned chance: {chance:F3}");

				// Recount after adding the new affix
				int newNonImplicitCount = CountNonImplicitModifiers(item.Mods, item);
				Debug.Log($"[MaxSpecialModifiers] Non-implicit modifiers after FixKeropokModifier: {newNonImplicitCount}");

				// Check if we should add the Keropok implicit
				if (newNonImplicitCount >= 5) // 5 from Keropok process + 1 existing = 6 total
				{
					Debug.Log($"[MaxSpecialModifiers] Item has {newNonImplicitCount} non-implicit modifiers (6+ total), allowing Keropok completion");

					if (progress.NumKilled > 5 || Helpers.PassedPercentage(chance))
					{
						Debug.Log($"[MaxSpecialModifiers] Keropok completion triggered! Adding implicit.");
						item.AddOrReplaceImplicit(awakenedItemManager.KeropokTags);
						RemoveFromAwakenedItems(awakenedItemManager, item.InstanceID);
					}
				}
				else
				{
					Debug.Log($"[MaxSpecialModifiers] Item has {newNonImplicitCount} non-implicit modifiers (need {5 - newNonImplicitCount} more), preventing Keropok completion");
				}

				return true; // Item was processed (it was a Keropok item)
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error processing Keropok item: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Counts current modifiers that are NOT implicit (excluding Keropok-tagged affixes)
		/// </summary>
		private static int CountNonImplicitModifiers(ModifierList mods, ItemInstance item)
		{
			if (mods?.Mods == null)
			{
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