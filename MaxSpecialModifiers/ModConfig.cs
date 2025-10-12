using System;

namespace MaxSpecialModifiers
{
    /// <summary>
    /// Configuration class for MaxSpecialModifiers mod settings
    /// </summary>
    [Serializable]
    public class ModConfig
    {
        /// <summary>
        /// Whether the mod is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to enable debug logging
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        // Add your configuration properties here
        // Examples:
        // public int SomeNumberSetting { get; set; } = 10;
        // public float SomeFloatSetting { get; set; } = 1.5f;
        // public string SomeStringSetting { get; set; } = "default value";
        // public GameKey SomeHotkey { get; set; } = GameKey.Slot1;
    }
}
