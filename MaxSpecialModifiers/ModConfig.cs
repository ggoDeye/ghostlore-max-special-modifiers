using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Configuration for selecting which implicit affixes are available for each tag type
	/// </summary>
	public class ModConfig
	{
		/// <summary>
		/// Enable debug logging for forced affixes
		/// </summary>
		public bool DebugLogging { get; set; } = false;

		/// <summary>
		/// Indicates if the configuration was loaded successfully or if we're using fallback behavior
		/// </summary>
		public bool IsConfigurationValid { get; set; } = true;

		private static void DebugLog(string message)
		{
			if (ModLoader.Config?.DebugLogging == true)
			{
				Debug.Log($"[MaxSpecialModifiers] {message}");
			}
		}

		/// <summary>
		/// Configuration for Keropok forced affixes
		/// </summary>
		public Dictionary<string, bool> Keropok { get; set; } = new Dictionary<string, bool>
		{
			["Increased buff effect"] = true,
			["Increased buff duration"] = true,
			["HP Regen"] = true,
			["Damage Reflection"] = true,
			["Elemental Resistance"] = true,
			["Class Passives Multiplier"] = true,
			["HP Steal"] = true,
			["MP Steal"] = true,
			["Crisis Threshold"] = true,
			["Crisis Absorb"] = true,
			["Max HP"] = true,
			["Cold Chance Defense"] = true,
			["Movement Speed"] = true
		};

		/// <summary>
		/// Configuration for Orang Bunian forced affixes
		/// </summary>
		public Dictionary<string, bool> OrangBunian { get; set; } = new Dictionary<string, bool>
		{
			["Additional Minions"] = true,
			["Minion Max HP"] = true,
			["HP Multiplier"] = true,
			["Max Skill Uses"] = true,
			["Increased Movement Speed"] = true,
			["Elemental Chance"] = true,
			["Absorb"] = true,
			["Increased Projectile Radius"] = true,
			["Basic attack as fire"] = true,
			["Basic attack as ice"] = true,
			["Fire penetration"] = true,
			["Ice penetration"] = true,
			["Blind on hit"] = true,
			["Slow on hit"] = true,
			["Fire Resistance Cap"] = true,
			["Ice Resistance Cap"] = true,
			["Attack Damage"] = true
		};

		/// <summary>
		/// Configuration for Awakened forced affixes
		/// </summary>
		public Dictionary<string, bool> Awakened { get; set; } = new Dictionary<string, bool>
		{
			["Minion Damage"] = true,
			["Minion Avoidance"] = true,
			["MP Multiplier"] = true,
			["Cooldown Reduction"] = true,
			["Elemental Multiplier"] = true,
			["Elemental Resistance"] = true,
			["Projectile Speed"] = true,
			["Armour Break"] = true,
			["Basic attack as lightning"] = true,
			["Basic attack as poison"] = true,
			["Lightning penetration"] = true,
			["Poison penetration"] = true,
			["Frenzy on hit"] = true,
			["Agility on hit"] = true,
			["Lightning Resistance Cap"] = true,
			["Poison Resistance Cap"] = true,
			["Skill Damage"] = true,
			["Critical Hit Multiplier"] = true,
			["Crisis Threshold"] = true,
			["Triggered Chance No Charge Use"] = true,
			["Triggered Skill Speed"] = true,
			["Crisis Absorb"] = true,
			["Movement Skill Distance Multiplier"] = true
		};

		/// <summary>
		/// Static mapping of affix display names to their exact ItemAffixName in the game
		/// This ensures we match the correct affix, including complex multi-modifier affixes
		/// </summary>
		private static readonly Dictionary<string, string> AffixNameMapping = new Dictionary<string, string>
		{
			// Keropok affixes
			["Increased buff effect"] = "Keropok Increased buff effect",
			["Increased buff duration"] = "Keropok Increased buff duration",
			["HP Regen"] = "Keropok HP Regen",
			["Damage Reflection"] = "Keropok Damage Reflection",
			["Elemental Resistance"] = "Keropok Elemental Resistance",
			["Class Passives Multiplier"] = "Keropok Class Passives Multiplier",
			["HP Steal"] = "Keropok HP Steal",
			["MP Steal"] = "Keropok MP Steal",
			["Crisis Threshold"] = "Keropok Crisis Threshold",
			["Crisis Absorb"] = "Keropok Crisis Absorb",
			["Max HP"] = "Keropok Max HP",
			["Cold Chance Defense"] = "Keropok Cold Chance Defense",
			["Movement Speed"] = "Keropok Movement Speed",

			// Orang Bunian affixes
			["Additional Minions"] = "Additional Minions",
			["Minion Max HP"] = "Minion Max HP",
			["HP Multiplier"] = "HP Multiplier",
			["Max Skill Uses"] = "Max Skill Uses",
			["Increased Movement Speed"] = "Increased Movement Speed",
			["Elemental Chance"] = "Elemental Chance",
			["Absorb"] = "Absorb",
			["Increased Projectile Radius"] = "Increased Projectile Radius",
			["Basic attack as fire"] = "Basic attack as fire",
			["Basic attack as ice"] = "Basic attack as ice",
			["Fire penetration"] = "Fire penetration",
			["Ice penetration"] = "Ice penetration",
			["Blind on hit"] = "Blind on hit",
			["Slow on hit"] = "Slow on hit",
			["Fire Resistance Cap"] = "Fire Resistance Cap",
			["Ice Resistance Cap"] = "Ice Resistance Cap",
			["Attack Damage"] = "More basic attack damage longer cooldown",
			["Class Passives Multiplier"] = "Class Passives Multiplier",
			["Faster cooldown less cast speed"] = "Faster cooldown less cast speed",
			["Triggered skills reduced cooldown and damage multiplier"] = "Triggered skills reduced cooldown and damage multiplier",
			["Triggered skills increased damage"] = "Triggered skills increased damage",
			["Crisis Damage"] = "Crisis Damage",
			["Blessed Minion Movement Speed"] = "Blessed Minion Movement Speed",

			// Awakened affixes
			["Minion Damage"] = "Minion Damage",
			["Minion Avoidance"] = "Minion Avoidance",
			["MP Multiplier"] = "MP Multiplier",
			["Cooldown Reduction"] = "Cooldown Reduction",
			["Elemental Multiplier"] = "Elemental Multiplier",
			["Elemental Resistance"] = "Elemental Resistance",
			["Projectile Speed"] = "Projectile Speed",
			["Armour Break"] = "Armour Break",
			["Basic attack as lightning"] = "Basic attack as lightning",
			["Basic attack as poison"] = "Basic attack as poison",
			["Lightning penetration"] = "Lightning penetration",
			["Poison penetration"] = "Poison penetration",
			["Frenzy on hit"] = "Frenzy on hit",
			["Agility on hit"] = "Agility on hit",
			["Lightning Resistance Cap"] = "Lightning Resistance Cap",
			["Poison Resistance Cap"] = "Poison Resistance Cap",
			["Skill Damage"] = "Faster cooldown less skill damage",
			["Critical Hit Multiplier"] = "Faster cooldown less critical multiplier",
			["Crisis Threshold"] = "Crisis threshhold",
			["Triggered Chance No Charge Use"] = "Triggered skills reduced cooldown and slower cast speed",
			["Triggered Skill Speed"] = "Triggered skills cast speed",
			["Crisis Absorb"] = "Crisis Absorb",
			["Movement Skill Distance Multiplier"] = "Movement Skill Distance Multiplier"
		};

		/// <summary>
		/// Gets the exact ItemAffixName for an affix display name
		/// </summary>
		public static string GetAffixName(string displayName)
		{
			return AffixNameMapping.TryGetValue(displayName, out var affixName) ? affixName : "";
		}

		/// <summary>
		/// Saves the configuration to JSON file
		/// </summary>
		public void SaveToFile(string filePath)
		{
			try
			{
				string json = JsonConvert.SerializeObject(this, Formatting.Indented);
				File.WriteAllText(filePath, json);
				DebugLog($"Configuration saved to {filePath}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error saving configuration: {ex.Message}");
			}
		}

		/// <summary>
		/// Loads configuration from JSON file, creates default if not found
		/// </summary>
		public static ModConfig LoadFromFile(string filePath)
		{
			try
			{
				if (File.Exists(filePath))
				{
					string json = File.ReadAllText(filePath);
					var config = JsonConvert.DeserializeObject<ModConfig>(json);
					config.IsConfigurationValid = true;
					DebugLog($"Configuration loaded from {filePath}");
					return config;
				}
				else
				{
					DebugLog($"Configuration file not found, creating default at {filePath}");
					var defaultConfig = new ModConfig();
					defaultConfig.IsConfigurationValid = true;
					defaultConfig.SaveToFile(filePath);
					return defaultConfig;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[MaxSpecialModifiers] Error loading configuration: {ex.Message}");
				Debug.LogError($"[MaxSpecialModifiers] Using fallback configuration - mod will use original game behavior");
				var fallbackConfig = new ModConfig();
				fallbackConfig.IsConfigurationValid = false;
				return fallbackConfig;
			}
		}
	}
}