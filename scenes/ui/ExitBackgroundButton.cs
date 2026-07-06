using Godot;
using System;

public partial class ExitBackgroundButton : TextureButton
{
	public override void _Ready()
	{
		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		GetParent().QueueFree();
	}
}
