using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sayohime.Main;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Sayohime.SceneObjects
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public class Scene
	{
		public delegate void SceneEndCallback();

		protected List<Overlay> overlayList = new List<Overlay>();
		protected List<Controller>[] controllerList = new List<Controller>[Enum.GetNames(typeof(PriorityLevel)).Length];
		protected PriorityLevel priorityLevel;

		protected List<Entity> entityList = new List<Entity>();
		protected List<Particle> particleList = new List<Particle>();

		protected Shader backgroundShader;
		protected Shader entityShader;
		protected Shader interfaceShader;

		public Shader SceneShader { get; set; }

		protected bool sceneStarted;
		protected bool sceneEnded;

		public Scene()
		{
			foreach (int controlLevel in Enum.GetValues<PriorityLevel>())
			{
				controllerList[controlLevel] = new List<Controller>();
			}
		}

		public virtual void BeginScene()
		{
			sceneStarted = true;

			TransitionController transitionController = new TransitionController(TransitionDirection.In, 600);
			ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
			transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
			transitionController.FinishTransition += new Action<TransitionDirection>(t => colorFade.Terminate());
			AddController(transitionController);
			CrossPlatformGame.TransitionShader = colorFade;
		}

		public virtual void ResumeScene()
		{
			Suspended = false;
			sceneEnded = false;
		}

		public virtual void EndScene()
		{
			if (!sceneEnded)
			{
				sceneEnded = true;
				OnTerminated?.Invoke();
			}
		}

		public virtual void Update(GameTime gameTime)
		{
			if (Suspended) return;

			int i = 0;

			priorityLevel = PriorityLevel.GameLevel;
			for (i = 0; i < Enum.GetNames(typeof(PriorityLevel)).Length; i++)
			{
				if (controllerList[i].Count > 0) priorityLevel = (PriorityLevel)i;
			}

			List<Controller> activeControllers = controllerList.LastOrDefault(x => x.All(y => y.PriorityLevel >= priorityLevel) && x.Count > 0);

			if (activeControllers != null)
			{
				i = 0;
				PriorityLevel startingPriority = priorityLevel;
				while (i < activeControllers.Count)
				{
					activeControllers[i].PreUpdate(gameTime);
					i++;

					if (startingPriority != priorityLevel) break;
				}

				activeControllers.RemoveAll(x => x.Terminated);
			}

			i = 0;

			if (priorityLevel < PriorityLevel.TransitionLevel)
			{
				PriorityLevel startingPriority = priorityLevel;
				while (i < overlayList.Count)
				{
					overlayList[i].Update(gameTime);
					i++;

					if (startingPriority != priorityLevel) break;
				}
				overlayList.RemoveAll(x => x.Terminated);
			}

			var entities = entityList.FindAll(x => x.PriorityLevel >= priorityLevel);
			foreach (Entity entity in entities)
			{
				entity.Update(gameTime);
			}
			entityList.RemoveAll(x => x.Terminated);

			i = 0;
			while (i < particleList.Count) { particleList[i].Update(gameTime); i++; }
			particleList.RemoveAll(x => x.Terminated);

			int j = 0;
			while (j < controllerList.Length)
			{
				i = 0;
				while (i < controllerList[j].Count)
				{
					controllerList[j][i].PostUpdate(gameTime);
					i++;
				}
				j++;
			}

			if (entityShader != null)
			{
				entityShader.Update(gameTime, Camera);
				if (entityShader.Terminated) entityShader = null;
			}

			if (SceneShader != null)
			{
				SceneShader.Update(gameTime, Camera);
				if (SceneShader.Terminated) SceneShader = null;
			}
		}

		public virtual void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D gameRender)
		{
			Effect shader;
			Matrix cameraMatrix = (Camera == null) ? Matrix.Identity : Camera.Matrix;
			
			graphicsDevice.SetRenderTarget(gameRender);
			if (CrossPlatformGame.SceneStack.Count == 0 || CrossPlatformGame.SceneStack.First() == this)
			{
				graphicsDevice.Clear(new Color(0));
			}

			
			shader = (backgroundShader == null) ? null : backgroundShader.Effect;
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
			DrawBackground(spriteBatch);
			spriteBatch.End();

			shader = (entityShader == null) ? null : entityShader.Effect;
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, cameraMatrix);
			DrawGame(spriteBatch, shader, cameraMatrix);
			spriteBatch.End();
			



			if (OverlayList.Count > 0)
			{
				shader = (interfaceShader == null) ? null : interfaceShader.Effect;
				spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
				DrawOverlay(spriteBatch);
				spriteBatch.End();
			}
		}

		public virtual void DrawBackground(SpriteBatch spriteBatch)
		{

		}

		public virtual void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
		{
			foreach (Entity entity in entityList) entity.Draw(spriteBatch, Camera);
			foreach (Particle particle in particleList) particle.Draw(spriteBatch, Camera);
		}

		public virtual void DrawOverlay(SpriteBatch spriteBatch)
		{
			foreach (Overlay overlay in overlayList) overlay.Draw(spriteBatch);
		}

		public virtual T AddParticle<T>(T newParticle) where T : Particle
		{
			particleList.Add(newParticle);
			return newParticle;
		}

		public T AddOverlay<T>(T newOverlay) where T : Overlay
		{
			overlayList.Add(newOverlay);
			return newOverlay;
		}

		public T AddController<T>(T newController, int index = -1) where T : Controller
		{
			if (index == -1) controllerList[(int)newController.PriorityLevel].Add(newController);
			else controllerList[(int)newController.PriorityLevel].Insert(index, newController);

			if (priorityLevel < newController.PriorityLevel) priorityLevel = newController.PriorityLevel;

			return newController;
		}

		public T AddEntity<T>(T newEntity) where T : Entity
		{
			entityList.Add(newEntity);
			return newEntity;
		}

		public T AddView<T>(T viewModel) where T : ViewModel
		{
			AddOverlay(viewModel);

			return viewModel;
		}

		public void ClearTerminationFollow()
		{
			foreach (Delegate d in OnTerminated.GetInvocationList())
			{
				OnTerminated -= (TerminationFollowup)d;
			}
		}

		public event TerminationFollowup OnTerminated;
		public Action OnRemoval;

		public PriorityLevel PriorityLevel { get => priorityLevel; }
		public List<Controller>[] ControllerStack { get => controllerList; }
		public List<Overlay> OverlayList { get => overlayList; }
		public Camera Camera { get; protected set; }
		public List<Entity> EntityList { get => entityList; }
		public bool SceneEnded { get => sceneEnded; }
		public bool Suspended { get; set; }
	}
}
