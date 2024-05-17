using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enums.GameRules
{
    public enum SplitScreenMergingType
    {
        [Description("None")]
        None,
        [Description("Screen Per Player")]
        ScreenPerPlayer,        //Each player gets their own screen
        [Description("Shared Screen - Locked")]
        SharedScreenLocked,   //Each player must be in the same screen at all times, screen does not zoom out if characters try to move past the screen edge
        [Description("Shared Screen - Adjust")]
        SharedScreenAdjust     //Each player must be in the same screen at all times, but the screen will zoom out if characters try to move past the screen edge 
    }
}
