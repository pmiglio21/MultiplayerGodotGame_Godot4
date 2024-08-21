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

	private List<TileMapSpace> _existingFloorGridSpaces = new List<TileMapSpace>();

	private int _maxNumberOfTiles = 0;

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Key Level Object Generation

	private PackedScene _portalScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/Portal/Portal.tscn");

	private PackedScene _portalSwitchScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/PortalSwitch/PortalSwitch.tscn");

	#endregion

	#region Component Nodes

	private Node _baseFloor = null;

	private TileMap _tileMap = null;

	#endregion

	public override void _Ready()
	{
        _rng.Randomize();

		_baseFloor = this.GetNode<Node>("BaseFloor");

		_tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		LoadInFloorTiles();

		_floorTileList = _tileMap.GetUsedCellsById(0, TileMappingMagicNumbers.TileMapCaveFloorSpriteId).ToList();

        RunProceduralPathGeneration();
	}

	#region Floor Generation

	private void LoadInFloorTiles()
    {
        if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.VerySmall)
        {
            _maxNumberOfTiles = 20;
        }
        if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.Small)
        {
            _maxNumberOfTiles = 40;
        }
        if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.Medium)
        {
            _maxNumberOfTiles = 60;
        }
        if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.Large)
        {
            _maxNumberOfTiles = 80;
        }
        if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.VeryLarge)
        {
            _maxNumberOfTiles = 100;
        }
        else if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.Colossal)
        {
            _maxNumberOfTiles = 120;
        }
        else if (CurrentSaveGameRules.CurrentLevelSize == LevelSize.Varied)
        {
            var floorSizeOptions = new List<int>() { 20, 40, 60, 80, 100, 120 };

            _maxNumberOfTiles = floorSizeOptions[_rng.RandiRange(0, floorSizeOptions.Count - 1)];
        }

        int x = 0;

        while (x < _maxNumberOfTiles)
        {
            int y = 0;

            while (y < _maxNumberOfTiles)
            {
                //How to make this dynamic? Need to find way to access atlas size.
                var xAtlasCoord = _rng.RandiRange(0, 3);
                var yAtlasCoord = _rng.RandiRange(0, 1);

                _tileMap.SetCell(0, new Vector2I(x, y), TileMappingMagicNumbers.TileMapCaveFloorSpriteId, new Vector2I(0, 0));

                y++;
            }

            x++;
        }
    }

	#endregion


	private void RunProceduralPathGeneration()
	{
		GenerateInteriorBlocksOnAllFloorTiles();

		if (CurrentSaveGameRules.CurrentRelativePlayerSpawnDistanceType == RelativePlayerSpawnDistanceType.SuperClose)
		{
			GenerateSingleSpawnPoints();
		}
		else
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

		#region Spawn Key Objects
		GenerateKeyMapItems();

		if (CurrentSaveGameRules.CurrentRelativePlayerSpawnDistanceType == RelativePlayerSpawnDistanceType.SuperClose)
		{
			SpawnPlayersClose();
		}
		else
		{
			SpawnPlayersNormal();
		}
		#endregion

        PaintInteriorWalls();
    }

    #region Procedural Path Generation
    //Do this in the same loop as creating floor tiles
    private void GenerateInteriorBlocksOnAllFloorTiles()
	{
		foreach (var tilePosition in _floorTileList)
		{
			GenerateInteriorBlock(tilePosition.X, tilePosition.Y);
        }
	}

    #region Spawn Point Generation

    private void GenerateMultipleSpawnPoints()
    {
        for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < PlayerManager.ActivePlayers.Count; spawnPointGeneratedCount++)
        {
            var floorTileIndex = _rng.RandiRange(0, _floorTileList.Count - 1);

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
                    if (currentFloorGridSpace.TileMapPosition.X != 0 && currentFloorGridSpace.TileMapPosition.Y != 0 
                        && currentFloorGridSpace.TileMapPosition.X != _maxNumberOfTiles - 1 && currentFloorGridSpace.TileMapPosition.Y != _maxNumberOfTiles - 1
                        && floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= TileMappingMagicNumbers.DiagonalDistanceBetweenInteriorBlocks)
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
        var floorTileIndex = _rng.RandiRange(0, _floorTileList.Count - 1);

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
        List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

        foreach (TileMapSpace startingSpawnPoint in spawnPoints)
        {
            TileMapSpace walkingFloorSpace = startingSpawnPoint;

            List<TileMapSpace> availableSpawnPoints = spawnPoints.Where(x => x != startingSpawnPoint).ToList();

            var targetSpawnPoint = availableSpawnPoints[_rng.RandiRange(0, availableSpawnPoints.Count - 1)];

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

                var newPositionToCheck = new Vector2I(walkingFloorSpace.TileMapPosition.X + changeInX, walkingFloorSpace.TileMapPosition.Y + changeInY);

                //Set walkingFloorSpace to somewhere adjacent to spawn
                //Instead of using any, just check that new x and y are within the max dimension range
                if (newPositionToCheck.X != 0 && newPositionToCheck.Y != 0 && newPositionToCheck.X != _maxNumberOfTiles - 1 && newPositionToCheck.Y != _maxNumberOfTiles - 1 &&
                    _existingFloorGridSpaces.Any(x => x.TileMapPosition == _tileMap.LocalToMap(newPositionToCheck)))
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

    #endregion

    private void CreatePathsBetweenPoints()
    {
        var availableStartSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion()).ToList();

        TileMapSpace startingPoint = availableStartSpaces[_rng.RandiRange(0, availableStartSpaces.Count - 1)];

        TileMapSpace walkingFloorSpace = startingPoint;

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

            var newPositionToCheck = new Vector2I(walkingFloorSpace.TileMapPosition.X + changeInX, walkingFloorSpace.TileMapPosition.Y + changeInY);

            //Set walkingFloorSpace to somewhere adjacent to spawn
            if (newPositionToCheck.X != 0 && newPositionToCheck.Y != 0 && newPositionToCheck.X != _maxNumberOfTiles - 1 && newPositionToCheck.Y != _maxNumberOfTiles - 1 && 
                _existingFloorGridSpaces.Any(x => x.TileMapPosition == _tileMap.LocalToMap(newPositionToCheck)))
            {
                var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == newPositionToCheck);

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

	private void GenerateInteriorBlock(int x, int y)
	{
        //Generate InteriorWallBlock at that position
        var tempBlock = _interiorBlockScene.Instantiate();
        var interiorBlock = tempBlock as Node2D;
        var interiorWallBlock = tempBlock as InteriorWallBlock;
        AddChild(interiorBlock);

        var tempTestText = _interiorBlockTestTextScene.Instantiate();
        var testText = tempTestText as Node2D;
        AddChild(testText);

        var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(new Vector2I(x, y)));
        interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);
        testText.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

        _existingFloorGridSpaces.Add(new TileMapSpace() { InteriorBlock = interiorBlock, TestText = testText, TileMapPosition = new Vector2I(x, y) });
    }

	private void PaintInteriorWalls()
	{
        foreach (TileMapSpace tileMapSpace in _existingFloorGridSpaces)
		{
			if (tileMapSpace.NumberOfSpawnPointWhoClearedIt == -1)
			{
                var interiorBlockSprite = tileMapSpace.InteriorBlock.FindChild("Sprite2D") as Sprite2D;

                Texture2D newTexture = ResourceLoader.Load("res://Levels/OverworldLevels/TileMapping/InteriorWalls/CaveWall_2.png") as Texture2D;
                interiorBlockSprite.Texture = newTexture;

    //            var xAtlasCoord = 0;
				//var yAtlasCoord = 0;

                TileMapSpace northBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X, tileMapSpace.TileMapPosition.Y - 1));
                TileMapSpace northEastBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X + 1, tileMapSpace.TileMapPosition.Y - 1));
                TileMapSpace eastBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X + 1, tileMapSpace.TileMapPosition.Y));
                TileMapSpace southEastBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X + 1, tileMapSpace.TileMapPosition.Y + 1));
                TileMapSpace southBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X, tileMapSpace.TileMapPosition.Y + 1));
                TileMapSpace southWestBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X - 1, tileMapSpace.TileMapPosition.Y + 1));
                TileMapSpace westBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X - 1, tileMapSpace.TileMapPosition.Y));
                TileMapSpace northWestBlock = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == new Vector2I(tileMapSpace.TileMapPosition.X - 1, tileMapSpace.TileMapPosition.Y - 1));

                //Block opens to at least the south
                if ((southBlock != null && southBlock.NumberOfSpawnPointWhoClearedIt != -1))
                {
                    //xAtlasCoord = 1;
                    //yAtlasCoord = 7;

                    Texture2D newTexture2 = ResourceLoader.Load("res://Levels/OverworldLevels/TileMapping/InteriorWalls/CaveWall_1.png") as Texture2D;
                    interiorBlockSprite.Texture = newTexture2;
                }

                if (northBlock != null && northBlock.NumberOfSpawnPointWhoClearedIt != -1)
                {
                    var collisionShape = tileMapSpace.InteriorBlock.FindChild("CollisionShape2D") as CollisionShape2D;

                    collisionShape.Shape = new RectangleShape2D() { Size = new Vector2(32, 16) };
                    collisionShape.Position = new Vector2(0, 8);
                }



                //_tileMap.SetCell(0, tileMapSpace.TileMapPosition, TileMappingMagicNumbers.TileMapCaveWallSpriteId, new Vector2I(xAtlasCoord, yAtlasCoord));
            }
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

	private void SpawnPlayersClose()
	{
		List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

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

		GD.Print("---------------------------------------");

		PlayerCharacterPickerManager.ActivePickers.Clear();
	}

	private void SpawnPlayersNormal()
	{
		List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		int playerCount = 0;

		foreach (BaseCharacter character in PlayerManager.ActivePlayers)
		{
			GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

			character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

			AddChild(character);

			playerCount++;
		}

		GD.Print("---------------------------------------");

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
