
namespace Levels.OverworldLevels.TileMapping
{
    public static class TileMappingConstants
    {
        public const int TileMapCastleFloorAtlasId = 0;
        public const int TileMapFrostFloorAtlasId = 10;

        // 32 * SQRT(2) = diagonal distance between interior blocks
        public const double DiagonalDistanceBetweenInteriorBlocks = 45.25483399593904;

        public const int NumberOfIterationsBeforeChangingAngle = 500;
    }
}
