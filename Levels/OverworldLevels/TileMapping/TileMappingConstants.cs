
namespace Levels.OverworldLevels.TileMapping
{
    public static class TileMappingConstants
    {
        #region Atlas Ids

        public const int TileMapCastleFloorAtlasId = 0;

        public const int TileMapFrostFloorAtlasId = 40;
        public const int TileMapFrostFloorPureWater1AtlasId = 41;
        public const int TileMapFrostFloorPureWater2AtlasId = 42;

        #endregion

        // 32 * SQRT(2) = diagonal distance between interior blocks
        public const double DiagonalDistanceBetweenInteriorBlocks = 45.25483399593904;

        public const int NumberOfIterationsBeforeChangingAngle_PathCreation = 250;
        //public const int NumberOfIterationsBeforeChangingAngle_PathCreation = 500;
        public const int NumberOfIterationsBeforeChangingAngle_WaterCreation = 100;
    }
}
