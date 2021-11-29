﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBakeTool
{
	class Constants
	{
		public static string helpText = @"

           // Netherlands3D Binary Tiles Generator 0.1 //


This tool parses CityJSON files and bakes them into single-mesh binary tile files.
Seperate metadata files contain the seperation of sub-objects.
Check out http:/3d.amsterdam.nl/netherlands3d for help.

Required options:

--source <path to CityJSON files>
--output <path to tile output folder>

Extra options:

--add						 Add objects to existing binary tile files
--replace					 Replace objects with the same ID
--id <property name>		 Unique ID property name
--type <type filter>		 Filter this type
--id-remove <string>		 Remove this substring from the ID's
--lod <lod filter>			 Target LOD. For example 2.2
--config <config file path>	 Apply settings above via config file
--obj						 Write .OBJ files as well (for previewing outputs)
--brotli					 Write a brotli compressed .br variant of the .bin

Pipeline example:
TileBakeTool.exe --source ""C:/MyProject/CityJsonFiles/*.json"" --output ""C:/MyProject/BinaryTiles/"" --filter-lod ""2"" --filter-type ""gebouw"" --id ""GebouwNummer"" 
TileBakeTool.exe --source ""C:/MyProject/CustomMadeBuildings/*.json""--output ""C:/MyProject/BinaryTiles/"" --id ""BAGID"" --add --replace

Config file example:
#Some comment
lod=2.2
id=building
type=Gebouw

";

	}
}
