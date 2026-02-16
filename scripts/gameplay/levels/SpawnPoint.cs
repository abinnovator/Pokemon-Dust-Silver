using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
	public partial class SpawnPoint : Node2D
	{
		// Called when the node enters the scene tree for the first time.
		public override void _EnterTree()
		{
			AddToGroup(LevelGroups.SPAWNPOINTS.ToString());
		}
		public override void _ExitTree()
		{
			RemoveFromGroup(LevelGroups.SPAWNPOINTS.ToString());
		}

	}
}
