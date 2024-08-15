using Godot;
using System;

public partial class TileMapSpace 
{
	public Node2D InteriorBlock = null;

	public bool IsSpawnPoint = false;

	public int NumberOfSpawnPointWhoClearedIt = -1;

	public Node2D TestText = null;

	public Vector2I TileMapPosition = Vector2I.Zero;
}
