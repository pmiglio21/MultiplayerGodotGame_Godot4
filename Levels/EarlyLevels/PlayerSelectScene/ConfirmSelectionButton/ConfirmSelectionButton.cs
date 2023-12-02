using FrogGame.Globals;
using FrogGame.Globals.PlayerManagement;
using FrogGame.MobileEntities.PlayerCharacters.Scripts;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogGame.Scenes.UI.PlayerSelectScene
{
	public partial class ConfirmSelectionButton : Node
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			var sprite = GetNode<Sprite3D>("Sprite");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (PlayerCharacterPickerManager.ActivePickers.Count != 0 &&
				PlayerCharacterPickerManager.ActivePickers.All(x => x.SelectionHasBeenMade))
			{
				var sprite = this.GetNode("Sprite") as Sprite3D;
				Texture2D newTexture = ResourceLoader.Load("res://Levels/EarlyLevels/PlayerSelectScene/ConfirmSelectionButton/Animations/ConfirmSelectionButton_Ready.png") as Texture2D;
				sprite.Texture = newTexture;
			}
			else
			{
				var sprite = this.GetNode("Sprite") as Sprite3D;
				Texture2D newTexture = ResourceLoader.Load("res://Levels/EarlyLevels/PlayerSelectScene/ConfirmSelectionButton/Animations/ConfirmSelectionButton_Waiting.png") as Texture2D;
				sprite.Texture = newTexture;
			}
		}

		//The character selection finishing process - tied to PlayerCharacterPicker via signal
		private void FinishSelectionForAllPickers()
		{
			if (PlayerCharacterPickerManager.ActivePickers.Count(x => x.SelectionHasBeenMade && x.CurrentPickerIsActivated) == PlayerCharacterPickerManager.ActivePickers.Count)
			{
				//Instances all characters from the currently activated pickers
				foreach (var picker in PlayerCharacterPickerManager.ActivePickers)
				{
					var currentPickerSprite = picker.GetNode("SelectedPlayerIcon") as Sprite3D;

					int matchingIndex = PlayerManager.AvailablePlayerImageOptions.IndexOf(currentPickerSprite.Texture.ResourcePath);

					var scene = GD.Load<PackedScene>(PlayerManager.AvailablePlayerSceneOptions[matchingIndex]);
					var instance = scene.Instantiate();

					//For ease of access
					var instanceAsBaseCharacter = instance as BaseCharacter;

					instanceAsBaseCharacter.PlayerNumber = PlayerManager.ActivePlayers.Count;
					instanceAsBaseCharacter.DeviceIdentifier = picker.CurrentDeviceId.ToString();

					PlayerManager.ActivePlayers.Add(instanceAsBaseCharacter);

					GD.Print($"Added Player: {instanceAsBaseCharacter.PlayerNumber} on Device {instanceAsBaseCharacter.DeviceIdentifier}");

					if (instanceAsBaseCharacter.PlayerNumber == 0)
					{
						instanceAsBaseCharacter.Position = new Vector3(-1, 0, -10);
					}
					else if (instanceAsBaseCharacter.PlayerNumber == 1)
					{
						instanceAsBaseCharacter.Position = new Vector3(1, 0, -10);
					}
					else if (instanceAsBaseCharacter.PlayerNumber == 2)
					{
						instanceAsBaseCharacter.Position = new Vector3(-1, 0, -8);
					}
					else if (instanceAsBaseCharacter.PlayerNumber == 3)
					{
						instanceAsBaseCharacter.Position = new Vector3(1, 0, -8);
					}
				}

				//Load next scene
				GetTree().ChangeSceneToFile(LevelScenePaths.OverworldLevel1Path);
			}
		}

		private void OnPlayerCharacterPicker_FinishSelectionProcess()
		{
			FinishSelectionForAllPickers();
		}
	}
}
