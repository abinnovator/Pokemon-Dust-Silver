using System;
using Game.Core;
using Godot;
using Godot.Collections;

public partial class TeleportButton : Button
{
    [ExportCategory("Settings")]
    [Export] public LevelName levelName;
    [Export] public int levelId;

    public override void _Ready()
    {
        Pressed += () => SceneManager.ChangeLevel(levelName, levelId, true);
    }

    public override void _Process(double delta)
    {
    }

}
