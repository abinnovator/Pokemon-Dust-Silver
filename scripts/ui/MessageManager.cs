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

		public override void _Ready()
		{
			Instance = this;
			// Ensure box is hidden at start
			if (Box != null) Box.Visible = false;
		}

		public static async Task<bool> PlayText(params string[] payload)
		{
			if (IsReading()) return false;
			
			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);
			Instance.Messages = [..payload];
			
			// 1. Loop through all dialogue pages
			while (Instance.Messages.Count > 0)
			{
				await ScrollText(); 
				
				// Wait for player to press 'use' to continue
				// We add a tiny frame delay to ensure the press is fresh
				await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
				
				while (!Input.IsActionJustPressed("use")) 
				{
					await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}

			// 2. Final Choice Phase
			// This loop waits specifically for a Yes (use) or No (back)
			bool result = false;
			while (true)
			{
				if (Input.IsActionJustPressed("use")) 
				{
					result = true; 
					break;
				}
				if (Input.IsActionJustPressed("back"))
				{
					result = false; 
					break;
				}
				await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			// 3. THE FIX: Clean up before closing
			CloseBox();
			
			// Wait 100ms or a few frames before returning. 
			// This prevents the Player script from "seeing" the Space press and interacting again!
			await Task.Delay(1500); 
			
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

			// Typewriter effect
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
