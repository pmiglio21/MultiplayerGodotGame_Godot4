using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enums
{
    public enum InputType
    {
        //Left joystick controls
        MoveEast,
        MoveNorth,
        MoveWest,
        MoveSouth,

        //4 buttons on right side of controller
        EastButton,
        SouthButton,
        WestButton,
        NorthButton, 

        //Middle buttons
        StartButton,

        //Shoulder buttons

        //D-Pad buttons
        DPadEast, 
        DPadNorth,
        DPadWest,
        DPadSouth
    }
}
