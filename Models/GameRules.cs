using Enums.GameRules;
using System.Collections.Generic;

namespace Models
{
    public class GameRules
    {
        public Dictionary<string, bool> LevelSizes = new Dictionary<string, bool>()
        {
            { "Small", false },
            { "Medium", false },
            { "Large", false },
        };

        public int NumberOfLevels = 1;

        public bool IsEndlessLevelsOn = false;

        public Dictionary<string, bool> BiomeTypes = new Dictionary<string, bool>()
        {
            { "Castle", false },
            { "Cave", false },
            { "Swamp", false },
            { "Frost", false },
        };

        public Dictionary<string, bool> SpawnProximityTypes = new Dictionary<string, bool>()
        {
            { "Super Close", false },
            { "Close", false },
            { "Normal", false },
            { "Far", false },
        };

        public Dictionary<string, bool> SwitchProximityTypes = new Dictionary<string, bool>()
        {
            { "Super Close", false },
            { "Close", false },
            { "Normal", false },
            { "Far", false },
        };

        public bool CanMinibossSpawn = false;

        public bool CanBossSpawn = false;

        public bool IsFriendlyFireOn = false;
    }
}
