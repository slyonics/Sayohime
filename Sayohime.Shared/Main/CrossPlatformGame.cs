using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Shaders;
using Sayohime.Scenes.CrawlerScene;
using Sayohime.Scenes.MapScene;

namespace Sayohime.Main
{
	public partial class CrossPlatformGame : Game
	{
		public const string GAME_NAME = "Sayohime";
		public const int SCREEN_WIDTH = 880;
		public const int SCREEN_HEIGHT = 540;

		private static readonly Color CLEAR_COLOR = Main.Graphics.PURE_BLACK;

		private GraphicsDeviceManager graphicsDeviceManager;
		private SpriteBatch spriteBatch;
		private RenderTarget2D gameRender;

		private static Scene pendingScene;

		public static Scene CurrentScene { get; private set; }
		public static List<Scene> SceneStack { get; } = new List<Scene>();
		public static Shader TransitionShader { get; set; }
		public static Vector2 ScreenShake { get; set; }

		public static GraphicsDevice Graphics { get => gameInstance.GraphicsDevice; }
		public static ContentManager ContentManager { get => gameInstance.Content; }
		public static CrossPlatformGame GameInstance { get => gameInstance; }

		private static int scaledScreenWidth = SCREEN_WIDTH;
		private static int scaledScreenHeight = SCREEN_HEIGHT;

		private static CrossPlatformGame gameInstance;

		public CrossPlatformGame()
		{
			gameInstance = this;
			graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = scaledScreenWidth,
				PreferredBackBufferHeight = scaledScreenHeight,
				GraphicsProfile = GraphicsProfile.HiDef
			};

			Content.RootDirectory = "Content";
		}

		protected override void LoadContent()
		{
			Debug.Initialize(GraphicsDevice);

			if (ConversationRecord.CONVERSATIONS == null) ConversationRecord.CONVERSATIONS = AssetCache.LoadRecords<ConversationRecord>("Data/ConversationData");
			if (HeroRecord.HEROES == null) HeroRecord.HEROES = AssetCache.LoadRecords<HeroRecord>("Data/HeroData");
			if (EnemyRecord.ENEMIES == null) EnemyRecord.ENEMIES = AssetCache.LoadRecords<EnemyRecord>("Data/EnemyData");
            if (ItemRecord.ITEMS == null) ItemRecord.ITEMS = AssetCache.LoadRecords<ItemRecord>("Data/ItemData");

			// BattleScene.Initialize();
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
			gameRender?.Dispose();
		}

		private void StartGame()
		{
			GameProfile.NewState();

			var mapScene = new CrawlerScene(GameMap.SandOne, 10, 3, Direction.North); //new MapScene(GameMap.Matsuura, "Default");
			CurrentScene = mapScene;
			CurrentScene.BeginScene();
		}

		protected override void Update(GameTime gameTime)
		{
			if (CurrentScene == null) return;

			Input.Update(gameTime);

			if (TransitionShader != null)
			{
				CurrentScene.Update(gameTime);
				TransitionShader.Update(gameTime, null);
				if (TransitionShader.Terminated) TransitionShader = null;
			}
			else
			{
				int i = 0;
				while (i < SceneStack.Count)
				{
					SceneStack[i].Update(gameTime);
					i++;
				}

				CurrentScene.Update(gameTime);
			}

			if (pendingScene != null)
			{
				SceneStack.Clear();
				SetCurrentScene(pendingScene);
				pendingScene = null;
			}

			while (CurrentScene.SceneEnded && SceneStack.Count > 0)
			{
				var previousScene = CurrentScene;

				CurrentScene = SceneStack.Last();
				CurrentScene.ResumeScene();
				SceneStack.Remove(CurrentScene);
				TransitionShader = null;

				previousScene.OnRemoval?.Invoke();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			if (CurrentScene == null) return;

			lock (SceneStack)
			{
				if (SceneStack.Count > 0)
				{
					foreach (Scene scene in SceneStack)
					{
						scene.Draw(GraphicsDevice, spriteBatch, gameRender);
					}
				}
			}

			CurrentScene.Draw(GraphicsDevice, spriteBatch, gameRender);

			/*
			GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
			spriteBatch.Draw(gameRender, ScreenShake, Color.White);
			spriteBatch.End();
			*/

			/*
			int currentWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
			int currentHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
			int scale = currentHeight / SCREEN_HEIGHT;
			Matrix matrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(currentWidth % SCREEN_WIDTH / 2, currentHeight % SCREEN_HEIGHT / 2, 0);

			Effect shader = TransitionShader == null ? null : TransitionShader.Effect;
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(CLEAR_COLOR);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
			spriteBatch.Draw(gameRender, ScreenShake, Color.White);
			spriteBatch.End();
			*/

			base.Draw(gameTime);
		}

		public static void Transition(Task<Scene> sceneTask)
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            
			Transition(CurrentScene, sceneTask, transitionController, colorFade);
        }

		public static void Transition(Scene parentScene, Task<Scene> sceneTask, TransitionController transitionController, Shader transitionShader)
		{
			parentScene.AddController(transitionController);
			TransitionShader = transitionShader;

			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (sceneTask.IsCompleted) pendingScene = sceneTask.Result;
				transitionShader?.Terminate();
			});

			sceneTask.ContinueWith(t =>
			{
				if (transitionController.Terminated)
				{
					pendingScene = sceneTask.Result;
					transitionShader?.Terminate();
				}
			});

			sceneTask.Start();
		}

		public static void SetCurrentScene(Scene newScene)
		{
			CurrentScene.EndScene();

			TransitionShader?.Terminate();
			CurrentScene = newScene;

			if (newScene.Suspended) newScene.ResumeScene();
			else newScene.BeginScene();
		}

		public static void StackScene(Scene newScene, bool suspended = false)
		{
			lock (SceneStack)
			{
				SceneStack.Add(CurrentScene);
			}

			CurrentScene.Suspended = suspended;
			var oldScene = CurrentScene;

			if (newScene != null) newScene.OnTerminated += new TerminationFollowup(() => oldScene.Suspended = false);

			Task.Run(() =>
			{
				while (newScene == null) Thread.Sleep(100);
				CurrentScene = newScene;
				newScene.BeginScene();
			});
		}

		public static T GetScene<T>() where T : Scene
		{
			if (CurrentScene is T) return (T)CurrentScene;
			else
			{
				T result;
				lock (SceneStack)
				{
					result = (T)SceneStack.FirstOrDefault(x => x is T);
				}

				return result;
			}
		}
	}
}
