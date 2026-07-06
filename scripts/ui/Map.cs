using Godot;
using System;


public partial class Map : Node2D
{

	[Export] public Control Container;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Container.Visible = SaveManager.Instance.IsOnBike;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Container.Visible = SaveManager.Instance.IsOnBike;
	}
	public void changeVisibility()
	{
		Container.Visible = SaveManager.Instance.IsOnBike;
	}
}
