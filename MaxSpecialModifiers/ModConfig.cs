using System.Collections.Generic;

namespace MaxSpecialModifiers
{
	/// <summary>
	/// Configuration for selecting which implicit affixes are available for each tag type
	/// </summary>
	public class ModConfig
	{
		/// <summary>
		/// Configuration for Keropok implicit affixes
		/// </summary>
		public Dictionary<string, bool> Keropok { get; set; } = new Dictionary<string, bool>
		{
			["Increased buff effect"] = true,
			["Increased buff duration"] = false,
			["HP Regen"] = false,
			["Damage Reflection"] = false,
			["Elemental Resistance"] = false,
			["Class Passives Multiplier"] = false,
			["HP Steal"] = false,
			["MP Steal"] = false,
			["Crisis Threshold"] = false,
			["Crisis Absorb"] = false,
			["Max HP"] = false,
			["Cold Chance Defense"] = false,
			["Movement Speed"] = false
		};

		/// <summary>
		/// Configuration for Orang Bunian implicit affixes
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
			["Attack Damage"] = true,
			["Cooldown Reduction"] = true,
			["Skill Speed"] = true,
			["Class Passives Multiplier"] = true,
			["Triggered Chance No Charge Use"] = true,
			["Triggered Damage Multiplier"] = true,
			["Crisis Damage"] = true,
			["Minion Movement Speed"] = true
		};

		/// <summary>
		/// Configuration for Awakened implicit affixes
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
	}
}
