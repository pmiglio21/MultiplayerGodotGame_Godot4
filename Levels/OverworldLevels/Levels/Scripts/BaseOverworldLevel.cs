using Globals;
using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using MultiplayerGodotGameGodot4.Levels.OverworldLevels.TileMapping;
using Scenes.UI.PlayerSelectScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class BaseOverworldLevel : Node
{
	#region TileMap Level Generation
	
	private List<Vector2I> _floorTileList = new List<Vector2I>();
	
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private List<TileMapFloorGridSpace> _existingFloorGridSpaces = new List<TileMapFloorGridSpace>();

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Key Level Object Generation

	private PackedScene _portalScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/Portal/Portal.tscn");

	private PackedScene _portalSwitchScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/PortalSwitch/PortalSwitch.tscn");

	#endregion

	#region Component Nodes

	private Node2D _baseFloor = null;

	private TileMap _tileMap = null;

	#endregion

	public override void _Ready()
	{
		_baseFloor = GetNode<Node2D>("BaseFloor");

		_tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		_floorTileList = _tileMap.GetUsedCellsById(0, TileMappingMagicNumbers.TileMapFloorSpriteId).ToList();

		GenerateInteriorWallsForLevel();
	}

	private void GenerateInteriorWallsForLevel()
	{
		_rng.Randomize();

		GenerateInteriorBlocksOnAllFloorTiles();

		if (CurrentSaveGameRules.CurrentGameType == GameType.LocalCompetitive)
		{
			if (PlayerManager.ActivePlayers.Count > 1)
			{
				GenerateMultipleSpawnPoints();

				CreatePathsBetweenSpawnPoints();
			}
			else
			{
				GenerateSingleSpawnPoints();
			}
		}
		else if (CurrentSaveGameRules.CurrentGameType == GameType.LocalCoop)
		{
			GenerateSingleSpawnPoints();
		}

		float percentageOfFloorToCover = 0;

		switch (PlayerManager.ActivePlayers.Count)
		{
			case 1:
				percentageOfFloorToCover = .125f;
				break;
			case 2:
				percentageOfFloorToCover = .25f;
				break;
			case 3:
				percentageOfFloorToCover = .333f;
				break;
			case 4:
				percentageOfFloorToCover = .4f;
				break;
			default:
				percentageOfFloorToCover = .25f;
				break;
		}

		//TODO: Get this to work concurrently
		while (_existingFloorGridSpaces.Count(x => x.InteriorBlock.IsQueuedForDeletion()) < (percentageOfFloorToCover * _existingFloorGridSpaces.Count))
		{
			CreatePathsBetweenPoints();
		}

		GenerateKeyMapItems();

		if (CurrentSaveGameRules.CurrentGameType == GameType.LocalCompetitive)
		{
			SpawnLocalCompetitivePlayers();
		}
		else if (CurrentSaveGameRules.CurrentGameType == GameType.LocalCoop)
		{
			SpawnLocalCoopPlayers();
		}
	}

	#region Path Generation
	private void GenerateInteriorBlocksOnAllFloorTiles()
	{
		foreach (var tilePosition in _floorTileList)
		{
			//Generate InteriorWallBlock at that position
			var tempBlock = _interiorBlockScene.Instantiate();
			var interiorBlock = tempBlock as Node2D;
			AddChild(interiorBlock);

			var tempTestText = _interiorBlockTestTextScene.Instantiate();
			var testText = tempTestText as Node2D;
			AddChild(testText);

			var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(new Vector2I(tilePosition.X, tilePosition.Y)));
			interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);
			testText.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

			_existingFloorGridSpaces.Add(new TileMapFloorGridSpace() { InteriorBlock = interiorBlock, TestText = testText });
		}
	}

	private void GenerateMultipleSpawnPoints()
	{
		for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < PlayerManager.ActivePlayers.Count; spawnPointGeneratedCount++)
		{
			var floorTileIndex = (int)Math.Floor(_rng.RandfRange(0, _floorTileList.Count));

			var currentFloorTileGridMapPosition = _floorTileList[floorTileIndex];

			if (_existingFloorGridSpaces.Any(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition)))
			{
				var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition));

				floorGridSpaceWithMatchingPosition.IsSpawnPoint = true;

				floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
				floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt = spawnPointGeneratedCount;

				var richTextLabel = floorGridSpaceWithMatchingPosition.TestText.GetNode("RichTextLabel") as RichTextLabel;
				richTextLabel.Text = spawnPointGeneratedCount.ToString();

				//Clear out spawn point areas
				foreach (var currentFloorGridSpace in _existingFloorGridSpaces)
				{
					if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= TileMappingMagicNumbers.DiagonalDistanceBetweenInteriorBlocks)
					{
						currentFloorGridSpace.InteriorBlock.QueueFree();
						currentFloorGridSpace.NumberOfSpawnPointWhoClearedIt = spawnPointGeneratedCount;

						var rtl = currentFloorGridSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = spawnPointGeneratedCount.ToString();
					}
				}
			}
		}
	}

	private void GenerateSingleSpawnPoints()
	{
		var floorTileIndex = (int)Math.Floor(_rng.RandfRange(0, _floorTileList.Count));

		var currentFloorTileGridMapPosition = _floorTileList[floorTileIndex];

		if (_existingFloorGridSpaces.Any(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition)))
		{
			var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition));

			floorGridSpaceWithMatchingPosition.IsSpawnPoint = true;

			floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
			floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt = 0;

			var richTextLabel = floorGridSpaceWithMatchingPosition.TestText.GetNode("RichTextLabel") as RichTextLabel;
			richTextLabel.Text = "0";

			//Clear out spawn point areas
			foreach (var currentFloorGridSpace in _existingFloorGridSpaces)
			{
				if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= TileMappingMagicNumbers.DiagonalDistanceBetweenInteriorBlocks)
				{
					currentFloorGridSpace.InteriorBlock.QueueFree();
					currentFloorGridSpace.NumberOfSpawnPointWhoClearedIt = 0;

					var rtl = currentFloorGridSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
					rtl.Text = "0";
				}
			}
		}
	}

	//TODO: Check if this method and the other one are similar enough to just make into one method
	private void CreatePathsBetweenSpawnPoints()
	{
		List<TileMapFloorGridSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		foreach (TileMapFloorGridSpace startingSpawnPoint in spawnPoints)
		{
			TileMapFloorGridSpace walkingFloorSpace = startingSpawnPoint;

			List<TileMapFloorGridSpace> availableSpawnPoints = spawnPoints.Where(x => x != startingSpawnPoint).ToList();

			var targetSpawnPoint = availableSpawnPoints[_rng.RandiRange(0, availableSpawnPoints.Count-1)];

			//For some reason, trig circle is flipped across its y axis... whatever...
			var angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

			var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetSpawnPoint);

			var changeInX = 0;
			var changeInY = 0;

			int iterationCount = 0;

			//Do this until the walkingFloorSpace is no longer on the starting spawnPoint
			while (true)
			{
				if (iterationCount < TileMappingMagicNumbers.NumberOfIterationsBeforeChangingAngle)
				{
					angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

					weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetSpawnPoint);

					iterationCount = 0;
				}

				if (_rng.RandfRange(0, 1) <= .5)
				{
					changeInX = SetRandomChangeInDirection(weightedValues.Item1);

					if (changeInX == 0)
					{
						changeInY = SetRandomChangeInDirection(weightedValues.Item2);
					}
					else
					{
						changeInY = 0;
					}
				}
				else
				{
					changeInY = SetRandomChangeInDirection(weightedValues.Item2);

					if (changeInY == 0)
					{
						changeInX = SetRandomChangeInDirection(weightedValues.Item1);
					}
					else
					{
						changeInX = 0;
					}
				}

				var newPositionToCheck = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

				//Set walkingFloorSpace to somewhere adjacent to spawn
				if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPositionToCheck)))
				{
					var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPositionToCheck);

					if (floorGridSpaceWithMatchingPosition != null && floorGridSpaceWithMatchingPosition != startingSpawnPoint)
					{
						if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
						{
							floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
						}

						var numberOfSpawnPointWhoClearedMatchingFloorSpace = floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt;

						walkingFloorSpace = floorGridSpaceWithMatchingPosition;
						walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;

						var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

						if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
							numberOfSpawnPointWhoClearedMatchingFloorSpace != startingSpawnPoint.NumberOfSpawnPointWhoClearedIt)
						{
							break;
						}
					}
				}

				iterationCount++;
			}
		}
	}

	private void CreatePathsBetweenPoints()
	{
		var availableStartSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion()).ToList();

		TileMapFloorGridSpace startingPoint = availableStartSpaces[_rng.RandiRange(0, availableStartSpaces.Count - 1)];

		TileMapFloorGridSpace walkingFloorSpace = startingPoint;

		var availableTargetSpaces = _existingFloorGridSpaces.Where(x => !x.InteriorBlock.IsQueuedForDeletion()).ToList();

		var targetPoint = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)];

		//For some reason, trig circle is flipped across its y axis... whatever...
		var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetPoint.InteriorBlock.GlobalPosition);

		var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetPoint);

		var changeInX = 0;
		var changeInY = 0;

		int iterationCount = 0;

		//Do this until the walkingFloorSpace is no longer on the starting spawnPoint
		while (iterationCount < TileMappingMagicNumbers.NumberOfIterationsBeforeChangingAngle)
		{
			if (_rng.RandfRange(0, 1) <= .5)
			{
				changeInX = SetRandomChangeInDirection(weightedValues.Item1);

				if (changeInX == 0)
				{
					changeInY = SetRandomChangeInDirection(weightedValues.Item2);
				}
				else
				{
					changeInY = 0;
				}
			}
			else
			{
				changeInY = SetRandomChangeInDirection(weightedValues.Item2);

				if (changeInY == 0)
				{
					changeInX = SetRandomChangeInDirection(weightedValues.Item1);
				}
				else
				{
					changeInX = 0;
				}
			}

			var newPosition = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

			//Set walkingFloorSpace to somewhere adjacent to spawn
			if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPosition)))
			{
				var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPosition);

				if (floorGridSpaceWithMatchingPosition != null && floorGridSpaceWithMatchingPosition != startingPoint)
				{
					if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
					{
						floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
					}

					var numberOfSpawnPointWhoClearedMatchingFloorSpace = floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt;

					walkingFloorSpace = floorGridSpaceWithMatchingPosition;
					walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = 99;

					var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
					rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

					if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
						numberOfSpawnPointWhoClearedMatchingFloorSpace != 99)
					{
						break;
					}
				}
			}

			iterationCount++;
		}
	}

	#endregion

	#region Key Object Generation

	private void GenerateKeyMapItems()
	{
		GeneratePortal();

		GenerateSwitches();
	}

	private void GeneratePortal() 
	{
		var tempPortal = _portalScene.Instantiate();
		var portal = tempPortal as Node2D;
		AddChild(portal);

		var availableTargetSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion() && !x.IsSpawnPoint).ToList();

		//Need to match to make sure portals and switches don't spawn on each other, maybe add "holding something flag" to tile class
		portal.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].InteriorBlock.GlobalPosition;
	}

	private void GenerateSwitches() 
	{
		for (int i = 0; i < 3; i++)
		{
			var tempPortalSwitch = _portalSwitchScene.Instantiate();
			var portalSwitch = tempPortalSwitch as Node2D;
			AddChild(portalSwitch);

			var availableTargetSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion() && !x.IsSpawnPoint).ToList();

			portalSwitch.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].InteriorBlock.GlobalPosition;
		}
	}

	private void SpawnLocalCoopPlayers()
	{
		List<TileMapFloorGridSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		int playerCount = 0;

		foreach (BaseCharacter character in PlayerManager.ActivePlayers)
		{
			if (playerCount == 0)
			{
				character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

				AddChild(character);
			}
			else
			{
				float changeInX = _rng.RandfRange(-30, 30);
				float changeInY = _rng.RandfRange(-30, 30);

				character.GlobalPosition = new Vector2(PlayerManager.ActivePlayers[0].GlobalPosition.X + changeInX, PlayerManager.ActivePlayers[0].GlobalPosition.Y + changeInY);

				AddChild(character);
			}

			GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

			playerCount++;
		}

		PlayerCharacterPickerManager.ActivePickers.Clear();
	}

	private void SpawnLocalCompetitivePlayers()
	{
		List<TileMapFloorGridSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		int playerCount = 0;

		foreach (BaseCharacter character in PlayerManager.ActivePlayers)
		{
			GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

			character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

			AddChild(character);

			playerCount++;
		}

		PlayerCharacterPickerManager.ActivePickers.Clear();
	}

	#endregion

	#region Direction-Changing Utility Methods

	private int SetRandomChangeInDirection(int weightedValue = 1)
	{
		var randChance = _rng.RandfRange(0, 1);

		if (randChance < .25)
		{
			return -weightedValue;
		}
		else if (randChance >= .25 && randChance < .5)
		{
			return 0;
		}
		else
		{
			return weightedValue;
		}
	}

	private Tuple<int, int> GetWeightedValues(float angleFromWalkingFloorSpaceToTargetSpawnPoint)
	{
		var weightedXValue = 0;
		var weightedYValue = 0;

		if (angleFromWalkingFloorSpaceToTargetSpawnPoint > 0 && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (Math.PI / 2f))
		{
			weightedXValue = 1;
			weightedYValue = 1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint > (Math.PI / 2f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= Math.PI)
		{
			weightedXValue = -1;
			weightedYValue = 1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < 0 && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(Math.PI / 2f))
		{
			weightedXValue = 1;
			weightedYValue = -1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(Math.PI / 2f) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -Math.PI)
		{
			weightedXValue = -1;
			weightedYValue = -1;
		}
		else
		{
			weightedXValue = 1;
			weightedYValue = 1;
		}

		return new Tuple<int, int>(weightedXValue, weightedYValue); ;
	}

	#endregion

}
