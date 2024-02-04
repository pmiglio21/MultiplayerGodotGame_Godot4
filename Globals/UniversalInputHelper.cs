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

        public static List<string> GetPlayersWhoJustPressedButton(InputType inputType)
        {
            List<string> playersWhoJustPressedButton = new List<string>();

            if (Input.IsActionJustPressed($"{inputType}_0"))
            {
                playersWhoJustPressedButton.Add("0");
            }

            if(Input.IsActionJustPressed($"{inputType}_1"))
            {
                playersWhoJustPressedButton.Add("1");
            }

            if (Input.IsActionJustPressed($"{inputType}_2"))
            {
                playersWhoJustPressedButton.Add("2");
            }

            if (Input.IsActionJustPressed($"{inputType}_3"))
            {
                playersWhoJustPressedButton.Add("3");
            }

            if (Input.IsActionJustPressed($"{inputType}_Keyboard"))
            {
                playersWhoJustPressedButton.Add("Keyboard");
            }

            return playersWhoJustPressedButton;
        }
    }
}
