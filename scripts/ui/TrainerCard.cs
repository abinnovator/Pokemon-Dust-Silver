using System;
using Godot;
using Game.Core;

public partial class TrainerCard : Node2D
{
	[Export] public RichTextLabel Money;
	[Export] public Sprite2D Boulder_badge;
	[Export] public Sprite2D Cascade_badge;
	[Export] public Sprite2D Thunder_badge;
	[Export] public Sprite2D Rainbow_badge;
	[Export] public Sprite2D Soul_badge;
	[Export] public Sprite2D Marsh_badge;
	[Export] public Sprite2D Volcano_badge;
	[Export] public Sprite2D Earth_badge;

	public override void _Ready()
	{
		var save = SaveManager.Instance.CurrentSave;
		Money.Text = save.Money.ToString();
		UpdateBadges(save);
	}

	private void UpdateBadges(PlayerSaveResource save)
	{
		var badges = save.Badges;

		Boulder_badge.Visible = badges.Contains(Badge.BOULDER);
		Cascade_badge.Visible = badges.Contains(Badge.CASCADE);
		Thunder_badge.Visible = badges.Contains(Badge.THUNDER);
		Rainbow_badge.Visible = badges.Contains(Badge.RAINBOW);
		Soul_badge.Visible = badges.Contains(Badge.SOUL);
		Marsh_badge.Visible = badges.Contains(Badge.MARSH);
		Volcano_badge.Visible = badges.Contains(Badge.VOLCANO);
		Earth_badge.Visible = badges.Contains(Badge.EARTH);
	}
}
