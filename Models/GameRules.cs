using Enums.GameRules;
using System.Collections.Generic;

namespace Models
{
    public class GameRules
    {
        //public Dictionary<LevelSize, bool> LevelSizes = new Dictionary<LevelSize, bool>()
        //{
        //    { LevelSize.Small, false },
        //    { LevelSize.Medium, false },
        //    { LevelSize.Large, false },
        //};

        public Dictionary<string, bool> LevelSizes = new Dictionary<string, bool>()
        {
            { "Small", false },
            { "Medium", false },
            { "Large", false },
        };

        public int NumberOfLevels = 1;

        public string IsInfiniteLevelsOn = "OFF";

        public Dictionary<BiomeType, bool> BiomeTypes = new Dictionary<BiomeType, bool>()
        {
            { BiomeType.Castle, false },
            { BiomeType.Cave, false },
            { BiomeType.Swamp, false },
            { BiomeType.Frost, false },
        };

        public Dictionary<SpawnProximityType, bool> SpawnProximityTypes = new Dictionary<SpawnProximityType, bool>()
        {
            { SpawnProximityType.SuperClose, false },
            { SpawnProximityType.Close, false },
            { SpawnProximityType.Normal, false },
            { SpawnProximityType.Far, false },
        };

        public Dictionary<SwitchProximityType, bool> SwitchProximityTypes = new Dictionary<SwitchProximityType, bool>()
        {
            { SwitchProximityType.SuperClose, false },
            { SwitchProximityType.Close, false },
            { SwitchProximityType.Normal, false },
            { SwitchProximityType.Far, false },
        };

        public string CanMinibossSpawn = "OFF";

        public string CanBossSpawn = "OFF";

        public string IsFriendlyFireOn = "OFF";
    }
}
