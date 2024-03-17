using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enums.GameRules
{
    public enum SplitScreenMergingType
    {
        None,
        ScreenPerPlayer,        //Each player gets their own screen
        SharedScreenNoAdjust,   //Each player must be in the same screen at all times, screen does not zoom out if characters try to move past the screen edge
        SharedScreenAdjust,     //Each player must be in the same screen at all times, but the screen will zoom out if characters try to move past the screen edge 
        MergeAndSplit           //When players are near, use shared screen. When characters try to move past the screen edge, characters start using their own screens
    }
}
