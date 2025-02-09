using Globals;
using Godot;

namespace Models
{
    public class Settings
    {
        public float MusicVolume = .50f;
        public float SoundEffectsVolume = .50f;
        public float DungeonSoundsVolume = .50f;
        public Vector2I Resolution = new Vector2I(1152, 648);
        public string FullscreenState = GlobalConstants.OffOnOptionOff;
    }
}
