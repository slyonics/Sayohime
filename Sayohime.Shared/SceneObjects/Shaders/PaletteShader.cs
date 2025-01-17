using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;

namespace Sayohime.SceneObjects.Shaders
{
	public class PaletteShader : Shader
	{
		public PaletteShader()
			: base(AssetCache.SHADERS[GameShader.Palette].Clone())
		{
			Effect.Parameters["paletteSprite"].SetValue(AssetCache.SPRITES[GameSprite.Palette]);
		}

		public PaletteShader(float brightness)
			: this()
		{
			SetGlobalBrightness(brightness);
		}

		public void SetGlobalBrightness(float brightness)
		{
			Effect.Parameters["global_brightness"].SetValue(brightness);
		}
	}
}
