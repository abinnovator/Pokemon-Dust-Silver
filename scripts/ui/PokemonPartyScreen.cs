using Godot;
using System;

public partial class PokemonPartyScreen : Node2D
{
	[ExportCategory("Pokemon slots")]
	[Export] public RichTextLabel Pokemon1Label;
	[Export] public RichTextLabel Pokemon1Level;
	[Export] public TextureProgressBar Pokemon1HpBar;
	[Export] public Sprite2D Pokemon1Sprite;

	[Export] public RichTextLabel Pokemon2Label;
	[Export] public RichTextLabel Pokemon2Level;
	[Export] public TextureProgressBar Pokemon2HpBar;
	[Export] public Sprite2D Pokemon2Sprite;

	[Export] public RichTextLabel Pokemon3Label;
	[Export] public RichTextLabel Pokemon3Level;
	[Export] public TextureProgressBar Pokemon3HpBar;
	[Export] public Sprite2D Pokemon3Sprite;

	[Export] public RichTextLabel Pokemon4Label;
	[Export] public RichTextLabel Pokemon4Level;
	[Export] public TextureProgressBar Pokemon4HpBar;
	[Export] public Sprite2D Pokemon4Sprite;

	[Export] public RichTextLabel Pokemon5Label;
	[Export] public RichTextLabel Pokemon5Level;
	[Export] public TextureProgressBar Pokemon5HpBar;
	[Export] public Sprite2D Pokemon5Sprite;

	[Export] public RichTextLabel Pokemon6Label;
	[Export] public RichTextLabel Pokemon6Level;
	[Export] public TextureProgressBar Pokemon6HpBar;
	[Export] public Sprite2D Pokemon6Sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
