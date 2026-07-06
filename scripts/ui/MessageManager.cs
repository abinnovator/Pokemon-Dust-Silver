using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace Game.Core
{
	public partial class MessageManager : CanvasLayer
	{
		public static MessageManager Instance { get; private set; }
		
		[ExportCategory("Components")]
		[Export] public NinePatchRect Box;
		[Export] public RichTextLabel TextLabel;

		[ExportCategory("Variables")]
		[Export] public bool isScrolling = false;
		[Export] public int Delay = 15;
		[Export] public Array<string> Messages;
		[Export] public AtlasTexture picture;
		[Export] public Sprite2D pictureSprite;

		public override void _Ready()
		{
			Instance = this;
			if (Box != null) Box.Visible = false;
		}

		public static async Task<bool> PlayText(Texture2D picture, string[] payload)
		{
			if (IsReading()) return false;
			
			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);
			Instance.Messages = [..payload];
			Instance.pictureSprite.Texture = picture;
			Instance.pictureSprite.Visible = picture != null;

			
			while (Instance.Messages.Count > 0)
			{
				await ScrollText(); 
				
				await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
				await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
				
				while (!Input.IsActionJustPressed("use")) 
				{
					await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}

			bool result = true;
			
			Instance.Box.Visible = false;
			Instance.pictureSprite.Visible = false;
			
			await Task.Delay(200); 
			
			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, false);
			
			return result;
		}
		
		private static void CloseBox()
		{
			Instance.Box.Visible = false;
			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, false);
		}

		public static async Task ScrollText()
		{
			if (!IsReading())
			{
				Instance.Box.Visible = true;
			}
			
			Instance.isScrolling = true;
			Instance.TextLabel.Text = "";

			foreach(char letter in Instance.Messages[0])
			{
				Instance.TextLabel.Text += letter;
				await Task.Delay(Instance.Delay);
			}
			
			Instance.Messages.RemoveAt(0);
			Instance.isScrolling = false;
		}

		public static bool IsReading() => Instance.Box.Visible;
		public static bool Scrolling() => Instance.isScrolling;
	}
}
