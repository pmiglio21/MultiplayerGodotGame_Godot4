using Enums;
using Globals;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Levels.OverworldLevels.TileMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileEntities.Enemies.Scripts;
using Root;
using Levels.OverworldLevels.KeyLevelObjects;

public partial class BaseDungeonLevel : Node
{
	private DungeonLevelSwapper _parentDungeonLevelSwapper;

    #region Signals

    [Signal]
    public delegate void GoToGameOverScreenEventHandler();

    [Signal]
    public delegate void ActivatePortalEventHandler();

    #endregion

    #region TileMap Level Generation

    private Dictionary<int,Vector2I> _possibleFloorPositionsByIndex = new Dictionary<int,Vector2I>();
    private Dictionary<Vector2I, int> _possibleIndicesByFloorPositions = new Dictionary<Vector2I, int>();
    private Dictionary<Vector2I, TileMapSpace> _possibleTileMapSpacesByFloorPosition = new Dictionary<Vector2I, TileMapSpace>();

    private List<TileMapSpace> _spawnPoints = new List<TileMapSpace>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private int _maxNumberOfTiles = 0;

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Key Level Object Generation

	private PackedScene _portalScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/Portal/Portal.tscn");
    private Portal _portal;

	private PackedScene _portalSwitchScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/PortalSwitch/PortalSwitch.tscn");
    private List<PortalSwitch> _portalSwitches = new List<PortalSwitch>();

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

    public string SelectedSpawnProximityType;
    public string SelectedSwitchProximityType;
    public string SelectedLevelSize;
    public string SelectedBiomeType;

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

        SelectedSpawnProximityType = SetSelectedType(_parentDungeonLevelSwapper.CurrentGameRules.SpawnProximityTypes);

        SelectedSwitchProximityType = SetSelectedType(_parentDungeonLevelSwapper.CurrentGameRules.SwitchProximityTypes);

        SelectedLevelSize = SetSelectedType(_parentDungeonLevelSwapper.CurrentGameRules.LevelSizes);

        SelectedBiomeType = SetSelectedType(_parentDungeonLevelSwapper.CurrentGameRules.BiomeTypes);

        #endregion

        SetPossibleFloorTiles();

        RunProceduralDungeonGeneration();

		//while (_enemyCount < _enemyCountMax)
		//{
		//	SpawnEnemy();
		//}	 
		_enemyRespawnTimer.Start();
	}

	#region Possible Floor Generation

	private void SetPossibleFloorTiles()
	{
        if (SelectedLevelSize == GlobalConstants.LevelSizeSmall)
		{
			_maxNumberOfTiles = 50;
		}
		else if (SelectedLevelSize == GlobalConstants.LevelSizeMedium)
		{
			_maxNumberOfTiles = 75;
		}
		else if (SelectedLevelSize == GlobalConstants.LevelSizeLarge)
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

    private void RunProceduralDungeonGeneration()
	{
        GenerateMultipleSpawnPoints();

        float percentageOfFloorToCover = 0;

        if (SelectedBiomeType == GlobalConstants.BiomeCastle)
        {
            percentageOfFloorToCover = .05f;
        }
        else
        {
            percentageOfFloorToCover = .15f;
        }

        while (_possibleTileMapSpacesByFloorPosition.Count < (percentageOfFloorToCover * _possibleFloorPositionsByIndex.Count))
        {
            CreatePathBetweenRandomPoints();
        }

        DrawOverviewAndWalls();

        //Add water
        if (SelectedBiomeType == GlobalConstants.BiomeFrost)
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

        if (SelectedSpawnProximityType == GlobalConstants.SpawnProximityGroup)
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

    private void GenerateSingleSpawnPoint(int spawnPointGeneratedCount = 0)
    {
		Dictionary<int,Vector2I> nonBorderPositions = new Dictionary<int, Vector2I>();

		int counter = 0;

        //TODO: Make sure this point is not a spawn point too
		foreach (var indexPositionPair in _possibleFloorPositionsByIndex.Where(position => IsBlockInsideBorders(position.Value)))
		{
			nonBorderPositions.Add(counter, indexPositionPair.Value);
			counter++;
        }

        //Get a random space in possible floor spaces to pick as spawn point
        var floorTileIndex = _rng.RandiRange(0, nonBorderPositions.Count - 1);

        //Create Spawn Point
        var newSpawnPoint_TileMapSpace = new TileMapSpace();

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
        newSpawnPoint_TileMapSpace.LastNumberToClearSpace = spawnPointGeneratedCount;
        newSpawnPoint_TileMapSpace.SpawnPointNumber = spawnPointGeneratedCount;

        _possibleTileMapSpacesByFloorPosition[newSpawnPoint_TileMapSpace.TileMapPosition].SpawnPointNumber = spawnPointGeneratedCount;

        var richTextLabel = newSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
        richTextLabel.Text = spawnPointGeneratedCount.ToString();

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
                nearSpawnPoint_TileMapSpace.SpawnPointNumber = spawnPointGeneratedCount;

                _possibleTileMapSpacesByFloorPosition[nearSpawnPoint_TileMapSpace.TileMapPosition].SpawnPointNumber = spawnPointGeneratedCount;

                var rtl = nearSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                rtl.Text = spawnPointGeneratedCount.ToString();

                DrawOnTileMap(nearSpawnPoint_TileMapSpace.TileMapPosition);
            }
        }

        _spawnPoints.Add(newSpawnPoint_TileMapSpace);
    }

    private void GenerateMultipleSpawnPoints()
	{
        int maxTilesAdditionalNumber = 0;

        if (SelectedLevelSize == GlobalConstants.LevelSizeSmall)
        {
            maxTilesAdditionalNumber = 5;
        }
        else if (SelectedLevelSize == GlobalConstants.LevelSizeMedium)
        {
            maxTilesAdditionalNumber = 7;
        }
        else if (SelectedLevelSize == GlobalConstants.LevelSizeLarge)
        {
            maxTilesAdditionalNumber = 9;
        }

        for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < _parentDungeonLevelSwapper.ActivePlayers.Count + maxTilesAdditionalNumber; spawnPointGeneratedCount++)
		{
			GenerateSingleSpawnPoint(spawnPointGeneratedCount);

            if (spawnPointGeneratedCount != 0)
            {
                CreatePathBetweenSpawnPoints(_spawnPoints[spawnPointGeneratedCount]);
            }
		}
	}

    private void CreatePathBetweenSpawnPoints(TileMapSpace startingSpawnPoint)
    {
        try
        {
            TileMapSpace walkingFloorSpace = startingSpawnPoint;

            TileMapSpace targetSpawnPoint = null;

            targetSpawnPoint = _spawnPoints.FirstOrDefault(x => x.LastNumberToClearSpace == startingSpawnPoint.LastNumberToClearSpace - 1);

            //For some reason, trig circle is flipped across its y axis... whatever...
            var angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

            Tuple<int, int> weightedValues = new Tuple<int, int>(0, 0);

            if (SelectedBiomeType == GlobalConstants.BiomeCastle)
            {
                weightedValues = GetWeightedValues_Direct(angleFromWalkingFloorSpaceToTargetSpawnPoint);
            }
            else
            {
                weightedValues = GetWeightedValues_Roundabout(angleFromWalkingFloorSpaceToTargetSpawnPoint);
            }

            var changeInX = 0;
            var changeInY = 0;

            int iterationCount = 0;

            //Do this until the walkingFloorSpace is no longer on the starting spawnPoint
            while (true)
            {
                if (iterationCount == TileMappingConstants.NumberOfIterationsBeforeChangingAngle_PathCreation)
                {
                    angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

                    if (SelectedBiomeType == GlobalConstants.BiomeCastle)
                    {
                        weightedValues = GetWeightedValues_Direct(angleFromWalkingFloorSpaceToTargetSpawnPoint);
                    }
                    else
                    {
                        weightedValues = GetWeightedValues_Roundabout(angleFromWalkingFloorSpaceToTargetSpawnPoint);
                    }

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

                    if (nextWalk_TileMapSpace != null)
                    {
                        if (!nextWalk_TileMapSpace.InteriorBlock.IsQueuedForDeletion())
                        {
                            nextWalk_TileMapSpace.InteriorBlock.QueueFree();
                        }

                        var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.LastNumberToClearSpace;

                        walkingFloorSpace = nextWalk_TileMapSpace;
                        walkingFloorSpace.LastNumberToClearSpace = startingSpawnPoint.SpawnPointNumber;


                        _possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].LastNumberToClearSpace = startingSpawnPoint.SpawnPointNumber;

                        var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                        rtl.Text = walkingFloorSpace.LastNumberToClearSpace.ToString();

                        DrawOnTileMap(nextWalk_TileMapSpace.TileMapPosition);

                        if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
                            numberOfSpawnPointWhoClearedMatchingFloorSpace != startingSpawnPoint.SpawnPointNumber)
                        {
                            break;
                        }
                    }
                }

                iterationCount++;
            }
        }
        catch (Exception e)
        { 
            GD.PushError(e);
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

    //TODO: Does this definitely not make paths floating out in space by themselves?
    private void CreatePathBetweenRandomPoints()
	{
		Vector2I startPosition = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

		if (_possibleTileMapSpacesByFloorPosition.ContainsKey(startPosition))
		{
            TileMapSpace startingPoint = _possibleTileMapSpacesByFloorPosition[startPosition];

            TileMapSpace walkingFloorSpace = startingPoint;

            var targetPoint = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

            //For some reason, trig circle is flipped across its y axis... whatever...
            var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(_tileMap.LocalToMap(targetPoint));

            Tuple<int, int> weightedValues = new Tuple<int, int>(0, 0);

            if (SelectedBiomeType == GlobalConstants.BiomeCastle)
            {
                weightedValues = GetWeightedValues_Direct(angleFromWalkingFloorSpaceToTargetPoint);
            }
            else
            {
                weightedValues = GetWeightedValues_Roundabout(angleFromWalkingFloorSpaceToTargetPoint);
            }

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

                        var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.LastNumberToClearSpace;

                        walkingFloorSpace = nextWalk_TileMapSpace;
                        walkingFloorSpace.LastNumberToClearSpace = 99;

                        _possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].LastNumberToClearSpace = 99;

                        var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                        rtl.Text = walkingFloorSpace.LastNumberToClearSpace.ToString();

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

						newWall_TileMapSpace.LastNumberToClearSpace = 90;

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
                        (_possibleTileMapSpacesByFloorPosition.ContainsKey(northBlockPosition) && _possibleTileMapSpacesByFloorPosition[northBlockPosition].LastNumberToClearSpace != 90)) &&
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

                    if (((_possibleTileMapSpacesByFloorPosition.ContainsKey(southBlockPosition) && _possibleTileMapSpacesByFloorPosition[southBlockPosition].LastNumberToClearSpace != 90)) &&
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

        var weightedValues = GetWeightedValues_Roundabout(angleFromWalkingFloorSpaceToTargetPoint);

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

    //TODO: Verify that it only returns adjacents that aren't out of bounds... when necessary
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

        if (SelectedBiomeType == GlobalConstants.BiomeCastle)
        {
            atlasId = TileMappingConstants.TileMapCastleFloorAtlasId;
            xAtlasCoord = _rng.RandiRange(0, 3);
            yAtlasCoord = _rng.RandiRange(0, 0);
        }
        else if (SelectedBiomeType == GlobalConstants.BiomeFrost)
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

        if (SelectedBiomeType == GlobalConstants.BiomeCastle)
		{
            overviewIndex = _rng.RandiRange(0, 3);

            overviewTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Overview/CastleOverview{overviewIndex}.png") as Texture2D;
        }
		else if (SelectedBiomeType == GlobalConstants.BiomeFrost)
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

        if (SelectedBiomeType == GlobalConstants.BiomeCastle)
        {
            wallIndex = _rng.RandiRange(0, 5);

            wallTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Wall/CastleWall{wallIndex}.png") as Texture2D;
        }
        else if (SelectedBiomeType == GlobalConstants.BiomeFrost)
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
        try
        {
            var tempPortal = _portalScene.Instantiate();
            _portal = tempPortal as Portal;
            AddChild(_portal);

            var availableTargetSpaces = _possibleTileMapSpacesByFloorPosition.Values.Where(x => !x.IsSpawnPoint && x.TileMapSpaceType == TileMapSpaceType.Floor && !x.IsSomethingInTileMapSpace && x.InteriorBlock.IsQueuedForDeletion()).ToList();

            Vector2 targetPortalPosition = new Vector2();

            foreach (TileMapSpace spawnPoint in _spawnPoints)
            {
                targetPortalPosition += spawnPoint.ActualGlobalPosition;
            }

            targetPortalPosition = targetPortalPosition / _spawnPoints.Count;

            targetPortalPosition = new Vector2(Mathf.Floor(targetPortalPosition.X), Mathf.Floor(targetPortalPosition.Y));

            Vector2 actualPortalPosition = new Vector2();

            float closestDistance = float.MaxValue;

            foreach (TileMapSpace availableSpace in availableTargetSpaces)
            {
                float distance = availableSpace.ActualGlobalPosition.DistanceTo(targetPortalPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;

                    actualPortalPosition = availableSpace.ActualGlobalPosition;
                }
            }

            _portal.GlobalPosition = actualPortalPosition;

            _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(actualPortalPosition)].IsSomethingInTileMapSpace = true;
        }
        catch (Exception ex)
        { 
            GD.PushError(ex);
        }
    }

	private void GenerateSwitches() 
	{
        if (SelectedSwitchProximityType == GlobalConstants.SwitchProximitySuperClose)
        {
            int switchCount = SelectedSpawnProximityType == GlobalConstants.SpawnProximityGroup ? 1 : _parentDungeonLevelSwapper.ActivePlayers.Count;

            for (int i = 0; i < switchCount; i++)
            {
                var tempPortalSwitch = _portalSwitchScene.Instantiate();
                var portalSwitch = tempPortalSwitch as PortalSwitch;
                AddChild(portalSwitch);

                var spawnPoint = _spawnPoints.FirstOrDefault(x => x.SpawnPointNumber == i);

                var spawnPointTileMapPosition = spawnPoint.TileMapPosition;

                var spawnPointAdjacentTileMapPositions = GetAllAdjacentFloorSpacePositions(spawnPointTileMapPosition);

                var spawnPointAdjacentTileMapSpaces = new List<TileMapSpace>();

                foreach (Vector2I adjacentTileMapPosition in spawnPointAdjacentTileMapPositions)
                {
                    TileMapSpace adjacentTileMapSpace = _possibleTileMapSpacesByFloorPosition[adjacentTileMapPosition];

                    if (adjacentTileMapSpace.InteriorBlock.IsQueuedForDeletion() && adjacentTileMapSpace.ActualGlobalPosition != _portal.GlobalPosition && adjacentTileMapSpace.TileMapSpaceType == TileMapSpaceType.Floor && !adjacentTileMapSpace.IsSomethingInTileMapSpace)
                    {
                        spawnPointAdjacentTileMapSpaces.Add(adjacentTileMapSpace);
                    }
                }

                portalSwitch.GlobalPosition = spawnPointAdjacentTileMapSpaces[_rng.RandiRange(0, spawnPointAdjacentTileMapSpaces.Count - 1)].ActualGlobalPosition;

                _portalSwitches.Add(portalSwitch);

                _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(portalSwitch.GlobalPosition)].IsSomethingInTileMapSpace = true;
            }
        }
        else if (SelectedSwitchProximityType == GlobalConstants.SwitchProximityClose)
        {
            SetSwitchPositions(Mathf.Floor(_maxNumberOfTiles *.15f), Mathf.Floor(_maxNumberOfTiles * .2f));
        }
        else if (SelectedSwitchProximityType == GlobalConstants.SwitchProximityNormal)
        {
            SetSwitchPositions(Mathf.Floor(_maxNumberOfTiles * .3f), Mathf.Floor(_maxNumberOfTiles * .45f));
        }
        else if (SelectedSwitchProximityType == GlobalConstants.SwitchProximityFar)
        {
            SetSwitchPositions(Mathf.Floor(_maxNumberOfTiles * .45f), Mathf.Floor(_maxNumberOfTiles * .6f));
        }

        foreach (PortalSwitch portalSwitch in _portalSwitches)
        {
            portalSwitch.SwitchActivated += CheckToActivatePortal; 
        }
	}

    private void SetSwitchPositions(float minDistanceFromSpawnPoint, float maxDistanceFromSpawnPoint)
    {
        var availableTargetSpaces = _possibleTileMapSpacesByFloorPosition.Values.Where(x => !x.IsSpawnPoint && x.TileMapSpaceType == TileMapSpaceType.Floor && !x.IsSomethingInTileMapSpace).ToList();

        var firstSpawnPointSatisfied = _spawnPoints.FirstOrDefault(x => x.SpawnPointNumber == 0) == null || _parentDungeonLevelSwapper.ActivePlayers.Count < 1;
        var secondSpawnPointSatisfied = _spawnPoints.FirstOrDefault(x => x.SpawnPointNumber == 1) == null || _parentDungeonLevelSwapper.ActivePlayers.Count < 2;
        var thirdSpawnPointSatisfied = _spawnPoints.FirstOrDefault(x => x.SpawnPointNumber == 2) == null || _parentDungeonLevelSwapper.ActivePlayers.Count < 3;
        var fourthSpawnPointSatisfied = _spawnPoints.FirstOrDefault(x => x.SpawnPointNumber == 3) == null || _parentDungeonLevelSwapper.ActivePlayers.Count < 4;

        foreach (TileMapSpace currentTileMapSpace in availableTargetSpaces)
        {
             if (!firstSpawnPointSatisfied)
             {
                 float distanceBetweenFirstSpawnPointAndCurrentTileMapSpace = ((Vector2)_spawnPoints[0].TileMapPosition).DistanceTo(currentTileMapSpace.TileMapPosition);

                 if (currentTileMapSpace.InteriorBlock.IsQueuedForDeletion()
                     && distanceBetweenFirstSpawnPointAndCurrentTileMapSpace > minDistanceFromSpawnPoint
                     && distanceBetweenFirstSpawnPointAndCurrentTileMapSpace < maxDistanceFromSpawnPoint)
                 {
                    var portalSwitch = _portalSwitchScene.Instantiate() as PortalSwitch;
                    AddChild(portalSwitch);

                     portalSwitch.GlobalPosition = currentTileMapSpace.ActualGlobalPosition;

                    _portalSwitches.Add(portalSwitch);

                    _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(portalSwitch.GlobalPosition)].IsSomethingInTileMapSpace = true;

                    firstSpawnPointSatisfied = true;
                 }
             }

             if (!secondSpawnPointSatisfied)
             {
                 float distanceBetweenSecondSpawnPointAndCurrentTileMapSpace = ((Vector2)_spawnPoints[1].TileMapPosition).DistanceTo(currentTileMapSpace.TileMapPosition);

                 if (currentTileMapSpace.InteriorBlock.IsQueuedForDeletion()
                     && distanceBetweenSecondSpawnPointAndCurrentTileMapSpace > minDistanceFromSpawnPoint
                     && distanceBetweenSecondSpawnPointAndCurrentTileMapSpace < maxDistanceFromSpawnPoint)
                 {
                    var portalSwitch = _portalSwitchScene.Instantiate() as PortalSwitch;
                    AddChild(portalSwitch);

                    portalSwitch.GlobalPosition = currentTileMapSpace.ActualGlobalPosition;

                    _portalSwitches.Add(portalSwitch);

                    _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(portalSwitch.GlobalPosition)].IsSomethingInTileMapSpace = true;

                    secondSpawnPointSatisfied = true;
                 }
             }

             if (!thirdSpawnPointSatisfied)
             {
                 float distanceBetweenThirdSpawnPointAndCurrentTileMapSpace = ((Vector2)_spawnPoints[2].TileMapPosition).DistanceTo(currentTileMapSpace.TileMapPosition);

                 if (currentTileMapSpace.InteriorBlock.IsQueuedForDeletion()
                     && distanceBetweenThirdSpawnPointAndCurrentTileMapSpace > minDistanceFromSpawnPoint
                     && distanceBetweenThirdSpawnPointAndCurrentTileMapSpace < maxDistanceFromSpawnPoint)
                 {
                    var portalSwitch = _portalSwitchScene.Instantiate() as PortalSwitch;
                    AddChild(portalSwitch);

                    portalSwitch.GlobalPosition = currentTileMapSpace.ActualGlobalPosition;

                    _portalSwitches.Add(portalSwitch);

                    _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(portalSwitch.GlobalPosition)].IsSomethingInTileMapSpace = true;

                    thirdSpawnPointSatisfied = true;
                 }
             }

             if (!fourthSpawnPointSatisfied)
             {
                 float distanceBetweenFourthSpawnPointAndCurrentTileMapSpace = ((Vector2)_spawnPoints[3].TileMapPosition).DistanceTo(currentTileMapSpace.TileMapPosition);

                 if (currentTileMapSpace.InteriorBlock.IsQueuedForDeletion()
                     && distanceBetweenFourthSpawnPointAndCurrentTileMapSpace > minDistanceFromSpawnPoint
                     && distanceBetweenFourthSpawnPointAndCurrentTileMapSpace < maxDistanceFromSpawnPoint)
                 {
                    var portalSwitch = _portalSwitchScene.Instantiate() as PortalSwitch;
                    AddChild(portalSwitch);

                    portalSwitch.GlobalPosition = currentTileMapSpace.ActualGlobalPosition;

                    _portalSwitches.Add(portalSwitch);

                    _possibleTileMapSpacesByFloorPosition[_tileMap.LocalToMap(portalSwitch.GlobalPosition)].IsSomethingInTileMapSpace = true;

                    fourthSpawnPointSatisfied = true;
                 }
             }


             if (firstSpawnPointSatisfied && secondSpawnPointSatisfied && thirdSpawnPointSatisfied && fourthSpawnPointSatisfied)
             {
                 break;
             }
        }
    }

    private void CheckToActivatePortal()
    {
        if (_portalSwitches.All(x => x.IsSwitchActivated))
        {
            _portal.IsPortalActivated = true;
            _portal.PlayPortalActivation();

            foreach (BaseCharacter player in _parentDungeonLevelSwapper.ActivePlayers)
            {
                if (player.IsInPortalArea)
                {
                    player.IsWaitingForPortal = true;
                }
            }

            EmitSignal(SignalName.ActivatePortal);
        }
    }

    #endregion

    #region Direction-Changing Utility Methods

    private int SetRandomChangeInDirection(int weightedValue)
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

    private Tuple<int, int> GetWeightedValues_Roundabout(float angleFromWalkingFloorSpaceToTargetSpawnPoint)
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

    private Tuple<int, int> GetWeightedValues_Direct(float angleFromWalkingFloorSpaceToTargetSpawnPoint)
    {
        var weightedXValue = 0;
        var weightedYValue = 0;

        //Northeast
        if (angleFromWalkingFloorSpaceToTargetSpawnPoint > (Math.PI / 6f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (Math.PI / 3f))
        {
            weightedXValue = 1;
            weightedYValue = 1;
        }
        //North
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint > (Math.PI / 3f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (2 * Math.PI / 3f))
        {
            weightedXValue = 0;
            weightedYValue = 1;
        }
        //Northwest
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint > (2 * Math.PI / 3f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (5 * Math.PI / 6))
        {
            weightedXValue = -1;
            weightedYValue = 1;
        }
        //West
        else if ((angleFromWalkingFloorSpaceToTargetSpawnPoint > (5 * Math.PI / 6f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (2 * Math.PI)) ||
                 (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(5 * Math.PI / 6) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(2 * Math.PI)))
        {
            weightedXValue = -1;
            weightedYValue = 0;
        }
        //Southwest
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(2 * Math.PI / 3) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(5 * Math.PI / 6))
        {
            weightedXValue = -1;
            weightedYValue = -1;
        }
        //South
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(Math.PI / 3) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(2 * Math.PI / 3))
        {
            weightedXValue = 0;
            weightedYValue = -1;
        }
        //Southeast
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(Math.PI / 6) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(Math.PI / 3))
        {
            weightedXValue = 1;
            weightedYValue = -1;
        }
        //East
        else if (angleFromWalkingFloorSpaceToTargetSpawnPoint > -(Math.PI / 6) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (Math.PI / 6))
        {
            weightedXValue = 1;
            weightedYValue = 0;
        }
        else //default
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

		//SpawnEnemies();
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

    private string SetSelectedType(Dictionary<string, bool> totalTypes)
    {
        Dictionary<string, bool> availableTypes = new Dictionary<string, bool>();

        foreach (KeyValuePair<string, bool> typePair in totalTypes)
        {
            if (typePair.Value)
            {
                availableTypes.Add(typePair.Key, typePair.Value);
            }
        }

        int typePairIndex = _rng.RandiRange(0, availableTypes.Count - 1);

        return availableTypes.ElementAt(typePairIndex).Key;
    }
}
