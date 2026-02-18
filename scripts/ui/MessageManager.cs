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

			bool result = true;
			
			// 3. THE FIX: Visual cleanup first, but delay the state change
			Instance.Box.Visible = false;
			
			// Wait to clear inputs to prevent re-triggering in the same frame
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
