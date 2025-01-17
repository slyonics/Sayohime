using Microsoft.Xna.Framework;

namespace Sayohime.SceneObjects.Controllers
{
	public abstract class ScriptController : Controller, IScripted
	{
		protected ScriptParser scriptParser;

		public ScriptController(Scene iParentScene, string script, PriorityLevel iPriorityLevel)
			: base(iPriorityLevel)
		{
			scriptParser = new ScriptParser(iParentScene, this);
			scriptParser.RunScript(script);
		}

		public ScriptController(Scene iParentScene, string[] script, PriorityLevel iPriorityLevel)
			: base(iPriorityLevel)
		{
			scriptParser = new ScriptParser(iParentScene, this);
			scriptParser.RunScript(script);
		}

		public override void PreUpdate(GameTime gameTime)
		{
			if (scriptParser.Finished) Terminate();
			else scriptParser.Update(gameTime);
		}

		public virtual bool ExecuteCommand(string[] tokens)
		{
			return false;
		}

		public virtual string ParseParameter(string parameter)
		{
			return null;
		}
	}
}
