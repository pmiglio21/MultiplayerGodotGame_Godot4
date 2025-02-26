using Globals;
using System.Collections.Generic;

namespace Models
{
    public class GameRules
    {
        public string RulesetName = string.Empty;

        public Dictionary<string, bool> LevelSizes = new Dictionary<string, bool>()
        {
            { GlobalConstants.LevelSizeSmall, true },
            { GlobalConstants.LevelSizeMedium, true },
            { GlobalConstants.LevelSizeLarge, true },
        };

        public int NumberOfLevels = 1;

        public bool IsEndlessLevelsOn = false;

        public Dictionary<string, bool> BiomeTypes = new Dictionary<string, bool>()
        {
            { GlobalConstants.BiomeCastle, true },
            { GlobalConstants.BiomeCave, true },
            { GlobalConstants.BiomeSwamp, true },
            { GlobalConstants.BiomeFrost, true },
        };

        public Dictionary<string, bool> SpawnProximityTypes = new Dictionary<string, bool>()
        {
            { GlobalConstants.SpawnProximityIndividual, true },
            { GlobalConstants.SpawnProximityGroup, true },
        };

        public Dictionary<string, bool> SwitchProximityTypes = new Dictionary<string, bool>()
        {
            { GlobalConstants.SwitchProximitySuperClose, true },
            { GlobalConstants.SwitchProximityClose, true },
            { GlobalConstants.SwitchProximityNormal, true },
            { GlobalConstants.SwitchProximityFar, true },
        };

        public bool CanMinibossSpawn = false;

        public bool CanBossSpawn = false;

        public bool IsFriendlyFireOn = false;
    }
}
