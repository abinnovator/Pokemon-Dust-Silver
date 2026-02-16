using Game.Core;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using static Game.Core.MovesEnum;
namespace Game.Gameplay;

[GlobalClass]
[Tool]
public partial class MoveResource : Resource
{
	[ExportCategory("Basic Info")]
	[Export]
	public string Name ="";
	[Export]
	public MoveTarget Target = MoveTarget.SelectedPokemon;
	[Export]
	public MoveCategory Category = MoveCategory.Physical;
	[Export]
	public PokemonType PokemonType = PokemonType.Normal;

	[ExportCategory("Metadata")]
	[Export]
	public int Accuracy;

	[Export]
	public int CritRate;
	[Export]
	public int Drain;
	[Export]
	public int FlinchChance;
	[Export]
	public int Healing;
	[Export]
	public int MaxHits;
	[Export]
	public int MaxTurns;
	[Export]
	public int MinHits;
	[Export]
	public int MinTurns;
	[Export]
	public int Power;
	[Export]
	public int PP;

	[ExportCategory("Move Effects")]

	[Export]
	public int AilmentChance;
	[Export]
	public PokemonAilment Ailment = PokemonAilment.None;
	[Export]
	public Godot.Collections.Dictionary<PokemonStat, int> StatChanges = [];
	
}
