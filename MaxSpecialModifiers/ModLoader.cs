using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Entry point for the MaxSpecialModifiers mod.
	/// This mod [[Add your mod description here]].
	/// </summary>
	public class ModLoader : IModLoader
	{
		private static Harmony harmony;
		private static string ConfigPath = Path.Combine(LoadingManager.PersistantDataPath, "max-special-modifiers.config.json");
		public static ModConfig Config;

		/// <summary>
		/// Called when mod is first loaded.
		/// </summary>
		public void OnCreated()
		{
			Debug.Log("[MaxSpecialModifiers] OnCreated() called - Applying patches...");

			try
			{
				// Load or create configuration
				LoadConfig();

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
		/// Loads configuration from file or creates default config
		/// </summary>
		private void LoadConfig()
		{
			try
			{
				if (File.Exists(ConfigPath))
				{
					string json = File.ReadAllText(ConfigPath);
					Config = JsonConvert.DeserializeObject<ModConfig>(json);
					Debug.Log($"[MaxSpecialModifiers] Configuration loaded from: {ConfigPath}");
				}
				else
				{
					Config = CreateAndSaveNewConfig();
					Debug.Log($"[MaxSpecialModifiers] Created default configuration at: {ConfigPath}");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error loading configuration: {ex.Message}");
				Config = new ModConfig(); // Use default config on error
			}
		}

		/// <summary>
		/// Creates and saves a new default configuration
		/// </summary>
		private ModConfig CreateAndSaveNewConfig()
		{
			var config = new ModConfig
			{
				// Add your default configuration properties here
			};
			File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));
			return config;
		}

		/// <summary>
		/// Saves the current configuration to file
		/// </summary>
		public static void SaveConfig()
		{
			try
			{
				if (Config != null)
				{
					string configJson = JsonConvert.SerializeObject(Config, Formatting.Indented);
					File.WriteAllText(ConfigPath, configJson);
					Debug.Log($"[MaxSpecialModifiers] Configuration saved to: {ConfigPath}");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error saving configuration: {ex.Message}");
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
			
			// Log configuration on game load for debugging
			if (Config.EnableDebugLogging)
			{
				Debug.Log($"[MaxSpecialModifiers] Mod enabled: {Config.Enabled}");
				Debug.Log($"[MaxSpecialModifiers] Debug logging: {Config.EnableDebugLogging}");
			}
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
