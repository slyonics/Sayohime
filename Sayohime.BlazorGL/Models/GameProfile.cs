using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Sayohime.Main;

namespace Sayohime.Models
{
	public static partial class GameProfile
	{

		public static void LoadState(int slot)
		{
			SaveSlot = slot;

			byte[] data = Program.StorageService.GetItem<byte[]>($"Sayohime_Save{SaveSlot}");
			using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
			{
				CurrentSave = new SaveProfile(reader);
			}
		}

		public static void SaveState()
		{
			using (MemoryStream dataStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(dataStream))
				{
					CurrentSave.WriteToFile(writer);
					dataStream.Flush();
				}

				byte[] data = dataStream.ToArray();
				Program.StorageService.SetItem($"Sayohime_Save{SaveSlot}", data);
			}
		}

		public static bool SaveAvailable()
		{
			for (int i = 0; i < 2; i++)
			{
				if (Program.StorageService.ContainKey($"Sayohime_Save{SaveSlot}")) return true;
			}

			return false;
		}
    }
}
