using Microsoft.Xna.Framework.Graphics;

namespace Sayohime.SceneObjects.Overlays
{
    public class ParticleOverlay : Overlay
    {
        private Particle particle;

        public ParticleOverlay(Particle iParticle)
        {
            particle = iParticle;
            particle.OnTerminated += new TerminationFollowup(Terminate);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            particle.Draw(spriteBatch, null);
        }
    }
}
