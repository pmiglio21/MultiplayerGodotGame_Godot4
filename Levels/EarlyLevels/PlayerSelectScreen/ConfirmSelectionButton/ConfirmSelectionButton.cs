using Godot;
using System.Linq;

namespace Scenes.UI.PlayerSelectScene
{
	public partial class ConfirmSelectionButton : Node
	{
        private PlayerCharacterSelectScreenManager _playerCharacterSelectScreenManager;

        public override void _Ready()
		{
            _playerCharacterSelectScreenManager = GetParent() as PlayerCharacterSelectScreenManager;

            var sprite = GetNode<Sprite2D>("Sprite");
		}

		public override void _Process(double delta)
		{
			if (_playerCharacterSelectScreenManager.ActivePickers.Count != 0 &&
                _playerCharacterSelectScreenManager.ActivePickers.All(x => x.SelectionHasBeenMade))
			{
				var sprite = this.GetNode("Sprite") as Sprite2D;
				Texture2D newTexture = ResourceLoader.Load("res://Levels/EarlyLevels/PlayerSelectScreen/ConfirmSelectionButton/Animations/ConfirmSelectionButton_Ready.png") as Texture2D;
				sprite.Texture = newTexture;
			}
			else
			{
				var sprite = this.GetNode("Sprite") as Sprite2D;
				Texture2D newTexture = ResourceLoader.Load("res://Levels/EarlyLevels/PlayerSelectScreen/ConfirmSelectionButton/Animations/ConfirmSelectionButton_Waiting.png") as Texture2D;
				sprite.Texture = newTexture;
			}
		}
	}
}
