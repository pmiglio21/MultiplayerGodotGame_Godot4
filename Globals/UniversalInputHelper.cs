using Enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Globals
{
    public static class UniversalInputHelper
    {
        public static bool IsButtonJustPressed(InputType inputType)
        {
            return (Input.IsActionJustPressed($"{inputType}_0") || Input.IsActionJustPressed($"{inputType}_1") || 
                    Input.IsActionJustPressed($"{inputType}_2") || Input.IsActionJustPressed($"{inputType}_3") || 
                    Input.IsActionJustPressed($"{inputType}_Keyboard"));
        }
    }
}
