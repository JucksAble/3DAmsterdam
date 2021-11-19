﻿using System;
using System.IO;
using System.Reflection;
using TileBakeLibrary;

namespace TileBakeTool
{
	class Program
	{
		private static string sourcePath = "";
		private static string outputPath = "";
		private static string newline = "\n";

		static void Main(string[] args)
		{
			if(args.Length == 0)
			{
				ShowHelp();
			}

			ParseArguments(args);
		}

		private static void ShowHelp()
		{
			Console.Write($"This tool converts CityJSON files into Netherlands3D binary tile files.{newline}");
			Console.Write($"{newline}");
			Console.Write($"Check out http:/3d.amsterdam.nl/netherlands3d for help.{newline}");
			Console.Write($"Required options:{newline}");
			Console.Write($"--source <pathToSourceFolder>{newline}");
			Console.Write($"--output <pathToTileOutputFolder>{newline}");
			Console.Write($"{newline}");
			Console.Write($"Extra options:{newline}");
			Console.Write($"{newline}");
			Console.Write($"--add                  Add objects to existing binary tile files{newline}");
			Console.Write($"--replace              Replace objects with the same ID{newline}");
			Console.Write($"--id <property name>   Unique ID property name{newline}");
			Console.Write($"--filter-type <type>   Filter object on type{newline}");
			Console.Write($"--filter-lod <lod>     Filter LOD level{newline}");
			Console.Write($"{newline}");
			Console.Write($"Pipeline example:{newline}");
			Console.Write($"{newline}");
			Console.Write($"TileBakeTool.exe --source \"C:/MyProject/CityJsonFiles/*.json\" --output \"C:/MyProject/BinaryTiles/\" --filter-lod \"2.2\" --filter-type \"gebouw\" --id \"GebouwNummer\"{newline}");
			Console.Write($"TileBakeTool.exe --source \"C:/MyProject/CustomMadeBuildings/*.json\" --output \"C:/MyProject/BinaryTiles/\" --id \"BAGID\" --add --replace");
		}

		private static void StartConverting(string sourcePath, string targetPath)
		{
			var tileBaker = new CityJSONToTileConverter();
			tileBaker.SetSourcePath(sourcePath);
			tileBaker.SetTargetPath(targetPath);

			tileBaker.Convert();
		}

		private static void ParseArguments(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				var argument = args[i];
				if (argument.Contains("--")){
					var value = (i + 1 < args.Length) ? "" : args[i + 1];
					ApplySetting(argument,value);
				}
			}

			if(sourcePath != "" && outputPath != "")
				StartConverting(sourcePath, outputPath);
		}

		private static void ApplySetting(string argument, string value)
		{
			switch (argument)
			{
				case "--source":
					sourcePath = value;
					break;
				case "--output":
					outputPath = value;
					break;
				default:
					break;
			}
		}
	}
}
