using System;
using Godot;

public partial class TrainerCard : Node2D
{
	[Export] public RichTextLabel Money;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Money.Text = SaveManager.Instance.CurrentSave.Money.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
