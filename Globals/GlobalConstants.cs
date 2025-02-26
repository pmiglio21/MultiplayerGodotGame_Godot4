using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Globals
{
    public static class GlobalConstants
    {
        #region Level Size

        public const string LevelSizeSmall = "Small";
        public const string LevelSizeMedium = "Medium";
        public const string LevelSizeLarge = "Large";

        #endregion

        #region Spawn Proximity

        public const string SpawnProximityIndividual = "Individual";
        public const string SpawnProximityGroup = "Group";

        #endregion

        #region Switch Proximity

        public const string SwitchProximitySuperClose = "Super Close";
        public const string SwitchProximityClose = "Close";
        public const string SwitchProximityNormal = "Normal";
        public const string SwitchProximityFar = "Far";

        #endregion

        #region Biomes

        public const string BiomeCastle = "Castle";
        public const string BiomeCave = "Cave";
        public const string BiomeSwamp = "Swamp";
        public const string BiomeFrost = "Frost";

        #endregion

        #region Off/On Options

        public const string OffOnOptionOff = "OFF";
        public const string OffOnOptionOn = "ON";

        public static Vector2I DefaultResolution = new Vector2I(1152, 648);

        #endregion
    }
}
