using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;

namespace Sayohime.SceneObjects.Shaders
{
	public class ColorFade : Shader
	{
		public ColorFade(Color color, float amount)
			: base(CrossPlatformGame.ContentManager.Load<Effect>("Shaders/ColorFade"))
		{
			Effect.Parameters["filterRed"].SetValue(color.R / 255.0f);
			Effect.Parameters["filterGreen"].SetValue(color.G / 255.0f);
			Effect.Parameters["filterBlue"].SetValue(color.B / 255.0f);
			Amount = amount;
		}

		public float Amount
		{
			set
			{
				Effect.Parameters["amount"].SetValue(value);
			}
		}
	}
}
