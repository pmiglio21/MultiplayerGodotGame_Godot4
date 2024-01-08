using Globals.PlayerManagement;
using MobileEntities.PlayerCharacters.Scripts;
using Scenes.UI.PlayerSelectScene;
using Godot;

namespace Levels.OverworldLevels.Utilities
{
	public partial class PlayerInstancer : Node
	{
		private Vector2 _tempSpawnPosition = new Vector2(303, 181);


		public override void _Ready()
		{
			//Going to have to find a way to not dispose of the characters every time, keep them global somehow

			foreach (BaseCharacter character in PlayerManager.ActivePlayers)
			{
				GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

				character.GlobalPosition = _tempSpawnPosition;

				//if (character.PlayerNumber == 0)
				//{
				//	character.GlobalPosition = new Vector2(50, 50);
				//}
				//else if (character.PlayerNumber == 1)
				//{
				//	character.GlobalPosition = new Vector2(50, 50);
				//}
				//else if (character.PlayerNumber == 2)
				//{
				//	character.GlobalPosition = new Vector2(-15, 15);
				//}
				//else if (character.PlayerNumber == 3)
				//{
				//	character.GlobalPosition = new Vector2(15, 15);
				//}

				AddChild(character);
			}

			PlayerCharacterPickerManager.ActivePickers.Clear();
		}

		public override void _Process(double delta)
		{
		}
	}
}
