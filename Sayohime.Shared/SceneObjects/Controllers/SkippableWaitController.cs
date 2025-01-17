using Microsoft.Xna.Framework;

using Sayohime.Main;

namespace Sayohime.SceneObjects.Controllers
{
	public interface ISkippableWait
	{
		void Notify(SkippableWaitController sender);
		bool Terminated { get; }
	}

	public class SkippableWaitController : Controller
	{
		private const int DEFAULT_WAIT = 3000;

		private ISkippableWait subject;

		private bool skippable;
		private int waitTimeLeft;

		public SkippableWaitController(PriorityLevel iPriorityLevel, ISkippableWait initialSubject, bool iSkippable = true, int iWait = DEFAULT_WAIT)
			: base(iPriorityLevel)
		{
			subject = initialSubject;

			skippable = iSkippable;
			waitTimeLeft = iWait;
		}

		public override void PreUpdate(GameTime gameTime)
		{
			if (subject != null && subject.Terminated)
			{
				Terminate();
				return;
			}
			if (waitTimeLeft <= 0) return;

			InputFrame inputFrame = Input.CurrentInput;
			if (skippable && inputFrame.AnythingPressed())
			{
				subject?.Notify(this);
				Terminate();
			}
			else
			{
				waitTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
				if (waitTimeLeft <= 0) subject?.Notify(this);
				Terminate();
			}
		}

		public void Reset(bool iSkippable = true, int iWait = DEFAULT_WAIT)
		{
			skippable = iSkippable;
			waitTimeLeft = iWait;
		}
	}
}
