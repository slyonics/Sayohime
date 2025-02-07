using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sayohime.Main
{
	public partial class CrossPlatformGame : Game
	{
		private const int MAXIMUM_SCREEN_WIDTH = 1920;
		private const int MAXIMUM_SCREEN_HEIGHT = 1080;
		private const int WINDOWED_MARGIN = 34;

		private int originalHeight = SCREEN_HEIGHT;

		protected override void Initialize()
		{
			graphicsDeviceManager.PreferredBackBufferWidth = scaledScreenWidth;
			graphicsDeviceManager.PreferredBackBufferHeight = scaledScreenHeight;
			graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;

			AssetCache.LoadAssets(Content);

			Settings.DEFAULT_PROGRAM_SETTINGS.Add("Fullscreen", false);
			Settings.DEFAULT_PROGRAM_SETTINGS.Add("TargetResolution", "Best Fit");
			Settings.LoadSettings();

			Input.Initialize();

			base.Initialize();

			bool fullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
			originalHeight = GraphicsDevice.Adapter.CurrentDisplayMode.TitleSafeArea.Height;

			//ApplySettings();
			Audio.ApplySettings();

			spriteBatch = new SpriteBatch(GraphicsDevice);
			gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT);

			

			StartGame();
		}
	}
}
