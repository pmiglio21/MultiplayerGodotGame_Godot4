using System.ComponentModel;

namespace Enums.GameRules
{
    public enum SpawnProximityType
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
