using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sayohime.SceneObjects
{
	public class Shader
	{
		protected Effect shaderEffect;

		protected bool terminated = false;

		public Shader(Effect initialEffect)
		{
			shaderEffect = initialEffect;
		}

		public virtual void Update(GameTime gameTime, Camera camera)
		{

		}

		public virtual void Terminate()
		{
			terminated = true;
		}

		public Effect Effect { get => shaderEffect; }
		public bool Terminated { get => terminated; }
	}
}
