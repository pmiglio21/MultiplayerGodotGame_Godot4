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

        //AdjustPlayerCameraView();
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
		if (PlayerManager.ActivePlayers.Count == 1)
		{
			_subViewports[0].Size = new Vector2I(100, 100);

			//PlayerManager.ActivePlayers[0].playerCamera = _subViewports[0].GetCamera2D();
			//playerCamera.Zoom = new Vector2(5, 5);
		}
		else if (PlayerManager.ActivePlayers.Count == 2)
		{

		}
		else if (PlayerManager.ActivePlayers.Count == 3)
		{

		}
		else if (PlayerManager.ActivePlayers.Count == 4)
		{

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
