using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using System;
using System.Collections.Generic;

public partial class SplitScreenManager : CanvasLayer
{
	private List<SubViewport> _subViewports = new List<SubViewport>();
	private List<Camera2D> _subViewportCameras = new List<Camera2D>();
	private Node _level = null;

	public override void _Ready()
	{
		_subViewports = GetSubViewports();
		_level = _subViewports[0].GetNode("Level");
		SetSubViewportWorlds();
		SetCamerasToPlayers();


		GD.Print($"SubViewports: {_subViewports.Count}");

		AdjustPlayerCameraView();
	}

	public override void _Process(double delta)
	{
	}

	private List<SubViewport> GetSubViewports()
	{
		GridContainer gridContainer = GetNode<GridContainer>("GridContainer");

		List<SubViewport> subViewports = new List<SubViewport>();

		foreach (var child in gridContainer.GetChildren())
		{
			if (child is SubViewportContainer)
			{
				SubViewport subViewport = child.GetNode("SubViewport") as SubViewport;

				subViewports.Add(subViewport);

				_subViewportCameras.Add(subViewport.GetNode<Camera2D>("Camera2D"));
			}
		}

		return subViewports;
	}

	private void SetSubViewportWorlds()
	{
		foreach (SubViewport subViewport in _subViewports)
		{
			subViewport.World2D = _subViewports[0].World2D;
		}
	}

	private void SetCamerasToPlayers()
	{
		int playerCount = 0;

		foreach (BaseCharacter player in PlayerManager.ActivePlayers)
		{
			player.playerCamera = _subViewportCameras[playerCount];

			playerCount++;
		}
	}

	private void AdjustPlayerCameraView()
	{
		Vector2I mainViewportSize = GetWindow().Size;

		if (PlayerManager.ActivePlayers.Count == 1)
		{
			_subViewports[0].Size = mainViewportSize;
		}
		else if (PlayerManager.ActivePlayers.Count == 2)
		{
			_subViewports[0].Size = new Vector2I((mainViewportSize.X/2), mainViewportSize.Y);
			_subViewports[1].Size = new Vector2I((mainViewportSize.X/2), mainViewportSize.Y);
		}
		else if (PlayerManager.ActivePlayers.Count == 3)
		{
			_subViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[3].Size = new Vector2I(0, 0);
		}
		else if (PlayerManager.ActivePlayers.Count == 4)
		{
			_subViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			_subViewports[3].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
		}

		//if (PlayerNumber == 1)
		//{

		//}
		//else if (PlayerNumber == 2)
		//{

		//}
		//else if (PlayerNumber == 3)
		//{

		//}
		//else if (PlayerNumber == 3)
		//{

		//}
	}
}
