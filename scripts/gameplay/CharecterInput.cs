using Godot;
using System;

namespace Game.Gameplay
{
	public partial class CharecterInput : Node
	{
        [Signal] public delegate void WalkEventHandler();
        [Signal] public delegate void TurnEventHandler();

		[ExportCategory("Common Input")]
		[Export] public Vector2 Direction = Vector2.Zero;
		[Export] public Vector2 TargetPosition = Vector2.Zero;
		
	}
}
