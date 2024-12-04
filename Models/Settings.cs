using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Settings
    {
        public float MusicVolume = .50f;
        public float SoundEffectsVolume = .50f;
        public float DungeonSoundsVolume = .50f;
        public Vector2I Resolution = new Vector2I(1152, 648);
        public string FullscreenState = "OFF";
    }
}
