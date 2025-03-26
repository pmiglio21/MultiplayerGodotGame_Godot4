using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using System.Collections.Generic;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
	public partial class SplitScreenManager : Control
	{
		private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private List<Camera2D> _subViewportCameras = new List<Camera2D>();
		private List<SubViewport> _availableSubViewports = new List<SubViewport>();
		private List<SubViewportContainer> _availableSubViewportContainers = new List<SubViewportContainer>();

        private bool hasSceneLoaded = false;

		public override void _Ready()
        {
            _parentDungeonLevelSwapper = GetParent() as DungeonLevelSwapper;

            _availableSubViewports = GetSubViewports();
			_availableSubViewportContainers = GetSubViewportContainers();

			SetSubViewportWorlds();

            //Lets subviewports scale as the window changes size
            GetTree().Root.SizeChanged += AdjustScreenPerPlayerCameraView;

            //Call it on its own to set cameras initially
            SetCamerasToPlayers();

            RunScreenAdjustmentProcess();
		}

		public override void _Process(double delta)
		{
			if (!hasSceneLoaded)
			{
                //Do this to load in scene and adjust the screen size right away.
                //Root.SizeChanged event doesn't happen after the scene loads, so we have to adjust the screen ourselves at the start of the scene.
                SetCamerasToPlayers();

                AdjustScreenPerPlayerCameraView();

                hasSceneLoaded = true;
			}
		}

		private List<SubViewport> GetSubViewports()
		{
			List<SubViewport> subViewports = new List<SubViewport>();

			int cameraCount = 1;

			while (cameraCount < 5)
			{
				SubViewport subViewport = FindChild($"SubViewport{cameraCount}") as SubViewport;

				subViewports.Add(subViewport);

				_subViewportCameras.Add(subViewport.GetNode<Camera2D>($"PlayerCamera{cameraCount}"));

				cameraCount++;
			}

			return subViewports;
		}

		private List<SubViewportContainer> GetSubViewportContainers()
		{
			List<SubViewportContainer> subViewportContainers = new List<SubViewportContainer>();

			int cameraCount = 1;

			while (cameraCount < 5)
			{
				SubViewportContainer subViewportContainer = FindChild($"SubViewportContainer{cameraCount}") as SubViewportContainer;

				subViewportContainers.Add(subViewportContainer);

				cameraCount++;
			}

			return subViewportContainers;
		}

		private void RunScreenAdjustmentProcess()
		{
            AdjustScreenPerPlayerCameraView();
        }

		private void SetSubViewportWorlds()
		{
			foreach (SubViewport subViewport in _availableSubViewports)
			{
				subViewport.World2D = _availableSubViewports[0].World2D;
			}
		}

		private void SetCamerasToPlayers()
		{
			int playerCount = 0;

			foreach (BaseCharacter player in _parentDungeonLevelSwapper.ActivePlayers)
			{
				player.playerCamera = _subViewportCameras[playerCount];

				playerCount++;
			}
		}

		private void AdjustScreenPerPlayerCameraView()
		{
            Vector2I mainViewportSize = (Vector2I)GetViewport().GetVisibleRect().Size;
			//Not tree.Root.Size, we need the visible rectangle. tree.Root.Size grabs everything even the distance covered by the resized window's black bars;

            if (_parentDungeonLevelSwapper.ActivePlayers.Count == 1)
			{
				//Remove all but the first SubviewportContainer
				for (int i = 1; i < _availableSubViewportContainers.Count; i++)
				{
					this.RemoveChild(_availableSubViewportContainers[1]);

					_availableSubViewportContainers.RemoveAt(1);
				}

                _availableSubViewportContainers[0].GlobalPosition = Vector2.Zero;
				_availableSubViewportContainers[0].Size = mainViewportSize;
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 2)
			{
				//Remove all but the first and second SubviewportContainer
				for (int i = 2; i < _availableSubViewportContainers.Count; i++)
				{
					this.RemoveChild(_availableSubViewportContainers[2]);

					_availableSubViewportContainers.RemoveAt(2);
				}

                _availableSubViewportContainers[0].GlobalPosition = new Vector2(0, 0);
				_availableSubViewportContainers[0].Size = new Vector2(mainViewportSize.X / 2, mainViewportSize.Y);

				_availableSubViewportContainers[1].GlobalPosition = new Vector2(mainViewportSize.X / 2, 0);
				_availableSubViewportContainers[1].Size = new Vector2(mainViewportSize.X / 2, mainViewportSize.Y);
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 3)
			{
				_availableSubViewportContainers[0].GlobalPosition = new Vector2(0, 0);
				_availableSubViewports[0].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[1].GlobalPosition = new Vector2(mainViewportSize.X / 2, 0);
				_availableSubViewports[1].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[2].GlobalPosition = new Vector2(0, mainViewportSize.Y / 2);
				_availableSubViewports[2].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewports[3].Size = Vector2I.Zero;
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 4)
			{
                _availableSubViewportContainers[0].GlobalPosition = new Vector2(0, 0);
                _availableSubViewports[0].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

                _availableSubViewportContainers[1].GlobalPosition = new Vector2(mainViewportSize.X / 2, 0);
                _availableSubViewports[1].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

                _availableSubViewportContainers[2].GlobalPosition = new Vector2(0, mainViewportSize.Y / 2);
                _availableSubViewports[2].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

                _availableSubViewportContainers[3].GlobalPosition = new Vector2(mainViewportSize.X / 2, mainViewportSize.Y / 2);
                _availableSubViewports[3].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);
			}
		}

		private void AdjustSharedScreenCameraView()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			_availableSubViewports[0].Size = mainViewportSize;
			_availableSubViewports[1].Size = new Vector2I(0, 0);
			_availableSubViewports[2].Size = new Vector2I(0, 0);
			_availableSubViewports[3].Size = new Vector2I(0, 0);

			GD.Print($"Size shared {_availableSubViewports[0].Size}");
		}
    }
}
