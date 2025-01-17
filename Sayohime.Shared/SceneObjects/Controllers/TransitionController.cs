using Sayohime.Main;
using Microsoft.Xna.Framework;
using System;

namespace Sayohime.SceneObjects.Controllers
{
	public enum TransitionDirection
	{
		In,
		Out
	}

	public class TransitionController : Controller
	{
		private TransitionDirection direction;
		private float transitionTime;
		private float length;
		private bool skippable;

		public TransitionController(TransitionDirection iDirection, int iLength, PriorityLevel iPriorityLevel = PriorityLevel.TransitionLevel, bool iSkippable = false)
			: base(iPriorityLevel)
		{
			direction = iDirection;
			transitionTime = 0;
			length = iLength;
			skippable = iSkippable;
		}

		public override void PreUpdate(GameTime gameTime)
		{
			transitionTime += gameTime.ElapsedGameTime.Milliseconds;
			if ((transitionTime >= length) || (skippable && (Input.CurrentInput.CommandPressed(Command.Confirm) || Input.CurrentInput.CommandPressed(Command.Interact)))) Finish();
			else UpdateTransition?.Invoke(TransitionProgress);
		}

		public void Finish()
		{
			transitionTime = length;
			UpdateTransition?.Invoke(TransitionProgress);
			FinishTransition?.Invoke(direction);
			Terminate();
		}

		public float TransitionProgress { get => (direction == TransitionDirection.In) ? transitionTime / length : 1.0f - (transitionTime / length); }

		public event Action<float> UpdateTransition;
		public event Action<TransitionDirection> FinishTransition;
	}
}
