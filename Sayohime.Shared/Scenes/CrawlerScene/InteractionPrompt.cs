using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.SceneObjects;

namespace Sayohime.Scenes.CrawlerScene
{
    public class InteractionPrompt : Overlay
    {
        private const string PROMPT_FRAME = "BattleWindow";
        private const GameFont PROMPT_FONT = GameFont.Interface;

        private CrawlerScene crawlerScene;
        private NinePatch textbox;
        private string[] textLines;
        private Color color = new Color(255, 255, 255, 255);

        public InteractionPrompt(CrawlerScene iCrawlerScene, Vector2 center, string label)
        {
            crawlerScene = iCrawlerScene;
            textbox = new NinePatch(PROMPT_FRAME, 0.05f);

			textLines = label.Split('_');
			string longestLine = textLines.MaxBy(x => Text.GetStringLength(PROMPT_FONT, x));
			int width = Text.GetStringLength(PROMPT_FONT, longestLine) + 13;
			int height = Text.GetStringHeight(PROMPT_FONT) * (textLines.Length - 1) + 18;
			textbox.Bounds = new Rectangle((int)center.X - (width / 2), (int)center.Y - (height / 2), width, height);
		}

        public override void Update(GameTime gameTime)
        {
			// if (CrossPlatformGame.CurrentScene != crawlerScene || crawlerScene.PriorityLevel != PriorityLevel.GameLevel) Terminate();
		}

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CrossPlatformGame.CurrentScene != crawlerScene || crawlerScene.PriorityLevel != PriorityLevel.GameLevel) return;

            textbox.Draw(spriteBatch, Vector2.Zero);

            int row = 0;
            foreach (string text in textLines)
            {
                Text.DrawCenteredText(spriteBatch, new Vector2(0, textbox.Bounds.Height / -2), PROMPT_FONT, text, color, 0.03f, row);
                row++;
            }
        }
    }
}
