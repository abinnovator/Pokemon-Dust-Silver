using Godot;
using System;
using System.Diagnostics;

namespace Game.Core
{

	public static class Logger
	{
		public static void Log(LevelLog level, params object[] message)
		{
			var dateTime = DateTime.Now;
			string timeStamp = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
			
			var stackTrace = new StackTrace();
			var frame = stackTrace.GetFrame(2);
			var method = frame.GetMethod();
			string className = method.DeclaringType.Name;
			
			string prefix = $"{timeStamp} [{level}] [{className}.{method.Name}] ";
			string finalMessage = prefix + string.Join(" ", message);

			// FIX: Use double quotes (") for strings. Single quotes (') are for chars.
			string color = level switch 
			{
				LevelLog.Debug => "gray",
				LevelLog.Info => "cyan",
				LevelLog.Warning => "yellow",
				LevelLog.Error => "red",
				_ => "white"
			};

			// This prints to the Godot console with the specified color
			GD.PrintRich($"[color={color}]{finalMessage}[/color]");
		}

		public static void Debug(params object[] message) => Log(LevelLog.Debug, message);
		public static void Info(params object[] message) => Log(LevelLog.Info, message);
		public static void Warning(params object[] message) => Log(LevelLog.Warning, message);
		public static void Error(params object[] message) => Log(LevelLog.Error, message);
	}
}
