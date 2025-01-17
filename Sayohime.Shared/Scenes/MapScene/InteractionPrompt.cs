using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.SceneObjects;


namespace Sayohime.Scenes.MapScene
{
    public class InteractionPrompt : Overlay
    {
        private const string PROMPT_FRAME = "DarkFrame";
        private const GameFont PROMPT_FONT = GameFont.Interface;

        private MapScene mapScene;
        private IInteractive target;

        private NinePatch textbox;

        private Color color = new Color(252, 224, 168);

        public InteractionPrompt(MapScene iMapScene)
        {
            mapScene = iMapScene;
            textbox = new NinePatch(PROMPT_FRAME, 0.05f);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CrossPlatformGame.CurrentScene != mapScene) return;

            if (target != null && mapScene.PriorityLevel == PriorityLevel.GameLevel)
            {
                string[] textLines = target.Label.Split('_');
                string longestLine = textLines.MaxBy(x => Text.GetStringLength(PROMPT_FONT, x));
                int width = Text.GetStringLength(PROMPT_FONT, longestLine);
                int height = Text.GetStringHeight(PROMPT_FONT);
                textbox.Bounds = new Rectangle(0, 0, width + 10, height * textLines.Count() + 5);
                Vector2 cameraOffset = new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);

                textbox.Draw(spriteBatch, target.LabelPosition - mapScene.Camera.Position - new Vector2(textbox.Bounds.Width / 2, 0) - cameraOffset);

                int row = 0;
                foreach (string text in textLines)
                {
                    Text.DrawCenteredText(spriteBatch, target.LabelPosition + new Vector2(1, 6) - mapScene.Camera.Position - cameraOffset, PROMPT_FONT, text, color, 0.03f, row);
                    row++;
                }
            }
        }

        public void Target(IInteractive newTarget)
        {
            if (newTarget == null) { target = null; return; }
            if (string.IsNullOrEmpty(newTarget.Label)) return;

            target = newTarget;
        }
    }
}
