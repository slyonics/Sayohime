using Microsoft.Xna.Framework;

namespace Sayohime.SceneObjects.Particles
{
	public class EmoteParticle : AnimationParticle
	{
		private Entity host;

		public EmoteParticle(Scene iScene, AnimationType iAnimationType, Entity iHost)
			: base(iScene, iHost.Position + new Vector2(0, -16), iAnimationType, true)
		{
			host = iHost;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			position = host.Position + new Vector2(0, -24 + SpriteBounds.Height / 2);
		}
	}
}
