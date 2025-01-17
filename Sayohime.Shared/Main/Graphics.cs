using Microsoft.Xna.Framework;
using System.Globalization;

namespace Sayohime.Main
{
	public static class Graphics
	{
		public static readonly Color PURE_BLACK = new Color(1, 1, 1, 255);
		public static readonly Color PURE_WHITE = new Color(250, 250, 250, 255);

		public enum PaletteHue
		{
			Unused1,
			DarkGrey,
			Red,
			DarkOrange,
			LightOrange,
			Yellow,
			LightGreen,
			Green,
			DarkGreen,
			Teal,
			LightBlue,
			DarkBlue,
			Purple,
			LightPurple,
			LightGrey,
			Unused2
		}

		public static Color ParseHexcode(string hexCode)
		{
			int red = int.Parse(hexCode.Substring(1, 2), NumberStyles.HexNumber);
			int green = int.Parse(hexCode.Substring(3, 2), NumberStyles.HexNumber);
			int blue = int.Parse(hexCode.Substring(5, 2), NumberStyles.HexNumber);
			int alpha = (hexCode.Length > 7) ? int.Parse(hexCode.Substring(7, 2), NumberStyles.HexNumber) : 255;

			return new Color(red, green, blue, alpha);
		}
    }
}
