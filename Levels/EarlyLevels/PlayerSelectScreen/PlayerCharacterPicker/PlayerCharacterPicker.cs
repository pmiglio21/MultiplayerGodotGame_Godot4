using Enums;
using Globals;
using Globals.PlayerManagement;
using Godot;
using System.Linq;

namespace Scenes.UI.PlayerSelectScene
{
	public partial class PlayerCharacterPicker : Node
	{
		#region Signals
		
		[Signal]
		public delegate void FinishSelectionProcessStartedEventHandler();

        [Signal]
        public delegate void TellPlayerCharacterSelectScreenToGoToGameRulesScreenEventHandler();

        [Signal]
        public delegate void TellPlayerCharacterSelectScreenToGoToDungeonLevelSwapperEventHandler();

        #endregion

        private PlayerCharacterSelectScreenManager _playerCharacterSelectScreenManager;

		#region Exported Properties
		//Crucial for character picker activation to work in the proper order - needs to be set in the editor (for now)
		[Export] public int NumberOfCharactersNecessaryBeforeGivingAccess = -1;

		#endregion

		#region Player Selection Properties

		public string CurrentDeviceId = "-1";

		private int _playerSelectionChangeTimer = 0;
		
		private const int _playerSelectionChangeTimerMax = 10;
		
		private bool _playerSelectionChangedRecently = false;
		
		public bool SelectionHasBeenMade = false;
		
		private bool _isClassAlreadySelectedByOtherPickers = false;

		private bool _isSelectionEnabled = true;

		#endregion

		#region Picker Activation Properties

		public bool CurrentPickerIsActivated;

		#endregion

		#region Components 

		public Sprite2D PickerSprite;

		#endregion

		#region Color Changes

		private Color _defaultTone = new Color(1, 1, 1);

		private Color _selectedTone = new Color(.5f, 1f, .5f);

		#endregion

		public override void _Ready()
		{
			_playerCharacterSelectScreenManager = GetParent() as PlayerCharacterSelectScreenManager;

            PickerSprite = GetNode<Sprite2D>("SelectedPlayerIcon");
		}

		public override void _Process(double delta)
		{
			if (!CurrentPickerIsActivated && 
				NumberOfCharactersNecessaryBeforeGivingAccess <= _playerCharacterSelectScreenManager.ActivePickers.Count)
			{
				//See if user wants to go back to Title Level
				if (!CurrentPickerIsActivated)
				{
					if (_playerSelectionChangedRecently)
					{
						_playerSelectionChangeTimer++;
					}

					if (_playerSelectionChangeTimer >= _playerSelectionChangeTimerMax)
					{
						_playerSelectionChangeTimer = 0;
						_playerSelectionChangedRecently = false;
					}

					if (!_playerSelectionChangedRecently)
					{
						bool goToTitleLevel = UniversalInputHelper.IsActionJustPressed(InputType.EastButton);

						if (goToTitleLevel && _playerCharacterSelectScreenManager.ActivePickers.Count == 0)
						{
							EmitSignal(SignalName.TellPlayerCharacterSelectScreenToGoToGameRulesScreen);
                        }
					}
				}

				//See if user wants to go back to activate a picker, which sets up establishing the device with the picker and selected character
				ActivatePickerAndEstablishDevice();
			}
			else
			{
				//Character selection can now occur
				if (CurrentPickerIsActivated && CurrentDeviceId != "-1")
				{
					//Timer exists because if it didn't, the user would hold right or left down and jump through hundreds of character selections at once. 
					//Timer makes it so that user can only move the cursor every so often, but its only 20 frames, which is still super small, just not instantaneous anymore
					if (_playerSelectionChangedRecently)
					{
						_playerSelectionChangeTimer++;
					}

					if (_playerSelectionChangeTimer >= _playerSelectionChangeTimerMax)
					{
						_playerSelectionChangeTimer = 0;
						_playerSelectionChangedRecently = false;
					}

					//Finish character selection for all players (activates confirmation button)
					if (SelectionHasBeenMade && !_playerSelectionChangedRecently &&
						(Input.IsActionJustPressed($"SouthButton_{CurrentDeviceId}") || Input.IsActionJustPressed($"StartButton_{CurrentDeviceId}")))
					{
						_playerSelectionChangedRecently = true;

						//FIX THIS//-----------------------------------------------------------------------------------------
						//This is where the warning is coming from...
                        EmitSignal(SignalName.TellPlayerCharacterSelectScreenToGoToDungeonLevelSwapper);
                    }

					//Select character
					if (!SelectionHasBeenMade)
					{
						if (!_playerSelectionChangedRecently)
						{
							ChangeCharacterChoice();
						}

						if (_isSelectionEnabled && !_playerSelectionChangedRecently && !SelectionHasBeenMade && !_isClassAlreadySelectedByOtherPickers
							&& (Input.IsActionJustPressed($"SouthButton_{CurrentDeviceId}") || Input.IsActionJustPressed($"StartButton_{CurrentDeviceId}")))
						{
							_playerSelectionChangedRecently = true;
							SelectionHasBeenMade = true;
							PickerSprite.Modulate = _selectedTone;

							//EmitSignal(SignalName.PlayerSelectionOccurred, PickerSprite, SelectionHasBeenMade);
						}
					}

					//Deselect character, but still keep current picker active
					if (SelectionHasBeenMade)
					{
						if (!_playerSelectionChangedRecently && SelectionHasBeenMade
							&& Input.IsActionJustPressed($"EastButton_{CurrentDeviceId}"))
						{
							_playerSelectionChangedRecently = true;
							SelectionHasBeenMade = false;
							PickerSprite.Modulate = _defaultTone;

							//EmitSignal(SignalName.PlayerSelectionOccurred, PickerSprite, SelectionHasBeenMade);
						}
					}

					//Deactivate picker 
					if (!_playerSelectionChangedRecently && CurrentDeviceId != "-1" && Input.IsActionJustPressed($"EastButton_{CurrentDeviceId}"))
					{
						_playerSelectionChangedRecently = true;

						Texture2D newTexture = ResourceLoader.Load(PlayerManager.DefaultPickerImageOption) as Texture2D;
						PickerSprite.Texture = newTexture;

                        _playerCharacterSelectScreenManager.ActivePickers.Remove(this);

						CurrentPickerIsActivated = false;

						SelectionHasBeenMade = false;

						CurrentDeviceId = "-1";

						PickerSprite.Modulate = _defaultTone;
					}
				}
			}
		}

		//Activating the picker for the first time - establishes which device activated it
		private void ActivatePickerAndEstablishDevice()
		{
			bool isWakeUp0 = false, isWakeUp1 = false, isWakeUp2 = false, isWakeUp3 = false, isWakeUpKeyboard = false;

			if (_playerCharacterSelectScreenManager.ActivePickers.Count(x => x.CurrentDeviceId == "0") == 0)
			{
				isWakeUp0 = Input.IsActionJustPressed($"SouthButton_0") || Input.IsActionJustPressed($"StartButton_0");
			}
			if (_playerCharacterSelectScreenManager.ActivePickers.Count(x => x.CurrentDeviceId == "1") == 0)
			{
				isWakeUp1 = Input.IsActionJustPressed($"SouthButton_1") || Input.IsActionJustPressed($"StartButton_1");
			}
			if (_playerCharacterSelectScreenManager.ActivePickers.Count(x => x.CurrentDeviceId == "2") == 0)
			{
				isWakeUp2 = Input.IsActionJustPressed($"SouthButton_2") || Input.IsActionJustPressed($"StartButton_2");
			}
			if (_playerCharacterSelectScreenManager.ActivePickers.Count(x => x.CurrentDeviceId == "3") == 0)
			{
				isWakeUp3 = Input.IsActionJustPressed($"SouthButton_3") || Input.IsActionJustPressed($"StartButton_3");
			}
			if (_playerCharacterSelectScreenManager.ActivePickers.Count(x => x.CurrentDeviceId == "Keyboard") == 0)
			{
				isWakeUpKeyboard = Input.IsActionJustPressed($"SouthButton_Keyboard") || Input.IsActionJustPressed($"StartButton_Keyboard");
			}

			if (isWakeUp0 || isWakeUp1 || isWakeUp2 || isWakeUp3 || isWakeUpKeyboard)
			{
				if (isWakeUp0)
				{
					CurrentDeviceId = "0";
				}
				else if (isWakeUp1)
				{
					CurrentDeviceId = "1";
				}
				else if (isWakeUp2)
				{
					CurrentDeviceId = "2";
				}
				else if (isWakeUp3)
				{
					CurrentDeviceId = "3";
				}
				else if (isWakeUpKeyboard)
				{
					CurrentDeviceId = "Keyboard";
				}

				#region Initial Texture Setting
				Texture2D startingTexture = null;

				if (_playerCharacterSelectScreenManager.ActivePickers.Count > 0)
				{
					foreach (string imageName in PlayerManager.AvailablePlayerImageOptions)
					{
						Texture2D textureToCheck = ResourceLoader.Load(imageName) as Texture2D;

						if (_playerCharacterSelectScreenManager.ActivePickers.Any(x => x.SelectionHasBeenMade && x.PickerSprite.Texture == textureToCheck))
						{
							continue;
						}
						else
						{
							startingTexture = textureToCheck;
							
							break;
						}
					}
				}

				if (startingTexture == null)
				{
					startingTexture = ResourceLoader.Load(PlayerManager.AvailablePlayerImageOptions[0]) as Texture2D;
				}

                PickerSprite.Modulate = _defaultTone;

                PickerSprite.Texture = startingTexture;
				#endregion

				CurrentPickerIsActivated = true;
				_playerSelectionChangedRecently = true;
                _playerCharacterSelectScreenManager.ActivePickers.Add(this);

				GD.Print($"Added device: {CurrentDeviceId}");
			}
		}

		//Move picker over new character icon, going right or left
		private void ChangeCharacterChoice()
		{
			//Want to limit classes based on which ones are already selected

			if (Input.IsActionPressed($"MoveEast_{CurrentDeviceId}"))
			{
				int matchingIndex = PlayerManager.AvailablePlayerImageOptions.IndexOf(PickerSprite.Texture.ResourcePath);

				int lookupIndex;

				if (matchingIndex == PlayerManager.AvailablePlayerImageOptions.Count - 1 || matchingIndex == -1)
				{
					lookupIndex = 0;
				}
				else
				{
					lookupIndex = matchingIndex + 1;
				}

				Texture2D newTexture = ResourceLoader.Load(PlayerManager.AvailablePlayerImageOptions[lookupIndex]) as Texture2D;

				PickerSprite.Texture = newTexture;

				_playerSelectionChangedRecently = true;

                PickerSprite.Modulate = _defaultTone;
            }
			else if (Input.IsActionPressed($"MoveWest_{CurrentDeviceId}"))
			{
				int matchingIndex = PlayerManager.AvailablePlayerImageOptions.IndexOf(PickerSprite.Texture.ResourcePath);

				int lookupIndex;

				if (matchingIndex == 0 || matchingIndex == -1)
				{
					lookupIndex = PlayerManager.AvailablePlayerImageOptions.Count - 1;
				}
				else
				{
					lookupIndex = matchingIndex - 1;
				}

				Texture2D newTexture = ResourceLoader.Load(PlayerManager.AvailablePlayerImageOptions[lookupIndex]) as Texture2D;

				PickerSprite.Texture = newTexture;

				_playerSelectionChangedRecently = true;

                PickerSprite.Modulate = _defaultTone;
            }
		}
	}
}
