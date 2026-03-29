using Godot;
using System;

public partial class ExitBackgroundButton : TextureButton
{
	public override void _Ready()
	{
		// Connect the signal using the C# Action syntax
		// Since this class IS a TextureButton, we use 'this' instead of looking for a child node
		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		// Closes the parent (likely the Menu or Popup)
		GetParent().QueueFree();
	}
}
