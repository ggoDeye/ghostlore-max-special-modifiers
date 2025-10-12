using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Entry point for the MaxSpecialModifiers mod.
	/// This mod maximizes special modifier values (Keropok, Orang Bunian, Awakened) to their maximum values.
	/// </summary>
	public class ModLoader : IModLoader
	{
		private static Harmony harmony;

		/// <summary>
		/// Called when mod is first loaded.
		/// </summary>
		public void OnCreated()
		{
			Debug.Log("[MaxSpecialModifiers] OnCreated() called - Applying patches...");

			try
			{
				// Initialize Harmony for patching with error handling
				harmony = new Harmony("com.max-special-modifiers");
				Debug.Log("[MaxSpecialModifiers] Harmony created, applying patches...");
				harmony.PatchAll(Assembly.GetExecutingAssembly());
				Debug.Log("[MaxSpecialModifiers] Patches applied successfully");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error applying patches: {ex.Message}");
			}
		}

		/// <summary>
		/// Called when mod is unloaded.
		/// </summary>
		public void OnReleased()
		{
			// Clean up patches
			if (harmony != null)
			{
				harmony.UnpatchAll("com.max-special-modifiers");
				harmony = null;
			}
		}

		/// <summary>
		/// Called when game is loaded.
		/// </summary>
		/// <param name="mode">Either a new game, or a previously saved game.</param>
		public void OnGameLoaded(LoadMode mode)
		{
			Debug.Log("[MaxSpecialModifiers] OnGameLoaded() called");
		}

		/// <summary>
		/// Called when game is unloaded.
		/// </summary>
		public void OnGameUnloaded()
		{
			Debug.Log("[MaxSpecialModifiers] OnGameUnloaded() called");
		}
	}
}
