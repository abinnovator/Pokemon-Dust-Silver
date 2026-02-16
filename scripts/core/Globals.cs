using Godot;
using System;

namespace Game.Core // Fixed capitalization to match Logger
{
	public partial class Globals : Node
	{
		public static Globals Instance { get; private set; }

		[ExportCategory("Gameplay")]
		public const int GridSize = 16; // Standard C# naming is PascalCase
		public const int MOVE_NUMBERS = 165;
		public const int POKEMON_NUMBERS = 151;
		[Export]
		public ulong Seed = 1337;

		private RandomNumberGenerator randomNumberGenerator;

		public override void _Ready()
		{
			Instance = this;

			randomNumberGenerator = new()
			{
				Seed = Seed
			};
			Logger.Info("Loading globals...");
		}
		public static RandomNumberGenerator GetRandomNumberGenerator()
		{
			return Instance.randomNumberGenerator;
		} 
	}
}
