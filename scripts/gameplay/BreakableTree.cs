using Game.Core;
using Godot;
using System;

public partial class BreakableTree : Area2D
{
	[ExportCategory("Config")]
	[Export] 
	public BreakableTrees tree_no;
	[Export]public TileMapLayer tree;
	[Export] public CollisionShape2D collisionshape;

	private bool _playerNearby = false;

	public override void _Ready()
	{
		// Connect the signals
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			_playerNearby = true;
			GD.Print("Player is next to the tree!");
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			_playerNearby = false;
			GD.Print("Player left the tree.");
		}
	}

	public override void _Process(double delta)
{
	if (_playerNearby && Input.IsActionJustPressed("V"))
	{
		tree.Visible = false;
		collisionshape.Disabled = true;

		tree.CollisionEnabled = false;

		MessageManager.PlayText("Your pokemon destroyed the tree for you!");
	}
}
}
