using Godot;
using System;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
	public partial class CharacterCollisionRaycast : RayCast2D
	{
		[Signal] public delegate void CollisionEventHandler(bool collided);
		[ExportCategory("Collision Vars")]
		[Export] public CharecterInput CharecterInput;
		[Export] public GodotObject Collider;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Logger.Info("CharacterCollisionRaycast Ready");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (TargetPosition != CharecterInput.TargetPosition)
			{
				TargetPosition = CharecterInput.TargetPosition;
			}
			if (IsColliding())
			{
				Collider = GetCollider();
				string colliderType = Collider.GetType().Name;
				// Logger.Info($"Collided with {colliderType}");
				switch(colliderType)
				{
					default:
					EmitSignal(SignalName.Collision, true);
					break;
				}
			}
			else
			{
				EmitSignal(SignalName.Collision, false);
			}
		}
	}
}
