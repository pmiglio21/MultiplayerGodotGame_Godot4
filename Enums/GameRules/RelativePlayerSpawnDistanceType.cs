using System.ComponentModel;

namespace Enums.GameRules
{
    public enum RelativePlayerSpawnDistanceType
    {
        None,
        [Description("Super Close")]
        SuperClose,
        [Description("Close")]
        Close,
        [Description("Normal")]
        Normal,
        [Description("Far")]
        Far
    }
}
