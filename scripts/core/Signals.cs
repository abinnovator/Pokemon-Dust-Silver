using Godot;
using System;

namespace Game.Core // Fixed capitalization to match Logger
{
	public partial class Signals : Node
	{
		public static Signals Instance { get; private set; }

		[Signal] public delegate void MessageBoxOpenEventHandler(bool value);
		[Signal] public delegate void MessageBoxCloseEventHandler();


		public override void _Ready()
		{
			Instance = this;
			Logger.Info("Loading Signals...");
		}
		public static void EmitGlobalSignal(StringName signal, params Variant[] args){
			Logger.Info($"Emitting global signal {signal}");
			Instance.EmitSignal(signal, args);
		}
	}
}
