using Globals;
using Godot;

namespace Models
{
    public class Settings
    {
        public float MusicVolume = .50f;
        public float SoundEffectsVolume = .50f;
        public float DungeonSoundsVolume = .50f;
        public string FullscreenState = GlobalConstants.OffOnOptionOff;
    }
}
