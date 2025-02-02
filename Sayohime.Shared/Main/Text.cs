using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sayohime.Main
{
	public static class Text
	{
		public class GameFontData
		{
			public int fontHeight;
			public int heightOffset;
		}

		private const float TEXT_DEPTH = 0.1f;

		public static readonly Dictionary<GameFont, GameFontData> FONT_DATA = new Dictionary<GameFont, GameFontData>()
		{
			{ GameFont.Interface, new GameFontData() { fontHeight = 7 } },
			{ GameFont.Main, new GameFontData() { fontHeight = 11, heightOffset = -2 } }
		};

		public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
		{
			Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
		}

		public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
		{
			Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
		}

		public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
		{
			Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
		}

		public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
		{
			Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
		}

		public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
		{
			Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
		}

		public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
		{
			Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
		}

		public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
		{
			Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
		}


		public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
		{
			Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
		}

		public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, Color color, int row = 0)
		{
			Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
			spriteBatch.DrawString(AssetCache.FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
		}

		public static int GetStringLength(GameFont font, string text)
		{
			return (int)AssetCache.FONTS[font].MeasureString(text).X;
		}

		public static int GetStringHeight(GameFont font)
		{
			return FONT_DATA[font].fontHeight;
		}
	}
}
