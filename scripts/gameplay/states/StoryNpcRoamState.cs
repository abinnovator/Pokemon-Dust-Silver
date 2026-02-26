using System;
using Game.Core;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

public partial class StoryNpcRoamState : State
{
	[ExportCategory("State Vars")]
	[Export]
	public StoryNpcInput NpcInput;

	[Export]
	public CharacterMovement CharacterMovement;

	private double timer = 2f;
	private Array<Vector2> currentPatrolPoints = [];
	private bool _isUnblocking = false;

	public override void _Process(double delta)
	{
		if (CharacterMovement.IsMoving()) return;

		var config = NpcInput.NpcInputConfig;
		if (config == null) return;

		// --- Passive Blocking Logic ---
		if (config.IsBlocker)
		{
			var storyNpc = (StoryNpc)StateOwner;
			if (storyNpc.HasUnblocked) 
			{
				RunMovementSwitch(delta);
				return;
			}

			bool conditionMet = SaveManager.Instance.CurrentSave
				.CompletedStoryProgress.Contains(config.UnblockCondition);

			if (conditionMet && !_isUnblocking)
			{
				_isUnblocking = true;
				UnblockAsync(storyNpc, config);
			}
			return;
		}

		// --- Normal Story NPC (event-gated movement) ---
		var eventTrigger = config.EventTrigger;
		if (SaveManager.Instance.CurrentSave.CompletedStoryProgress.Contains(eventTrigger))
		{
			RunMovementSwitch(delta);
		}
	}

	private async void UnblockAsync(StoryNpc storyNpc, StoryNpcInputConfig config)
	{
		if (config.ClearPosition != Vector2.Zero)
		{
			await storyNpc.TweenToClearPosition(config.ClearPosition);
		}
		storyNpc.HasUnblocked = true;
		_isUnblocking = false;
	}

	private void RunMovementSwitch(double delta)
	{
		switch (NpcInput.NpcInputConfig.NpcMovementType)
		{
			case NpcMovementType.Wander:
				HandleWander(delta, NpcInput.NpcInputConfig.WanderMoveInterval);
				break;
			case NpcMovementType.LookAround:
				HandleLookAround(delta, NpcInput.NpcInputConfig.LookAroundInterval);
				break;
			case NpcMovementType.Patrol:
				HandlePatrol(delta, NpcInput.NpcInputConfig.PatrolMoveInterval);
				break;
		}
	}

	private void HandlePatrol(double delta, double interval)
	{
		if (NpcInput.NpcInputConfig.PatrolPoints.Count == 0) return;

		timer -= delta;
		if (timer > 0) return;

		Vector2 currentPosition = ((StoryNpc)StateOwner).Position;
		var level = SceneManager.GetCurrentLevel();
		if (currentPatrolPoints.Count == 0)
		{
			var patrolPoint = NpcInput.NpcInputConfig.PatrolPoints[NpcInput.NpcInputConfig.PatrolIndex];
			NpcInput.NpcInputConfig.PatrolIndex = (NpcInput.NpcInputConfig.PatrolIndex + 1) % NpcInput.NpcInputConfig.PatrolPoints.Count;
			var pathing = level.Grid.GetIdPath(Modules.ConvertVector2ToVector2I(currentPosition), Modules.ConvertVector2ToVector2I(patrolPoint));

			for (int i = 1; i < pathing.Count; i++)
			{
				var point = pathing[i];
				currentPatrolPoints.Add(Modules.ConvertVector2IToVector2(point));
			}
			level.CurrentControlPoints = currentPatrolPoints;
			if (currentPatrolPoints.Count == 0) return;
		}
		if (currentPosition.DistanceTo(currentPatrolPoints[0]) < Globals.GridSize / 2f)
		{
			currentPatrolPoints.RemoveAt(0);
			if (currentPatrolPoints.Count == 0) return;
		}
		Vector2 difference = currentPatrolPoints[0] - currentPosition;
		if (Mathf.Abs(difference.X) > Math.Abs(difference.Y))
			NpcInput.Direction = difference.X > 0 ? Vector2.Right : Vector2.Left;
		else
			NpcInput.Direction = difference.Y > 0 ? Vector2.Down : Vector2.Up;

		NpcInput.TargetPosition = NpcInput.Direction * Globals.GridSize;
		level.TargetPosition = currentPatrolPoints[0];
		NpcInput.EmitSignal(CharecterInput.SignalName.Walk);
		timer = interval;
	}

	private void HandleWander(double delta, double interval)
	{
		timer -= delta;
		if (timer > 0) return;

		var (direction, targetPosition) = GetNewDirections();
		NpcInput.Direction = direction;
		NpcInput.TargetPosition = targetPosition;
		NpcInput.EmitSignal(CharecterInput.SignalName.Walk);
		timer = interval;
	}

	private void HandleLookAround(double delta, double interval)
	{
		timer -= delta;
		if (timer > 0) return;

		var (direction, targetPosition) = GetNewDirections();
		if (direction == NpcInput.Direction)
		{
			timer = interval;
			return;
		}
		NpcInput.Direction = direction;
		NpcInput.TargetPosition = targetPosition;
		NpcInput.EmitSignal(CharecterInput.SignalName.Turn);
		timer = interval;
	}

	private (Vector2, Vector2) GetNewDirections()
	{
		Vector2[] directions = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
		Vector2 chosenDirection;
		int tries = 0;
		do
		{
			chosenDirection = directions[Globals.GetRandomNumberGenerator().RandiRange(0, directions.Length - 1)];
			Vector2 nextPosition = CharacterMovement.Character.Position + chosenDirection * Globals.GridSize;
			if (NpcInput.NpcInputConfig.NpcMovementType == NpcMovementType.Wander)
			{
				float distanceFromOrigin = nextPosition.DistanceTo(NpcInput.NpcInputConfig.WanderOrigin);
				if (distanceFromOrigin <= NpcInput.NpcInputConfig.WanderRadius)
					break;
			}
			else
			{
				break;
			}
			tries++;
		} while (tries < 10);
		return (chosenDirection, chosenDirection * Globals.GridSize);
	}
}
