using Godot;
using System;

public partial class ExitBackgroundButton : TextureButton
{
	// Declare the variable here, but don't assign GetNode yet
	private BaseButton _button;

	public override void _Ready()
	{
		// GetNode must be called inside _Ready or another method
		_button = GetNode<BaseButton>("Button");

		if (_button == null)
		{
			GD.PrintErr("Button node not found!");
			return;
		}

		// Connect the signal using the C# Action syntax
		_button.Pressed += OnButtonPressed;
	}

	// Process isn't needed for a simple button exit, so we can remove it to keep it clean

	private void OnButtonPressed()
	{
		// Closes the parent (likely the Menu or Popup)
		GetParent().QueueFree();
	}
}
