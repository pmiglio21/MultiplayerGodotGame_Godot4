using Enums.GameRules;

namespace Models
{
    public class GameRules
    {
        public BiomeType BiomeType = BiomeType.None;

        public RelativePlayerSpawnDistanceType CurrentRelativePlayerSpawnDistanceType = RelativePlayerSpawnDistanceType.None;

        public string NumberOfLevels = string.Empty;

        public LevelSize CurrentLevelSize = LevelSize.None;
    }
}
