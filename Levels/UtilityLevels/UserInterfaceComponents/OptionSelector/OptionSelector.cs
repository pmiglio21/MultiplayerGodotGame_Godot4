using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enums;
using Enums.GameRules;
using Globals;
using Godot;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
	public partial class OptionSelector : Control
	{
		#region Exported Properties
		[Export]
		public string LabelText = string.Empty;

		[Export]
		public OptionSelectorType OptionSelectorType = OptionSelectorType.None;
		#endregion

		#region Components
		private Label _optionLabel;
		private Button _optionButton;
		#endregion

		private List<string> _options = new List<string>();

		public override void _Ready()
		{
			_optionLabel = GetNode<Label>("OptionLabel");
			_optionLabel.Text = LabelText;

			_optionButton = GetNode<Button>("OptionButton");

			SetupOptionsList();

			_optionButton.Text = _options[0];
		}

		public override void _Process(double delta)
		{
			if (_options.Count > 1 && _optionButton.HasFocus())
			{
				if (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast))
				{
					int indexOfCurrentOptionButtonText = _options.IndexOf(_optionButton.Text);

					if (indexOfCurrentOptionButtonText == _options.Count - 1)
					{
						_optionButton.Text = _options[0];
					}
					else
					{
						_optionButton.Text = _options[indexOfCurrentOptionButtonText + 1];
					}
				}
				else if (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest))
				{
					int indexOfCurrentOptionButtonText = _options.IndexOf(_optionButton.Text);

					if (indexOfCurrentOptionButtonText == 0)
					{
						_optionButton.Text = _options[_options.Count - 1];
					}
					else
					{
						_optionButton.Text = _options[indexOfCurrentOptionButtonText - 1];
					}
				}
			}
		}

		private void SetupOptionsList()
		{
			switch (OptionSelectorType)
			{
				case OptionSelectorType.None:

					_options = new List<string>();
					
					break;

				case OptionSelectorType.SplitScreenMergingType:

					foreach (var enumValue in Enum.GetValues(typeof(SplitScreenMergingType)))
					{
						var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

						if (enumDescription != "None")
						{
							_options.Add(enumDescription);
						}
					}

					break;

				case OptionSelectorType.RelativePlayerSpawnDistance:

					foreach (var enumValue in Enum.GetValues(typeof(RelativePlayerSpawnDistanceType)))
					{
						var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

						if (enumDescription != "None")
						{
							_options.Add(enumDescription);
						}
					}

					break;

				case OptionSelectorType.LevelSize:

					foreach (var enumValue in Enum.GetValues(typeof(LevelSize)))
					{
						var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

						if (enumDescription != "None")
						{
							_options.Add(enumDescription);
						}
					}

					break;

				default:

					_options = new List<string>();

					break;
			}
		}

		public Button GetOptionButton()
		{
			return _optionButton;
		}
	}
}
