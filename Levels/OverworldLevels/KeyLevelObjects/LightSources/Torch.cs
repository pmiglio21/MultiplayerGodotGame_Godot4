using Globals;
using Godot;
using MobileEntities.PlayerCharacters;
using Root;
using System;
using System.Collections.Generic;

public partial class Torch : Node2D
{
    private DungeonLevelSwapper _parentDungeonLevelSwapper;

	public Sprite2D Sprite;
    private PointLight2D _upperLight;
    private PointLight2D _lowerLight;
    protected Area2D playerDetectionBox;
    protected CollisionShape2D playerDetectionBoxCollisionShape;

    private const int _distanceToDetectPlayer = 256;
    private List<int> _playersInArea = new List<int>();
    private bool _isInsideWall = false;

    protected bool isPlayerDetected = false;
    protected bool isOverviewDetected = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

        _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();

        Sprite = GetNode<Sprite2D>("Sprite2D");
        _upperLight = GetNode<PointLight2D>("UpperLight");
        _lowerLight = GetNode<PointLight2D>("LowerLight");

        playerDetectionBox = GetNode<Area2D>("PlayerDetectionBox");
        playerDetectionBoxCollisionShape = playerDetectionBox.GetNode<CollisionShape2D>("CollisionShape");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (!_isInsideWall && _lowerLight.Energy == 0)
        {
            _lowerLight.Energy = 2.0f;
        }

        BaseCharacter closestPlayer = FindClosestPlayer();

        if (closestPlayer != null)
        {
            playerDetectionBox.Scale = Vector2.One;

            var distanceBetweenClosestPlayer = closestPlayer.GlobalPosition.DistanceTo(GlobalPosition);

            var direction = (closestPlayer.GlobalPosition - GlobalPosition).Normalized();

            var angleBetweenClosestPlayerAndEnemy = (closestPlayer.GlobalPosition - GlobalPosition).Angle();


            playerDetectionBox.Rotation = angleBetweenClosestPlayerAndEnemy;

            playerDetectionBox.GlobalPosition = (closestPlayer.GlobalPosition + GlobalPosition) / 2;

            var playerDetectionBoxSize = new Vector2(distanceBetweenClosestPlayer, playerDetectionBoxCollisionShape.Shape.GetRect().Size.Y);

            playerDetectionBoxCollisionShape.Shape = new RectangleShape2D() { Size = playerDetectionBoxSize };


            var overlappingBodies = playerDetectionBox.GetOverlappingBodies();

            isOverviewDetected = false;

            foreach (var body in overlappingBodies)
            {
                if (body.GetParent() is InteriorWallBlock && body.GlobalPosition.DistanceTo(GlobalPosition) > 16 && !(body.GetParent() as InteriorWallBlock).IsWall)
                {
                    isOverviewDetected = true;
                    break;
                }
            }


            if ((distanceBetweenClosestPlayer <= _distanceToDetectPlayer && isPlayerDetected && !isOverviewDetected))
            {
                _upperLight.Show();
                _lowerLight.Show();
            }
            else
            {
                _upperLight.Hide();
                _lowerLight.Hide();
            }
        }
        else
        {
            playerDetectionBox.Scale = Vector2.Zero;
        }
    }

    protected BaseCharacter FindClosestPlayer()
    {
        BaseCharacter closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in _parentDungeonLevelSwapper.ActivePlayers)
        {
            var newDistance = player.GlobalPosition.DistanceTo(this.GlobalPosition);

            if (newDistance <= _distanceToDetectPlayer && newDistance < minDistance)
            {
                closestPlayer = player;
                minDistance = newDistance;
            }
        }

        return closestPlayer;
    }


    private void OnInteractionAreaEntered(Area2D area)
	{
        if (area.IsInGroup("PlayerHurtBox"))
        {
            var character = area.GetParent() as BaseCharacter;

            ShaderMaterial shaderMaterial = GD.Load<ShaderMaterial>(ShaderMaterialPaths.OutlineShaderMaterialPath);
            Sprite.Material = shaderMaterial;

            if (!_playersInArea.Contains(character.PlayerNumber))
            {
                _playersInArea.Add(character.PlayerNumber);
            }
        }
	}

    private void OnInteractionAreaExited(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            var character = area.GetParent() as BaseCharacter;

            if (_playersInArea.Contains(character.PlayerNumber))
            {
                _playersInArea.Remove(character.PlayerNumber);
            }

            if (_playersInArea.Count == 0)
            {
                Sprite.Material = new ShaderMaterial();
            }
        }
    }

    private void OnInteriorWallBlockCollisionAreaOnBodyShapeEntered(Node2D node2D)
    {
        if (node2D.IsInGroup("InteriorWallBlock"))
        {
            _isInsideWall = true;

            _upperLight.Scale = new Vector2(1.0f, 1.0f);

            _lowerLight.Hide();
        }
    }

    private void OnPlayerDetectionBoxAreaEntered(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

            if (!collisionShape.Disabled)
            {
                isPlayerDetected = true;

                //GD.Print($"PlayerHurtBox Entered {area.GetParent().Name}");
            }
        }
    }

    private void OnPlayerDetectionBoxAreaExited(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

            if (!collisionShape.Disabled)
            {
                Node2D character = area.GetParent() as Node2D;

                isPlayerDetected = false;

                //GD.Print($"PlayerHurtBox Exited {area.GetParent().Name}");
            }
        }
    }
}
