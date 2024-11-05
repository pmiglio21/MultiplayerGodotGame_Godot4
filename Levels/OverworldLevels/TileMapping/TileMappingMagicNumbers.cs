using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGodotGameGodot4.Levels.OverworldLevels.TileMapping
{
    public static class TileMappingMagicNumbers
    {
        public const int TileMapCastleFloorAtlasId = 0;

        // 32 * SQRT(2) = diagonal distance between interior blocks
        public const double DiagonalDistanceBetweenInteriorBlocks = 45.25483399593904;

        public const int NumberOfIterationsBeforeChangingAngle = 500;
    }
}
