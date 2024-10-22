using Enums;
using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Scenes.UI.PlayerSelectScene;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerCharacterSelectScreenManager : Control
{
	#region Signals

	[Signal]
    public delegate void GoToGameRulesScreenEventHandler();

    [Signal]
    public delegate void GoToDungeonLevelSwapperEventHandler();

    #endregion

    #region Properties

    public List<PlayerCharacterPicker> ActivePickers = new List<PlayerCharacterPicker>();

    #endregion

    public override void _Ready()
	{
        ActivePickers.Clear();
    }

	public override void _Process(double delta)
	{
	}

	private void OnPlayerCharacterPicker_TellPlayerCharacterSelectScreenToGoToGameRulesScreen()
	{
        ActivePickers.Clear();

        EmitSignal(SignalName.GoToGameRulesScreen);
	}

    private void OnPlayerCharacterPicker_TellPlayerCharacterSelectScreenToGoToDungeonLevelSwapper()
    {
        FinishSelectionForAllPickers();

        EmitSignal(SignalName.GoToDungeonLevelSwapper);
    }

    private void FinishSelectionForAllPickers()
    {
        if (ActivePickers.Count(x => x.SelectionHasBeenMade && x.CurrentPickerIsActivated) == ActivePickers.Count)
        {
            //Instances all characters from the currently activated pickers
            foreach (var picker in ActivePickers)
            {
                var currentPickerSprite = picker.GetNode("SelectedPlayerIcon") as Sprite2D;

                int matchingIndex = PlayerManager.AvailablePlayerImageOptions.IndexOf(currentPickerSprite.Texture.ResourcePath);

                var scene = GD.Load<PackedScene>(PlayerManager.AvailablePlayerSceneOptions[matchingIndex]);
                var instance = scene.Instantiate();

                //For ease of access
                var instanceAsBaseCharacter = instance as BaseCharacter;

                instanceAsBaseCharacter.CharacterClassName = DeterminePlayableCharacterClass(instance);

                instanceAsBaseCharacter.PlayerNumber = PlayerManager.ActivePlayers.Count;
                instanceAsBaseCharacter.DeviceIdentifier = picker.CurrentDeviceId.ToString();

                PlayerManager.ActivePlayers.Add(instanceAsBaseCharacter);

                GD.Print($"Added Player: {instanceAsBaseCharacter.PlayerNumber} on Device {instanceAsBaseCharacter.DeviceIdentifier}");

                if (instanceAsBaseCharacter.PlayerNumber == 0)
                {
                    instanceAsBaseCharacter.Position = new Vector2(-1, 1);
                }
                else if (instanceAsBaseCharacter.PlayerNumber == 1)
                {
                    instanceAsBaseCharacter.Position = new Vector2(1, 1);
                }
                else if (instanceAsBaseCharacter.PlayerNumber == 2)
                {
                    instanceAsBaseCharacter.Position = new Vector2(-1, -1);
                }
                else if (instanceAsBaseCharacter.PlayerNumber == 3)
                {
                    instanceAsBaseCharacter.Position = new Vector2(1, -1);
                }
            }

            GD.Print("---------------------------------------");
        }
    }

    private PlayableCharacterClass DeterminePlayableCharacterClass(Node instance)
    {
        if (instance is Knight)
        {
            return PlayableCharacterClass.Knight;
        }
        else if (instance is Mage)
        {
            return PlayableCharacterClass.Mage;
        }
        else if (instance is Rogue)
        {
            return PlayableCharacterClass.Rogue;
        }
        else if (instance is Cleric)
        {
            return PlayableCharacterClass.Cleric;
        }
        else
        {
            return PlayableCharacterClass.BaseCharacter;
        }
    }
}
