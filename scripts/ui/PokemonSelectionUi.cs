using Godot;
using Godot.Collections;
using System.Threading.Tasks;
using Game.Core;

public partial class PokemonSelectionUi : CanvasLayer
{
	public static PokemonSelectionUi Instance { get; private set; }

	[ExportCategory("Components")]
	[Export] public NinePatchRect Box;
	[Export] public RichTextLabel TextLabel;
	[Export] public TextureRect PokemonSprite; 

	[ExportCategory("Variables")]
	[Export] public int Delay = 15;
	
	private Array<string> _messages = new Array<string>();
	private bool _isScrolling = false;
	private bool _waitingForChoice = false;
	private string _currentPokeName;

	public override void _Ready()
	{
		Instance = this;
		
		// Fix NRE: Get nodes if not assigned in inspector
		if (Box == null) Box = GetNode<NinePatchRect>("Control/NinePatchRect");
		if (TextLabel == null) TextLabel = GetNode<RichTextLabel>("Control/NinePatchRect/RichTextLabel");
		
		if (Box != null) Box.Visible = false;
		
		// Fix Input Blocking: Ensure the main control doesn't block mouse events
		GetNode<Control>("Control").MouseFilter = Control.MouseFilterEnum.Ignore;
		
		SetProcessInput(false); // Only listen for input when the UI is open
	}

	public static void OpenSelection(string name, string type, Texture2D sprite)
	{
		if (Instance._isScrolling || Instance.Box.Visible) return;

		Instance._currentPokeName = name;
		Instance.PokemonSprite.Texture = sprite;
		Instance.PokemonSprite.Visible = true;
		
		Instance._messages = new Array<string> {
			$"It's a {name}!",
			$"This {type} type Pokemon looks energetic.",
			$"Choose {name}? [Space: Yes / Backspace: No]"
		};

		Instance.Box.Visible = true;
		Instance.SetProcessInput(true);
		ScrollNextMessage();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_waitingForChoice || _isScrolling) return;

		if (@event.IsActionPressed("ui_accept")) // Usually mapped to Space/Enter
		{
			ConfirmChoice();
		}
		else if (Input.IsKeyPressed(Key.Backspace)) // Explicit Backspace check
		{
			DeclineChoice();
		}
	}

	public static async void ScrollNextMessage()
	{
		if (Instance._messages.Count == 0)
		{
			Instance._waitingForChoice = true;
			return;
		}

		Instance._isScrolling = true;
		Instance.TextLabel.Text = "";

		foreach (char letter in Instance._messages[0])
		{
			Instance.TextLabel.Text += letter;
			await Task.Delay(Instance.Delay);
		}

		Instance._messages.RemoveAt(0);
		Instance._isScrolling = false;
		
		// Brief pause between messages or wait for a click to continue
		await Task.Delay(500); 
		ScrollNextMessage();
	}

	private void ConfirmChoice()
	{
		GD.Print($"Player chose {_currentPokeName}!");
		// TODO: Add to Party logic here
		Close();
	}

	private void DeclineChoice()
	{
		GD.Print("Player changed their mind.");
		Close();
	}

	public void Close()
	{
		Box.Visible = false;
		Instance.PokemonSprite.Visible = false;
		_messages.Clear();
		_waitingForChoice = false;
		SetProcessInput(false);
	}
}
