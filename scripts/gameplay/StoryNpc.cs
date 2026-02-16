using Game.Core;
using Game.Utilities;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System;
using Logger = Game.Core.Logger;


namespace Game.Gameplay;

[Tool]
public partial class StoryNpc : CharacterBody2D
{
	private StoryNpcAppearance npcAppearance = StoryNpcAppearance.OldMan;

	[ExportCategory("Traits")]
	[Export]
	public StoryNpcAppearance NpcAppearance
	{
		get => npcAppearance;
		set
		{
			if (npcAppearance != value)
			{
				npcAppearance = value;
				UpdateAppearence();
				
			}
		}
	}

	private AnimatedSprite2D animatedSprite2D;
	private StoryNpcInput npcInput;
	private StateMachine stateMachine;
	private CharacterMovement characterMovement;

	private readonly System.Collections.Generic.Dictionary<StoryNpcAppearance, SpriteFrames> appearanceFrames = new(){
		{StoryNpcAppearance.Delia, GD.Load<SpriteFrames>("res://resources/spriteframes/Delia.tres")},
		{StoryNpcAppearance.ProfessorOak, GD.Load<SpriteFrames>("res://resources/spriteframes/Oak.tres")},
		{StoryNpcAppearance.OldMan, GD.Load<SpriteFrames>("res://resources/spriteframes/OldMan.tres")},
	};
	[Export]
	public StoryNpcInputConfig NpcInputConfig;
	public override void _Ready(){
		if (Engine.IsEditorHint()) {
			UpdateAppearence();
			return;
		}
		UpdateAppearence();

		npcInput = GetNode<StoryNpcInput>("Input");
		npcInput.NpcInputConfig = NpcInputConfig;

		stateMachine ??= GetNode<StateMachine>("StateMachine");
		stateMachine.ChangeState("Roam");

		animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		characterMovement ??= GetNode<CharacterMovement>("Movement");
		
	}
	public override void _Process(double delta){
		if (Engine.IsEditorHint()) return;
		var player = GameManager.GetPlayer();
		if (player != null) {
			ZIndex = (player.Position.Y <= Position.Y)?6 : 4;
		};
		
	}
	private void UpdateAppearence(){
		Logger.Info($"Updating appearence for {NpcAppearance}");
		
		if (animatedSprite2D == null ){
			animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (animatedSprite2D == null){
				Logger.Error("AnimatedSprite2D not found");
				return;
			}
		}
		
		if (appearanceFrames.TryGetValue(npcAppearance,out var spriteFrames)){
			animatedSprite2D.SpriteFrames = spriteFrames;
		}else{
			animatedSprite2D.SpriteFrames =null;
			Logger.Error($"SpriteFrames not found for {npcAppearance}");
		}
	}
	public async System.Threading.Tasks.Task PlayMessage(Vector2 Direction){
		if (Engine.IsEditorHint()) return;
		if (characterMovement.IsMoving()) return;
		if (npcInput.Direction != Direction * -1)
		{
			npcInput.Direction = Direction * -1;
			npcInput.EmitSignal(CharecterInput.SignalName.Turn);
		}
		stateMachine.ChangeState("Message");
		await MessageManager.PlayText([..NpcInputConfig.Messages]);
	}
}
