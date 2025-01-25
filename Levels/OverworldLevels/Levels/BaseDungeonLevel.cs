using Enums;
using Globals;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using Levels.OverworldLevels.TileMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileEntities.Enemies.Scripts;
using Root;

public partial class BaseDungeonLevel : Node
{
	private DungeonLevelSwapper _parentDungeonLevelSwapper;

    #region Signals

    [Signal]
    public delegate void GoToGameOverScreenEventHandler();

    #endregion

    #region TileMap Level Generation

    private Dictionary<int,Vector2I> _possibleFloorPositionsByIndex = new Dictionary<int,Vector2I>();
    private Dictionary<Vector2I, int> _possibleIndicesByFloorPositions = new Dictionary<Vector2I, int>();
    private Dictionary<Vector2I, TileMapSpace> _possibleTileMapSpacesByFloorPosition = new Dictionary<Vector2I, TileMapSpace>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private int _maxNumberOfTiles = 0;

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Key Level Object Generation

	private PackedScene _portalScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/Portal/Portal.tscn");

	private PackedScene _portalSwitchScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/PortalSwitch/PortalSwitch.tscn");

	#endregion

	#region Enemy Generation

	private int _enemyCountMax = 5;
	private int _enemyCount = 0;

	#endregion

	#region Component Nodes

	private Node _baseFloor;

	private TileMap _tileMap;

	private Timer _enemyRespawnTimer;

    #endregion

    #region Selected Game Rules

    public SpawnProximityType SelectedSpawnProximityType;
    public LevelSize SelectedLevelSize;
    public BiomeType SelectedBiomeType;

    #endregion

    public override void _Ready()
	{
		#region Initialize Properties

		_rng.Randomize();

        RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

        _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();

        _baseFloor = this.GetNode<Node>("BaseFloor");

		_tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		_enemyRespawnTimer = this.GetNode<Timer>("EnemyRespawnTimer");

        #endregion

        #region Selected GameRules

        //TODO: Fix this by looking at the number of ENABLED types of each list of enums
        //int spawnProximityTypeIndex = _rng.RandiRange(0, _parentDungeonLevelSwapper.CurrentGameRules.SpawnProximityTypes.Count - 1);
        //SelectedSpawnProximityType = _parentDungeonLevelSwapper.CurrentGameRules.SpawnProximityTypes[spawnProximityTypeIndex];

        //int levelSizeIndex = _rng.RandiRange(0, _parentDungeonLevelSwapper.CurrentGameRules.LevelSizes.Count - 1);
        //SelectedLevelSize = _parentDungeonLevelSwapper.CurrentGameRules.LevelSizes[levelSizeIndex];

        //int biomeTypeIndex = _rng.RandiRange(0, _parentDungeonLevelSwapper.CurrentGameRules.BiomeTypes.Count - 1);
        //SelectedBiomeType = _parentDungeonLevelSwapper.CurrentGameRules.BiomeTypes[biomeTypeIndex];

        #endregion

        SetPossibleFloorTiles();

		RunProceduralPathGeneration();

		while (_enemyCount < _enemyCountMax)
		{
			SpawnEnemy();
		}	 
		_enemyRespawnTimer.Start();
	}

	#region Possible Floor Generation

	private void SetPossibleFloorTiles()
	{
        if (SelectedLevelSize == LevelSize.Small)
		{
			_maxNumberOfTiles = 50;
		}
		else if (SelectedLevelSize == LevelSize.Medium)
		{
			_maxNumberOfTiles = 75;
		}
		else if (SelectedLevelSize == LevelSize.Large)
		{
			_maxNumberOfTiles = 100;
		}

		int overallCounter = 0;

		for (int x = 0; x < _maxNumberOfTiles; x++)
        {
			for (int y = 0; y < _maxNumberOfTiles; y++)
            {
                _possibleFloorPositionsByIndex.Add(overallCounter, new Vector2I(x, y));
                _possibleIndicesByFloorPositions.Add(new Vector2I(x, y), overallCounter);

                overallCounter++;
            }
        }
    }

    #endregion

    private void RunProceduralPathGeneration()
	{
        if (SelectedSpawnProximityType == SpawnProximityType.SuperClose)
		{
			GenerateSingleSpawnPoint();
		}
		else
		{
			if (_parentDungeonLevelSwapper.ActivePlayers.Count > 1)
			{
				GenerateMultipleSpawnPoints();

				CreatePathsBetweenSpawnPoints();
			}
			else
			{
				GenerateSingleSpawnPoint();
			}
		}

		float percentageOfFloorToCover = 0;

		switch (_parentDungeonLevelSwapper.ActivePlayers.Count)
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

		percentageOfFloorToCover = .05f;

        //TODO: Get this to work concurrently
        while (_possibleTileMapSpacesByFloorPosition.Count < (percentageOfFloorToCover * _possibleFloorPositionsByIndex.Count))
		{
			CreatePathsBetweenPoints();
        }

		DrawOverviewAndWalls();

        //Add water
        if (SelectedBiomeType == BiomeType.Frost)
        {
            List<TileMapSpace> possibleWaterSpaces = new List<TileMapSpace>();

            foreach (TileMapSpace tileMapSpace in _possibleTileMapSpacesByFloorPosition.Values)
            {
                if ((!tileMapSpace.IsSpawnPoint && !tileMapSpace.IsAdjacentToSpawnPoint) && tileMapSpace.TileMapSpaceType == TileMapSpaceType.Floor)
                {
                    var adjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(tileMapSpace.TileMapPosition);

                    //if all adjacent floor positions are also floor-type
                    if (adjacentFloorSpacePositions.All(x => 
                                                        (!_possibleTileMapSpacesByFloorPosition[x].IsSpawnPoint && !_possibleTileMapSpacesByFloorPosition[x].IsAdjacentToSpawnPoint) &&
                                                         (_possibleTileMapSpacesByFloorPosition[x].TileMapSpaceType == TileMapSpaceType.Floor ||
                                                          _possibleTileMapSpacesByFloorPosition[x].TileMapSpaceType == TileMapSpaceType.Water)))
                    {
                        if (_rng.RandiRange(1, 100) <= 30)
                        {
                            DrawWater(tileMapSpace);
                        }
                    }
                }
            }
        }
        
        //Remove one-block water
        foreach (TileMapSpace tileMapSpace in _possibleTileMapSpacesByFloorPosition.Values)
        {
            if (tileMapSpace.TileMapSpaceType == TileMapSpaceType.Water)
            {
                var adjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(tileMapSpace.TileMapPosition);

                //if all adjacent map positions are water-type
                if (adjacentFloorSpacePositions.All(x => _possibleTileMapSpacesByFloorPosition[x].TileMapSpaceType == TileMapSpaceType.Floor))
                {
                    tileMapSpace.TileMapSpaceType = TileMapSpaceType.Floor;
                    DrawOnTileMap(tileMapSpace.TileMapPosition);
                }
            }
        }

        #region Spawn Key Objects

        if (SelectedSpawnProximityType == SpawnProximityType.SuperClose)
        {
            SpawnPlayersSuperClose();
        }
        else
        {
            SpawnPlayersNormal();
        }

        GenerateKeyMapObjects();

        #endregion
    }

    #region Procedural Path Generation

    #region Spawn Point Generation

    private void GenerateSingleSpawnPoint(int playerNumber = 0)
    {
		Dictionary<int,Vector2I> nonBorderPositions = new Dictionary<int, Vector2I>();

		int counter = 0;
		foreach (var indexPositionPair in _possibleFloorPositionsByIndex.Where(position => position.Value.X != 0 && position.Value.X != _maxNumberOfTiles - 1 &&
                                                                           position.Value.Y != 0 && position.Value.Y != _maxNumberOfTiles - 1))
		{
			nonBorderPositions.Add(counter, indexPositionPair.Value);
			counter++;
        }

        //Get a random space in possible floor spaces to pick as spawn point
        var floorTileIndex = _rng.RandiRange(0, nonBorderPositions.Count - 1);

        //Create Spawn Point
        var newSpawnPoint_TileMapSpace = new TileMapSpace();

		//This line has a "key not present" issue
        if (_possibleTileMapSpacesByFloorPosition.ContainsKey(nonBorderPositions[floorTileIndex]))
        {
			newSpawnPoint_TileMapSpace = _possibleTileMapSpacesByFloorPosition[nonBorderPositions[floorTileIndex]];
        }
        else
        {
            newSpawnPoint_TileMapSpace = CreateDefaultTileMapSpace(nonBorderPositions[floorTileIndex], TileMapSpaceType.Floor);

            _possibleTileMapSpacesByFloorPosition.Add(nonBorderPositions[floorTileIndex], newSpawnPoint_TileMapSpace);
        }

        newSpawnPoint_TileMapSpace.IsSpawnPoint = true;

        newSpawnPoint_TileMapSpace.InteriorBlock.QueueFree();
        newSpawnPoint_TileMapSpace.NumberOfSpawnPointWhoClearedIt = playerNumber;

        _possibleTileMapSpacesByFloorPosition[newSpawnPoint_TileMapSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = playerNumber;

        var richTextLabel = newSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
        richTextLabel.Text = playerNumber.ToString();

		DrawOnTileMap(newSpawnPoint_TileMapSpace.TileMapPosition);

        //Clear out area near spawn point                     //TODO: Fix this to use GetAllAdjacent()...
        var floorSpacesAdjacentToSpawnPoint = _possibleFloorPositionsByIndex.Values.Where(
            floorSpace => (floorSpace != newSpawnPoint_TileMapSpace.TileMapPosition &&
                          ((Vector2)newSpawnPoint_TileMapSpace.TileMapPosition).DistanceTo(floorSpace) <= Math.Sqrt(2))).ToList();

        foreach (Vector2I floorSpaceAdjacentToSpawnPoint in floorSpacesAdjacentToSpawnPoint)
        {
			if (floorSpaceAdjacentToSpawnPoint.X != 0 && floorSpaceAdjacentToSpawnPoint.X != _maxNumberOfTiles - 1 && floorSpaceAdjacentToSpawnPoint.Y != 0 && floorSpaceAdjacentToSpawnPoint.Y != _maxNumberOfTiles - 1)
			{
                var nearSpawnPoint_TileMapSpace = new TileMapSpace();

                if (_possibleTileMapSpacesByFloorPosition.ContainsKey(floorSpaceAdjacentToSpawnPoint))
                {
                    nearSpawnPoint_TileMapSpace = _possibleTileMapSpacesByFloorPosition[floorSpaceAdjacentToSpawnPoint];
                }
                else
                {
                    nearSpawnPoint_TileMapSpace = CreateDefaultTileMapSpace(floorSpaceAdjacentToSpawnPoint, TileMapSpaceType.Floor);

                    _possibleTileMapSpacesByFloorPosition.Add(floorSpaceAdjacentToSpawnPoint, nearSpawnPoint_TileMapSpace);
                }

                nearSpawnPoint_TileMapSpace.IsAdjacentToSpawnPoint = true;
                nearSpawnPoint_TileMapSpace.InteriorBlock.QueueFree();
                nearSpawnPoint_TileMapSpace.NumberOfSpawnPointWhoClearedIt = playerNumber;

                _possibleTileMapSpacesByFloorPosition[nearSpawnPoint_TileMapSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = playerNumber;

                var rtl = nearSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                rtl.Text = playerNumber.ToString();

                DrawOnTileMap(nearSpawnPoint_TileMapSpace.TileMapPosition);
            }
        }
    }

    private void GenerateMultipleSpawnPoints()
	{
		for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < _parentDungeonLevelSwapper.ActivePlayers.Count; spawnPointGeneratedCount++)
		{
			GenerateSingleSpawnPoint(spawnPointGeneratedCount);
		}
	}

	//TODO: Check if this method and the other one are similar enough to just make into one method
	private void CreatePathsBetweenSpawnPoints()
	{
		List<TileMapSpace> spawnPoints = _possibleTileMapSpacesByFloorPosition.Values.Where(x => x.IsSpawnPoint).ToList();

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
				if (iterationCount < TileMappingConstants.NumberOfIterationsBeforeChangingAngle_PathCreation)
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
				if (IsBlockInsideBorders(newPositionToCheck) &&_possibleIndicesByFloorPositions.ContainsKey(newPositionToCheck))
				{
                    var nextWalk_TileMapSpace = new TileMapSpace();

                    if (_possibleTileMapSpacesByFloorPosition.ContainsKey(newPositionToCheck))
                    {
                        nextWalk_TileMapSpace = _possibleTileMapSpacesByFloorPosition[newPositionToCheck];
                    }
                    else
                    {
                        nextWalk_TileMapSpace = CreateDefaultTileMapSpace(newPositionToCheck, TileMapSpaceType.Floor);

                        _possibleTileMapSpacesByFloorPosition.Add(newPositionToCheck, nextWalk_TileMapSpace);
                    }

					if (nextWalk_TileMapSpace != null && nextWalk_TileMapSpace != startingSpawnPoint)
					{
						if (!nextWalk_TileMapSpace.InteriorBlock.IsQueuedForDeletion())
						{
                            nextWalk_TileMapSpace.InteriorBlock.QueueFree();
						}

						var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.NumberOfSpawnPointWhoClearedIt;

						walkingFloorSpace = nextWalk_TileMapSpace;
						walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;


                        _possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;

                        var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

                        DrawOnTileMap(nextWalk_TileMapSpace.TileMapPosition);

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

    private void SpawnPlayersSuperClose()
    {
        List<TileMapSpace> spawnPoints = _possibleTileMapSpacesByFloorPosition.Values.Where(x => x.IsSpawnPoint).ToList();

        int playerCount = 0;

        foreach (BaseCharacter character in _parentDungeonLevelSwapper.ActivePlayers)
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

                character.GlobalPosition = new Vector2(_parentDungeonLevelSwapper.ActivePlayers[0].GlobalPosition.X + changeInX, _parentDungeonLevelSwapper.ActivePlayers[0].GlobalPosition.Y + changeInY);

                AddChild(character);
            }

            GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

            playerCount++;
        }

        GD.Print("---------------------------------------");
    }

    private void SpawnPlayersNormal()
    {
        List<TileMapSpace> spawnPoints = _possibleTileMapSpacesByFloorPosition.Values.Where(x => x.IsSpawnPoint).ToList();

        int playerCount = 0;

        foreach (BaseCharacter character in _parentDungeonLevelSwapper.ActivePlayers)
        {
            GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

            character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

            AddChild(character);

            playerCount++;
        }

        GD.Print("---------------------------------------");
    }


    #endregion

    #region Creating Paths

    private void CreatePathsBetweenPoints()
	{
		Vector2I startPosition = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

		if (_possibleTileMapSpacesByFloorPosition.ContainsKey(startPosition))
		{
            TileMapSpace startingPoint = _possibleTileMapSpacesByFloorPosition[startPosition];

            TileMapSpace walkingFloorSpace = startingPoint;

            var targetPoint = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

            //For some reason, trig circle is flipped across its y axis... whatever...
            var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(_tileMap.LocalToMap(targetPoint));

            var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetPoint);

            var changeInX = 0;
            var changeInY = 0;

            int iterationCount = 0;

            //Do this until the walkingFloorSpace is no longer on the starting spawnPoint
            while (iterationCount < TileMappingConstants.NumberOfIterationsBeforeChangingAngle_PathCreation)
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

                //Set walkingFloorSpace to somewhere adjacent to starting point
                if (IsBlockInsideBorders(newPositionToCheck) && _possibleIndicesByFloorPositions.ContainsKey(newPositionToCheck))
                {
                    var nextWalk_TileMapSpace = new TileMapSpace();

                    if (_possibleTileMapSpacesByFloorPosition.ContainsKey(newPositionToCheck))
                    {
						nextWalk_TileMapSpace = _possibleTileMapSpacesByFloorPosition[newPositionToCheck];
                    }
                    else
                    {
                        nextWalk_TileMapSpace = CreateDefaultTileMapSpace(newPositionToCheck, TileMapSpaceType.Floor);

                        _possibleTileMapSpacesByFloorPosition.Add(newPositionToCheck, nextWalk_TileMapSpace);
                    }

                    if (nextWalk_TileMapSpace != null && nextWalk_TileMapSpace != startingPoint)
                    {
                        if (!nextWalk_TileMapSpace.InteriorBlock.IsQueuedForDeletion())
                        {
                            nextWalk_TileMapSpace.InteriorBlock.QueueFree();
                        }

                        var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.NumberOfSpawnPointWhoClearedIt;

                        walkingFloorSpace = nextWalk_TileMapSpace;
                        walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = 99;

                        _possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = 99;

                        var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                        rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

                        DrawOnTileMap(nextWalk_TileMapSpace.TileMapPosition);

                        if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
                            numberOfSpawnPointWhoClearedMatchingFloorSpace != 99)
                        {
                            break;
                        }
                    }
                }
				else
				{
					break;
				}

                iterationCount++;
            }
        }
	}

	private TileMapSpace CreateDefaultTileMapSpace(Vector2I positionOfTileMapSpace, TileMapSpaceType tileMapSpaceType)
	{
		//Generate InteriorWallBlock at that position
		var tempBlock = _interiorBlockScene.Instantiate();
		var interiorBlock = tempBlock as Node2D;
		var interiorWallBlock = tempBlock as InteriorWallBlock;
		AddChild(interiorBlock);

		var tempTestText = _interiorBlockTestTextScene.Instantiate();
		var testText = tempTestText as Node2D;
		AddChild(testText);

		var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(positionOfTileMapSpace));
		interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);
		testText.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

		TileMapSpace newTileMapSpace = new TileMapSpace()
		{
			InteriorBlock = interiorBlock,
			TestText = testText,
			TileMapPosition = positionOfTileMapSpace,
			ActualGlobalPosition = _tileMap.MapToLocal(positionOfTileMapSpace),
            TileMapSpaceType = tileMapSpaceType
        };

		return newTileMapSpace;
	}

    private bool IsBlockInsideBorders(Vector2I vector)
    {
        return vector.X != 0 && vector.Y != 0 && vector.X != _maxNumberOfTiles - 1 && vector.Y != _maxNumberOfTiles - 1;
    }

    #endregion

    #region Drawing Tiles & Art

    private void DrawOverviewAndWalls()
	{
		try
		{
			List<TileMapSpace> wallTileMapSpaces = new List<TileMapSpace>();

			//Create all floor-adjacent wall-tile map spaces
            foreach (Vector2I possiblePosition in _possibleTileMapSpacesByFloorPosition.Keys)
            {
                List<Vector2I> allAdjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(possiblePosition);

                foreach (Vector2I adjacentFloorSpacePosition in allAdjacentFloorSpacePositions)
                {
                    if (!_possibleTileMapSpacesByFloorPosition.ContainsKey(adjacentFloorSpacePosition))
                    {
                        var newWall_TileMapSpace = new TileMapSpace();

                        newWall_TileMapSpace = CreateDefaultTileMapSpace(adjacentFloorSpacePosition, TileMapSpaceType.Overview);

						newWall_TileMapSpace.NumberOfSpawnPointWhoClearedIt = 90;

                        //Can't add to list of TileMapSpaces-by-FloorPosition because it will alter the enumeration we're looping through

                        wallTileMapSpaces.Add(newWall_TileMapSpace);

                        var interiorBlockSprite = newWall_TileMapSpace.InteriorBlock.FindChild("Sprite2D") as Sprite2D;
						interiorBlockSprite.Texture = GetOverviewTexture();
					}
                }
			}

			//Add all newly-created tiles to the list of TileMapSpaces-by-FloorPosition
            foreach (TileMapSpace tileMapSpace in wallTileMapSpaces)
            {
                if (!_possibleTileMapSpacesByFloorPosition.ContainsKey(tileMapSpace.TileMapPosition))
                {
                    _possibleTileMapSpacesByFloorPosition.Add(tileMapSpace.TileMapPosition, tileMapSpace);
                }
            }

			//Adjust tiles for collisions and draw walls
			foreach (TileMapSpace wallTileMapSpace in wallTileMapSpaces)
            {
                List<Vector2I> allAdjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(wallTileMapSpace.TileMapPosition);

                //if current index isn't the very top
                if (wallTileMapSpace.TileMapPosition.Y != 0)
                {
                    Vector2I northBlockPosition = new Vector2I(wallTileMapSpace.TileMapPosition.X, wallTileMapSpace.TileMapPosition.Y - 1);

                    if ((!_possibleTileMapSpacesByFloorPosition.ContainsKey(northBlockPosition) ||
                        (_possibleTileMapSpacesByFloorPosition.ContainsKey(northBlockPosition) && _possibleTileMapSpacesByFloorPosition[northBlockPosition].NumberOfSpawnPointWhoClearedIt != 90)) &&
                        allAdjacentFloorSpacePositions.Any(x => x == northBlockPosition))
                    {
                        var collisionShape = wallTileMapSpace.InteriorBlock.FindChild("CollisionShape2D") as CollisionShape2D;

                        collisionShape.Shape = new RectangleShape2D() { Size = new Vector2(32, 16) };
                        collisionShape.Position = new Vector2(0, 8);
                    }
                }

				//if the index isn't the very bottom
                if (wallTileMapSpace.TileMapPosition.Y != _maxNumberOfTiles - 1)
                {
                    Vector2I southBlockPosition = new Vector2I(wallTileMapSpace.TileMapPosition.X, wallTileMapSpace.TileMapPosition.Y + 1);

                    if (((_possibleTileMapSpacesByFloorPosition.ContainsKey(southBlockPosition) && _possibleTileMapSpacesByFloorPosition[southBlockPosition].NumberOfSpawnPointWhoClearedIt != 90)) &&
                        allAdjacentFloorSpacePositions.Any(x => x == southBlockPosition))
                    {
                        var interiorBlockSprite = wallTileMapSpace.InteriorBlock.FindChild("Sprite2D") as Sprite2D;

						interiorBlockSprite.Texture = GetWallTexture();
                        wallTileMapSpace.TileMapSpaceType = TileMapSpaceType.Wall;
					}
                }
            }
        }
		catch (Exception ex)
		{
            GD.PushError(ex.Message);
        }
	}

	private void DrawWater(TileMapSpace startingTileMapSpace)
	{
        _tileMap.SetCell(0, startingTileMapSpace.TileMapPosition, TileMappingConstants.TileMapFrostFloorPureWater1AtlasId, Vector2I.Zero);
        startingTileMapSpace.TileMapSpaceType = TileMapSpaceType.Water;

        TileMapSpace walkingFloorSpace = startingTileMapSpace;

        var targetPoint = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

        //For some reason, trig circle is flipped across its y axis... whatever...
        var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(_tileMap.LocalToMap(targetPoint));

        var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetPoint);

        var changeInX = 0;
        var changeInY = 0;

        int iterationCount = 0;

        //Do this until the walkingFloorSpace is no longer on the starting spawnPoint
        while (iterationCount < TileMappingConstants.NumberOfIterationsBeforeChangingAngle_WaterCreation)
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

            var adjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(newPositionToCheck);

            //if all adjacent floor positions are also floor-type
            if (adjacentFloorSpacePositions.All(x => !_possibleTileMapSpacesByFloorPosition[x].IsSpawnPoint && !_possibleTileMapSpacesByFloorPosition[x].IsAdjacentToSpawnPoint &&
                                                (_possibleTileMapSpacesByFloorPosition[x].TileMapSpaceType == TileMapSpaceType.Floor
                                                || _possibleTileMapSpacesByFloorPosition[x].TileMapSpaceType == TileMapSpaceType.Water)))
            {
                walkingFloorSpace = _possibleTileMapSpacesByFloorPosition[newPositionToCheck];

                _tileMap.SetCell(0, newPositionToCheck, TileMappingConstants.TileMapFrostFloorPureWater1AtlasId, Vector2I.Zero);
                walkingFloorSpace.TileMapSpaceType = TileMapSpaceType.Water;
            }

            iterationCount++;
        }
    }

    private List<Vector2I> GetAllAdjacentFloorSpacePositions(Vector2I centralFloorSpacePosition)
	{
        List<Vector2I> adjacentFloorSpaces = new List<Vector2I>();

		Vector2I northAdjacentPosition = new Vector2I(centralFloorSpacePosition.X, centralFloorSpacePosition.Y - 1);
        Vector2I northEastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y - 1);
        Vector2I eastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y);
        Vector2I southEastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y + 1);
        Vector2I southAdjacentPosition = new Vector2I(centralFloorSpacePosition.X, centralFloorSpacePosition.Y + 1);
        Vector2I southWestAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y + 1);
        Vector2I westAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y);
        Vector2I northWestAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y - 1);

		CheckIfContainsKey(adjacentFloorSpaces, northAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, northEastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, eastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southEastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southWestAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, westAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, northWestAdjacentPosition);

        return adjacentFloorSpaces;
    }

	private void CheckIfContainsKey(List<Vector2I> adjacentFloorSpaces, Vector2I newPosition)
	{
        if (_possibleIndicesByFloorPositions.ContainsKey(newPosition))
        {
            adjacentFloorSpaces.Add(newPosition);
        }
    }

    private void DrawOnTileMap(Vector2I positionOfTile)
    {
        var atlasId = -1;
        var xAtlasCoord = -1;
        var yAtlasCoord = -1;

        if (SelectedBiomeType == BiomeType.Castle)
        {
            atlasId = TileMappingConstants.TileMapCastleFloorAtlasId;
            xAtlasCoord = _rng.RandiRange(0, 3);
            yAtlasCoord = _rng.RandiRange(0, 0);
        }
        else if (SelectedBiomeType == BiomeType.Frost)
        {
            atlasId = TileMappingConstants.TileMapFrostFloorAtlasId;
            xAtlasCoord = _rng.RandiRange(0, 3);
            yAtlasCoord = _rng.RandiRange(0, 0);
        }

        _tileMap.SetCell(0, positionOfTile, atlasId, new Vector2I(xAtlasCoord, yAtlasCoord));
    }

	private Texture2D GetOverviewTexture()
	{
		Texture2D overviewTexture = null;

		int overviewIndex = 0;

        if (SelectedBiomeType == BiomeType.Castle)
		{
            overviewIndex = _rng.RandiRange(0, 3);

            overviewTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Overview/CastleOverview{overviewIndex}.png") as Texture2D;
        }
		else if (SelectedBiomeType == BiomeType.Frost)
		{
            overviewIndex = _rng.RandiRange(0, 5);

            overviewTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Frost/Overview/FrostOverview{overviewIndex}.png") as Texture2D;
        }
        
		return overviewTexture;
    }

    private Texture2D GetWallTexture()
    {
        Texture2D wallTexture = null;

        int wallIndex = 0;

        if (SelectedBiomeType == BiomeType.Castle)
        {
            wallIndex = _rng.RandiRange(0, 5);

            wallTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Wall/CastleWall{wallIndex}.png") as Texture2D;
        }
        else if (SelectedBiomeType == BiomeType.Frost)
        {
            wallIndex = _rng.RandiRange(0, 2);

            wallTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Frost/Wall/FrostWall{wallIndex}.png") as Texture2D;
        }

        return wallTexture;
    }

    #endregion

    #endregion

    #region Key Object Generation

    private void GenerateKeyMapObjects()
	{
		GeneratePortal();

        GenerateSwitches();
    }

	private void GeneratePortal() 
	{
		var tempPortal = _portalScene.Instantiate();
		var portal = tempPortal as Node2D;
		AddChild(portal);

		var availableTargetSpaces = _possibleTileMapSpacesByFloorPosition.Values.Where(x => !x.IsSpawnPoint && x.TileMapSpaceType == TileMapSpaceType.Floor).ToList();

		//Need to match to make sure portals and switches don't spawn on each other, maybe add "holding something flag" to tile class
		portal.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].ActualGlobalPosition;
	}

	private void GenerateSwitches() 
	{
		for (int i = 0; i < 3; i++)
		{
			var tempPortalSwitch = _portalSwitchScene.Instantiate();
			var portalSwitch = tempPortalSwitch as Node2D;
			AddChild(portalSwitch);

			var availableTargetSpaces = _possibleTileMapSpacesByFloorPosition.Values.Where(x => !x.IsSpawnPoint && x.TileMapSpaceType == TileMapSpaceType.Floor).ToList();

			portalSwitch.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].ActualGlobalPosition;
		}
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

	public override void _Process(double delta)
	{
		if (_parentDungeonLevelSwapper.ActivePlayers.Count > 0 && _parentDungeonLevelSwapper.ActivePlayers.All(x => x.IsDead))
		{
            _parentDungeonLevelSwapper.ActivePlayers.Clear();

			EmitSignal(SignalName.GoToGameOverScreen);
		}

		if (GetTree() != null)
		{
            _enemyCount = GetTree().GetNodesInGroup("Enemy").Count;
        }

		SpawnEnemies();
	}

	#region Enemy Generation

	private List<string> _enemyList = new List<string>()
	{
		EnemyType.Slime.ToString()
	};

	private void SpawnEnemies()
	{
		if (_enemyRespawnTimer.IsStopped() &&_enemyCount < _enemyCountMax)
		{
			int availableEnemyCount = _enemyCountMax - _enemyCount;

			int chanceOfSpawning = _rng.RandiRange(1, 100) + availableEnemyCount * 10;

			if (chanceOfSpawning > 80)
			{
				SpawnEnemy();
			}

			_enemyRespawnTimer.Start();
		}
	}

	private void SpawnEnemy()
	{
		var tempEnemy = GD.Load<PackedScene>(EnemyScenePaths.SlimeScenePath).Instantiate();

		var enemy = tempEnemy as BaseEnemy;
		AddChild(enemy);

		var availableTargetSpaces = _possibleTileMapSpacesByFloorPosition.Values.Where(x => x.TileMapSpaceType == TileMapSpaceType.Floor && !x.IsSpawnPoint).ToList();

		enemy.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].ActualGlobalPosition;

		_enemyCount++;
	}

	#endregion
}
