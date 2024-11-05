using Godot;
using System;

public partial class TileMapSpace 
{
	public Node2D InteriorBlock = null;

	public bool IsSpawnPoint = false;

	public int NumberOfSpawnPointWhoClearedIt = -1;

	public Node2D TestText = null;

	public Vector2I TileMapPosition = Vector2I.Zero;

    public Vector2 ActualGlobalPosition = Vector2I.Zero; //Needed because can't access InteriorBlock's global position if it is queued for deletion

	public bool IsCleared = false;
}
