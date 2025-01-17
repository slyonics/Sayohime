using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sayohime.Main
{
	public partial class CrossPlatformGame : Game
	{
		protected override void Initialize()
		{
			Input.Initialize();

			base.Initialize();

			spriteBatch = new SpriteBatch(GraphicsDevice);
			gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT);

			StartGame();
		}
	}
}
