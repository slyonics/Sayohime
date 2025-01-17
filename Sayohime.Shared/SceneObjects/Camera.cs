using Microsoft.Xna.Framework;
using Sayohime.Main;
using System;

namespace Sayohime.SceneObjects
{
	public class Camera
	{
		public const float MINIMUM_ENTITY_DEPTH = 0.2f;
		public const float MAXIMUM_ENTITY_DEPTH = 0.8f;
		public const int LARGEST_ENTITY_SIZE = 48;

		private protected Vector2 position;
		private protected Matrix matrix;
		private protected Rectangle bounds;
		private protected Rectangle view;

		public Camera(Rectangle iBounds)
		{
			bounds = iBounds;
			ClampBounds();
		}

		public void Center(Vector2 target)
		{
			position = target - new Vector2(CrossPlatformGame.SCREEN_WIDTH / 2, CrossPlatformGame.SCREEN_HEIGHT / 2);
			ClampBounds();
		}

		private protected void ClampBounds()
		{
			if (position.X < bounds.Left) position.X = bounds.Left;
			if (position.X > bounds.Right - CrossPlatformGame.SCREEN_WIDTH) position.X = bounds.Right - CrossPlatformGame.SCREEN_WIDTH;
			if (position.Y < bounds.Top) position.Y = bounds.Top;
			if (position.Y > bounds.Bottom - CrossPlatformGame.SCREEN_HEIGHT) position.Y = bounds.Bottom - CrossPlatformGame.SCREEN_HEIGHT;

			matrix = Matrix.CreateTranslation(new Vector3(-((int)position.X + CenteringOffsetX), -((int)position.Y + CenteringOffsetY), 0.0f));
			view = new Rectangle((int)position.X + CenteringOffsetX, (int)position.Y + CenteringOffsetY, CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT);
		}

		public float GetDepth(float z)
		{
			int bottomZ = view.Bottom + LARGEST_ENTITY_SIZE - view.Top;
			float entityZ = z - view.Top;
			float factorZ = entityZ / bottomZ;
			float depth = MathHelper.Lerp(MAXIMUM_ENTITY_DEPTH, MINIMUM_ENTITY_DEPTH, factorZ);

			return depth;
		}

		public Vector2 Position
		{
			get
			{
				return position;
			}

			set
			{
				position = value;
				ClampBounds();
			}
		}

		public Matrix Matrix { get => matrix; }
		public Rectangle View { get => view; }

		public int CenteringOffsetX { get => Math.Max(0, (CrossPlatformGame.SCREEN_WIDTH - bounds.Width) / 2); }
		public int CenteringOffsetY { get => Math.Max(0, (CrossPlatformGame.SCREEN_WIDTH - bounds.Height) / 2); }
		public int MaxVisibleY { get => view.Bottom + LARGEST_ENTITY_SIZE; }
	}
}
