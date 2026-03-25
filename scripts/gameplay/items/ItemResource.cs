using Godot;
using Game.Core;

namespace Game.Gameplay;

[GlobalClass]
[Tool]
public partial class ItemResource : Resource
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

    [ExportCategory("Category")]
    [Export]
    public string Category;
    [Export]
    public string[] Attributes;

    [ExportCategory("Battle")]
    [Export]
    public string ShortEffect;
    [Export(PropertyHint.MultilineText)]
    public string Effect;
    [Export]
    public int FlingPower;

    [ExportCategory("Visuals")]
    [Export]
    public Texture2D Sprite;
}