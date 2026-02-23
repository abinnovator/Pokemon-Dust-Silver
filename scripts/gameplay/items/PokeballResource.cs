using Godot;
using Game.Core;

namespace Game.Gameplay;

[GlobalClass]
[Tool]
public partial class PokeballResource : Resource
{
    [ExportCategory("Basic Info")]
    [Export]
    public string Name;
    [Export]
    public int Id;
    [Export(PropertyHint.MultilineText)]
    public string Description;
    [Export]
    public int Cost;

    [ExportCategory("Capture Mechanics")]
    [Export]
    public float CatchRate = 1.0f;

    [ExportCategory("Visuals")]
    [Export]
    public Texture2D Sprite;
}
