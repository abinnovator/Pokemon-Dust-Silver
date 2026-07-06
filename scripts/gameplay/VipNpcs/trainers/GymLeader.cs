using Game.Core;
using Game.Gameplay;
using Game.Utilities;
using Godot;
using System;

[Tool]
public partial class GymLeader : CharacterBody2D
{
	private GymLeaderAppearanceType _GymLeaderAppearence = GymLeaderAppearanceType.Brock;
	[ExportCategory("Traits")]
	[Export]
	public GymLeaderAppearanceType GymLeaderAppearenceType { get => _GymLeaderAppearence; set
		{
			if (_GymLeaderAppearence != value)
			{
				_GymLeaderAppearence = value;
				UpdateAppearence();
				
			}
		} }
	
	[Export]
	public Resource InputConfig { get; set; }
	private AnimatedSprite2D _animatedSprite2D;
	private StateMachine _stateMachine;
	private CharacterMovement _characterMovement;
	private GymLeaderInput _npcInput;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			UpdateAppearence();
			return;
		}
		UpdateAppearence();

		_npcInput = GetNode<GymLeaderInput>("Input");
		if (InputConfig is GymNpcInputConfig config)
			_npcInput.NpcInputConfig = config;

		_stateMachine ??= GetNode<StateMachine>("StateMachine");
		_animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_characterMovement ??= GetNode<CharacterMovement>("Movement");
	}

	private readonly System.Collections.Generic.Dictionary<GymLeaderAppearanceType, SpriteFrames> _appearanceFrames = new()
	{
		{ GymLeaderAppearanceType.Brock, GD.Load<SpriteFrames>("res://resources/spriteframes/Brock.tres") },
		{ GymLeaderAppearanceType.Misty, GD.Load<SpriteFrames>("res://resources/spriteframes/Misty.tres") },
		{GymLeaderAppearanceType.Giovanni, GD.Load<SpriteFrames>("res://resources/spriteframes/Giovanni.tres")}
	};
	private void UpdateAppearence(){
		Game.Core.Logger.Info($"Updating appearence for {GymLeaderAppearenceType}");
		
		if (_animatedSprite2D == null ){
			_animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animatedSprite2D == null){
				Game.Core.Logger.Error("AnimatedSprite2D not found");
				return;
			}
		}
		
		if (_appearanceFrames.TryGetValue(GymLeaderAppearenceType,out var spriteFrames)){
			_animatedSprite2D.SpriteFrames = spriteFrames;
		}else{
			_animatedSprite2D.SpriteFrames =null;
			Game.Core.Logger.Error($"SpriteFrames not found for {GymLeaderAppearenceType}");
		}
	}

	public async void StartBattle()
	{
		if (InputConfig is not GymNpcInputConfig config)
		{
			Game.Core.Logger.Warning("GymLeader has no valid InputConfig assigned!");
			return;
		}

		if (SaveManager.Instance.CurrentSave.DefeatedTrainers.Contains(config.LeaderName))
		{
			if (config.DefeatMessages.Count > 0)
				await MessageManager.PlayText(null, new string[] { config.DefeatMessages[0] });
			else
				await MessageManager.PlayText(null, new string[] { "..." });
			return;
		}

		Game.Core.BattleManager.Instance.StartGymBattle(config);
	}

}
