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
			Settings.DEFAULT_PROGRAM_SETTINGS.Add("Fullscreen", false);
			Settings.DEFAULT_PROGRAM_SETTINGS.Add("TargetResolution", "Best Fit");
			Settings.LoadSettings();

			Input.Initialize();

			base.Initialize();

			bool fullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
			originalHeight = GraphicsDevice.Adapter.CurrentDisplayMode.TitleSafeArea.Height;

			ApplySettings();
			Audio.ApplySettings();

			spriteBatch = new SpriteBatch(GraphicsDevice);
			gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT);

			StartGame();
		}

		public void ApplySettings()
		{
			if (fullscreen)
			{
				DisplayModeCollection displayModes = GraphicsDevice.Adapter.SupportedDisplayModes;
				IEnumerable<DisplayMode> bestModes = displayModes.Where(x => x.Width >= SCREEN_WIDTH && x.Width <= MAXIMUM_SCREEN_WIDTH &&
																				x.Height >= SCREEN_HEIGHT && x.Height <= MAXIMUM_SCREEN_HEIGHT);

				DisplayMode targetMode = bestModes.OrderByDescending(x => x.Width).FirstOrDefault();
				scaledScreenWidth = targetMode.Width;
				scaledScreenHeight = targetMode.Height;
				int scale = targetMode.Height / SCREEN_HEIGHT;
				screenScale = scale;
			}
			else
			{
				string targetResolution = Settings.GetProgramSetting<string>("TargetResolution");

				if (targetResolution == "Best Fit")
				{
					int availableHeight = originalHeight - WINDOWED_MARGIN;
					int scale = availableHeight / SCREEN_HEIGHT;

					screenScale = scale;
					scaledScreenWidth = SCREEN_WIDTH * scale;
					scaledScreenHeight = SCREEN_HEIGHT * scale;
				}
				else
				{
					screenScale = 1;
					scaledScreenWidth = SCREEN_WIDTH;
					scaledScreenHeight = SCREEN_HEIGHT;
				}

				graphicsDeviceManager.IsFullScreen = false;
				graphicsDeviceManager.PreferredBackBufferWidth = scaledScreenWidth;
				graphicsDeviceManager.PreferredBackBufferHeight = scaledScreenHeight;
				graphicsDeviceManager.ApplyChanges();

				if (screenScale == 1) scaleMatrix = Matrix.Identity;
				else
				{
					scaleMatrix = Matrix.CreateScale(screenScale, screenScale, 1);
				}
			}
		}
	}
}
