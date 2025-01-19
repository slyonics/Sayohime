using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Shaders;

namespace Sayohime.Scenes.ConversationScene
{
    public class Portrait : Entity
    {
        public string Name { get; private set; }

        public Effect Shader { set; private get; }

        public List<TransitionController> PortraitControllers { get; set; } = new List<TransitionController>();

        public Portrait(ConversationScene iScene, string iName, string iSprite, Vector2 iPosition, byte brightness = 4)
            : base(iScene, iPosition)
        {
            Name = iName;

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Portraits_Front_" + iSprite)], null);
			AnimatedSprite.SpriteColor = new Color((byte)0, (byte)0, brightness, (byte)255);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            PortraitControllers.RemoveAll(x => x.Terminated);
        }

        public void FinishTransition()
        {
            foreach (TransitionController portraitController in PortraitControllers)
            {
                portraitController.Finish();
            }
        }
    }
}
