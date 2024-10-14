using Enums.GameRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class GameRules
    {
        public SplitScreenMergingType CurrentSplitScreenMergingType = SplitScreenMergingType.None;

        public RelativePlayerSpawnDistanceType CurrentRelativePlayerSpawnDistanceType = RelativePlayerSpawnDistanceType.None;

        public string NumberOfLevels = string.Empty;

        public LevelSize CurrentLevelSize = LevelSize.None;
    }
}
