using FrogGame.Globals.PlayerManagement;
using FrogGame.MobileEntities.PlayerCharacters.Scripts;
using FrogGame.Scenes.UI.PlayerSelectScene;
using Godot;

namespace FrogGame.Scenes.OverworldLevels
{
	public partial class PlayerInstancer : Node
	{
		public override void _Ready()
		{
			//Going to have to find a way to not dispose of the characters every time, keep them global somehow

			foreach (BaseCharacter character in PlayerManager.ActivePlayers)
			{
				GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}");
				AddChild(character);
			}

			PlayerCharacterPickerManager.ActivePickers.Clear();
		}

		public override void _Process(double delta)
		{
		}
	}
}
