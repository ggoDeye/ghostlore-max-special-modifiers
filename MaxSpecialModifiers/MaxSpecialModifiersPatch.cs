using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Harmony patch template for MaxSpecialModifiers.
	/// Replace this with your actual patches.
	/// 
	/// Common patch patterns:
	/// - [HarmonyPrefix] - Runs before the original method
	/// - [HarmonyPostfix] - Runs after the original method  
	/// - [HarmonyTranspiler] - Modifies IL code
	/// - [HarmonyFinalizer] - Runs after method completes (even on exceptions)
	/// </summary>
	[HarmonyPatch(typeof(SomeClass))]
	public class SomeClassPatch
	{
		/// <summary>
		/// Prefix patch - runs before the original method
		/// Return false to skip the original method, true to run it
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch("SomeMethod")]
		static bool Prefix(SomeClass __instance)
		{
			try
			{
				// Check if mod is enabled
				if (!ModLoader.Config.Enabled)
					return true; // Let original method run if mod is disabled

				// Log debug information if enabled
				if (ModLoader.Config.EnableDebugLogging)
					Debug.Log("[MaxSpecialModifiers] SomeMethod prefix called");
				
				// Your patch logic here
				// Access original method parameters via __instance
				
				// Return false to skip original method, true to run original method
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in SomeMethod prefix: {ex.Message}");
				return true; // Let original method run on error
			}
		}

		/// <summary>
		/// Postfix patch - runs after the original method
		/// Useful for modifying return values or performing cleanup
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch("SomeMethod")]
		static void Postfix(SomeClass __instance, ref int __result)
		{
			try
			{
				if (!ModLoader.Config.Enabled)
					return;

				if (ModLoader.Config.EnableDebugLogging)
					Debug.Log($"[MaxSpecialModifiers] SomeMethod postfix called, result: {__result}");
				
				// Your postfix logic here
				// Modify __result to change the return value
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in SomeMethod postfix: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Example patch for input handling using the game's InputManager
	/// </summary>
	[HarmonyPatch(typeof(InputManager))]
	public class InputManagerPatch
	{
		private static float lastActionTime = 0f;
		private const float actionCooldown = 0.5f;

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		static void Postfix()
		{
			try
			{
				if (!ModLoader.Config.Enabled)
					return;

				// Example: Check for a hotkey using the game's InputManager
				// if (Singleton<InputManager>.instance != null && 
				//     Singleton<InputManager>.instance.IsKeyDown(GameKey.Slot1, 0) && 
				//     Time.time - lastActionTime > actionCooldown)
				// {
				//     // Perform action
				//     lastActionTime = Time.time;
				// }
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error in input handling: {ex.Message}");
			}
		}
	}
}
