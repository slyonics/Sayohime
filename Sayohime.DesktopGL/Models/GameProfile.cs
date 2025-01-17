using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sayohime.Main;

namespace Sayohime.Models
{
	public static partial class GameProfile
	{
		public static readonly string SETTINGS_DIRECTORY = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "AppData\\Local") + "\\" + CrossPlatformGame.GAME_NAME;
		public const string SAVE_FOLDER = "\\Save";

		public static void LoadState(int slot)
		{
            SaveSlot = slot;

            FileInfo fileInfo = new FileInfo($"{SETTINGS_DIRECTORY}{SAVE_FOLDER}/Save{SaveSlot}.sav");
			using (FileStream fileStream = fileInfo.OpenRead())
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					CurrentSave = new SaveProfile(reader);
				}
			}
		}

		public static void SaveState()
		{
			string directory = SETTINGS_DIRECTORY + SAVE_FOLDER;
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using (FileStream fileStream = File.Open($"{directory}\\Save{SaveSlot}.sav", FileMode.OpenOrCreate))
			{
				using (BinaryWriter writer = new BinaryWriter(fileStream))
				{
					CurrentSave.WriteToFile(writer);
					fileStream.Flush();
				}
			}
		}

		public static bool SaveAvailable()
		{
			string directory = SETTINGS_DIRECTORY + SAVE_FOLDER;
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				return false;
			}

			var saveFiles = Directory.EnumerateFiles(directory);
			return saveFiles.Any();
		}
	}
}
