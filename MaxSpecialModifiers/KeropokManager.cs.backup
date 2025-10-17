using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Manages Keropok progression logic with custom 6-modifier completion requirements
	/// </summary>
	public static class KeropokManager
	{
		// Cache reflection field for performance
		private static readonly FieldInfo awakenedItemsField = typeof(AwakenedItemManager).GetField("awakenedItems", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void DebugLog(string message)
		{
			if (ModLoader.Config?.DebugLogging == true)
			{
				Debug.Log($"[MaxSpecialModifiers] {message}");
			}
		}

		/// <summary>
		/// Processes Keropok kill count with our custom logic
		/// </summary>
		public static bool ProcessKeropokKillCount(KillQuestItemProgress progress, ItemInstance item, CharacterContainer creature, AwakenedItemManager instance)
		{
			try
			{
				DebugLog($"Processing Keropok item: {item.Item?.ItemName}, Current kills: {progress.NumKilled}");

				// Count current non-implicit modifiers (excluding Keropok-tagged affixes)
				int nonImplicitCount = CountNonImplicitModifiers(item.Mods, item);
				DebugLog($"Current non-implicit modifiers: {nonImplicitCount}");

				// Increment kill count
				progress.NumKilled++;

				// Let FixKeropokModifier run to add a normal affix
				float chance = item.Mods.FixKeropokModifier(item, instance.KeropokCurseTags, progress.NumKilled);
				DebugLog($"FixKeropokModifier returned chance: {chance:F3}");

				// Recount after adding the new affix
				int newNonImplicitCount = CountNonImplicitModifiers(item.Mods, item);
				DebugLog($"Non-implicit modifiers after FixKeropokModifier: {newNonImplicitCount}");

				// Check if we should add the Keropok implicit
				if (newNonImplicitCount >= 5) // 5 from Keropok process + 1 existing = 6 total
				{
					DebugLog($"Item has {newNonImplicitCount} non-implicit modifiers (6+ total), allowing Keropok completion");

					if (progress.NumKilled > 5 || Helpers.PassedPercentage(chance))
					{
						DebugLog($"Keropok completion triggered! Adding implicit.");
						item.AddOrReplaceImplicit(instance.KeropokTags);
						RemoveFromAwakenedItems(instance, item.InstanceID);
					}
				}
				else
				{
					DebugLog($"Item has {newNonImplicitCount} non-implicit modifiers (need {5 - newNonImplicitCount} more), preventing Keropok completion");
				}

				return true; // Item was processed
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error processing Keropok kill count: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Counts current modifiers that are NOT implicit (excluding Keropok-tagged affixes)
		/// </summary>
		public static int CountNonImplicitModifiers(ModifierList mods, ItemInstance item)
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
		public static bool IsKeropokAffix(ItemAffix affix)
		{
			if (affix?.Tags == null)
			{
				return false;
			}

			return affix.Tags.Any(tag => tag?.GameTagName == "Keropok");
		}

		/// <summary>
		/// Removes an item from the awakenedItems list using reflection
		/// </summary>
		public static void RemoveFromAwakenedItems(AwakenedItemManager instance, int itemInstanceID)
		{
			try
			{
				if (awakenedItemsField != null)
				{
					var awakenedItems = (System.Collections.Generic.Dictionary<int, KillQuestItemProgress>)awakenedItemsField.GetValue(instance);
					if (awakenedItems != null)
					{
						awakenedItems.Remove(itemInstanceID);
						DebugLog($"Removed item {itemInstanceID} from awakenedItems");
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
