using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Sayohime.Main
{
	public static class AssetCache
	{
		public static readonly Dictionary<GameFont, SpriteFont> FONTS = new Dictionary<GameFont, SpriteFont>();
		public static readonly Dictionary<GameMap, LdtkJson> MAPS = new Dictionary<GameMap, LdtkJson>();
		public static readonly Dictionary<GameMusic, Song> MUSIC = new Dictionary<GameMusic, Song>();
		public static readonly Dictionary<GameShader, Effect> SHADERS = new Dictionary<GameShader, Effect>();
		public static readonly Dictionary<GameSound, SoundEffect> SOUNDS = new Dictionary<GameSound, SoundEffect>();
		public static readonly Dictionary<GameSprite, Texture2D> SPRITES = new Dictionary<GameSprite, Texture2D>();
		public static readonly Dictionary<GameView, string> VIEWS = new Dictionary<GameView, string>();

		public static void LoadAssets(ContentManager contentManager)
        {
			foreach (GameFont font in Enum.GetValuesAsUnderlyingType(typeof(GameFont)))
			{
				if (font == GameFont.None) continue;

				SpriteFont mapData = contentManager.Load<SpriteFont>($"Fonts/{font.ToString().Replace('_', '/')}");
				FONTS.Add(font, mapData);
			}

			foreach (GameMap map in Enum.GetValuesAsUnderlyingType(typeof(GameMap)))
            {
                if (map == GameMap.None) continue;

				LdtkJson mapData = LdtkJson.FromJson(contentManager.Load<string>($"Maps/{map.ToString().Replace('_', '/')}"));
                MAPS.Add(map, mapData);
            }

            foreach (GameMusic music in Enum.GetValuesAsUnderlyingType(typeof(GameMusic)))
            {
                if (music == GameMusic.None) continue;

                Song musicData = contentManager.Load<Song>($"Music/{music.ToString().Replace('_', '/')}");
                MUSIC.Add(music, musicData);
            }

            foreach (GameShader shader in Enum.GetValuesAsUnderlyingType(typeof(GameShader)))
            {
                if (shader == GameShader.None) continue;

                Effect shaderData = contentManager.Load<Effect>($"Shaders/{shader.ToString().Replace('_', '/')}");
                SHADERS.Add(shader, shaderData);
            }

            foreach (GameSound sound in Enum.GetValuesAsUnderlyingType(typeof(GameSound)))
            {
                if (sound == GameSound.None) continue;

                SoundEffect soundData = contentManager.Load<SoundEffect>($"Sounds/{sound.ToString().Replace('_', '/')}");
                SOUNDS.Add(sound, soundData);
            }

            foreach (GameSprite sprite in Enum.GetValuesAsUnderlyingType(typeof(GameSprite)))
            {
                if (sprite == GameSprite.None) continue;

                Texture2D spriteData = contentManager.Load<Texture2D>($"Sprites/{sprite.ToString().Replace('_', '/')}");
                SPRITES.Add(sprite, spriteData);
            }

            foreach (GameView view in Enum.GetValuesAsUnderlyingType(typeof(GameView)))
            {
                if (view == GameView.None) continue;

                string viewData = contentManager.Load<string>($"Views/{view.ToString().Replace('_', '/')}");
                VIEWS.Add(view, viewData);
            }
        }

        public static string ReadContentASCII(string path)
		{
			byte[] result = null;
			using (var spriteStream = TitleContainer.OpenStream(CrossPlatformGame.ContentManager.RootDirectory + "/" + path))
			{
				var length = spriteStream.Length;
				if (length <= int.MaxValue)
				{
					result = new byte[length];
					var bytesRead = spriteStream.Read(result, 0, (int)length);
				}
			}

			return Encoding.ASCII.GetString(result);
		}

		public static List<T> LoadRecords<T>(string dataFile)
		{
			Newtonsoft.Json.JsonSerializer deserializer = Newtonsoft.Json.JsonSerializer.Create();
			Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(new StringReader(CrossPlatformGame.ContentManager.Load<string>(dataFile)));
			return deserializer.Deserialize<List<T>>(reader);
		}
	}
}
