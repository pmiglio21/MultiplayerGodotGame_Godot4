using Enums.GameRules;
using System.Collections.Generic;

namespace Models
{
    public class GameRules
    {
        public List<LevelSize> LevelSizes = new List<LevelSize>();

        public string NumberOfLevels = string.Empty;

        public List<BiomeType> BiomeTypes = new List<BiomeType>();

        public List<SpawnProximityType> SpawnProximityTypes = new List<SpawnProximityType>();

        public List<SwitchProximityType> SwitchProximityTypes = new List<SwitchProximityType>();

        public bool CanMinibossSpawn = false;

        public bool CanBossSpawn = false;

        public bool IsFriendlyFireOn = false;
    }
}
