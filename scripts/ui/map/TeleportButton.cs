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
        Pressed += async () =>
        {
            if (!SaveManager.Instance.HasBike())
            {
                await MessageManager.PlayText(null, ["You need a bike to use the teleport!"]);
                return;
            }
            
            GameManager.IsPlayerMovementLocked = true;
            SceneManager.ChangeLevel(levelName, levelId, false, null);
            
            while (SceneManager.isChanging)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            var player = GameManager.GetPlayer();
            GD.Print($"Player position after teleport: {player?.GlobalPosition}");

            var camera = player?.GetNodeOrNull<Camera2D>("Camera2D");
            camera?.ResetSmoothing();
            
            GameManager.IsPlayerMovementLocked = false;
        };
    }

    public override void _Process(double delta)
    {
    }


}
