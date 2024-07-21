using Enums;
using Globals;
using Godot;
using System;
using System.Collections.Generic;

public partial class NumberSpinner : Control
{
	#region Exported Properties
	[Export]
	public int MinNumber = 5;
	[Export]
	public int MaxNumber = 10;
	[Export]
	public int NumberStep = 1;
	[Export]
	public bool UsesInfinity = false;
	#endregion

	#region Components
	private Button _numberSpinnerButton;
	#endregion

	public override void _Ready()
	{
		_numberSpinnerButton = GetNode<Button>("NumberSpinnerButton");

		_numberSpinnerButton.Text = MinNumber.ToString();
	}

	public override void _Process(double delta)
	{
		if (_numberSpinnerButton.HasFocus())
		{
			int newNumber;

			if (_numberSpinnerButton.Text == GlobalConstants.Infinity)
			{
				newNumber = 0;
			}
			else
			{
				newNumber = int.Parse(_numberSpinnerButton.Text);
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast))
			{
				if (newNumber >= MinNumber)
				{
					newNumber += NumberStep;

					if (newNumber <= MaxNumber)
					{
						_numberSpinnerButton.Text = newNumber.ToString();
					}
					else if (newNumber > MaxNumber && UsesInfinity)
					{
						_numberSpinnerButton.Text = GlobalConstants.Infinity;
					}
				}
			}
			else if (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest))
			{
				if (newNumber == 0 && UsesInfinity)
				{
					_numberSpinnerButton.Text = MaxNumber.ToString();
				}
				else
				{
					newNumber -= NumberStep;

					if (newNumber > MinNumber)
					{
						_numberSpinnerButton.Text = newNumber.ToString();
					}

					else if (newNumber <= MinNumber)
					{
						_numberSpinnerButton.Text = MinNumber.ToString();
					}
				}
			}
		}
	}

	public Button GetNumberSpinnerButtonButton()
	{
		return _numberSpinnerButton;
	}
}
