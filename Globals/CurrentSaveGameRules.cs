using Enums.GameRules;

namespace Globals
{
    public static class CurrentSaveGameRules
    {
        public static SplitScreenMergingType CurrentSplitScreenMergingType = SplitScreenMergingType.None;

        public static RelativePlayerSpawnDistanceType CurrentRelativePlayerSpawnDistanceType = RelativePlayerSpawnDistanceType.None;

        public static string NumberOfLevels = string.Empty;
    }
}
