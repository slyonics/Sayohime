using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.SceneObjects;

namespace Sayohime.Scenes.MapScene
{
    public class AnnouncePopup : Overlay
    {
        private const string PROMPT_FRAME = "BattleFrame";
        private const GameFont PROMPT_FONT = GameFont.Interface;

        private MapScene mapScene;
        private NinePatch textbox;
        private string message;
        private Color color = new Color(127, 61, 63);

        int timeLeft = 2000;

        public AnnouncePopup(MapScene iMapScene, string iMessage)
        {
            mapScene = iMapScene;
            textbox = new NinePatch(PROMPT_FRAME, 0.05f);
            message = iMessage;

            mapScene.OverlayList.FirstOrDefault(x => x is AnnouncePopup)?.Terminate();
		}

        public override void Update(GameTime gameTime)
        {
            if (CrossPlatformGame.CurrentScene != mapScene || mapScene.PriorityLevel > PriorityLevel.CutsceneLevel) Terminate();
            else
            {
                timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (timeLeft <= 0) Terminate();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (terminated) return;

            string[] textLines = message.Split('_');
            string longestLine = textLines.MaxBy(x => Text.GetStringLength(PROMPT_FONT, x));
            int width = Text.GetStringLength(PROMPT_FONT, longestLine);
            int height = Text.GetStringHeight(PROMPT_FONT);
            textbox.Bounds = new Rectangle(0, 0, width + 9, height * textLines.Count() + 1);
            Vector2 centerPoint = new Vector2(CrossPlatformGame.SCREEN_WIDTH / 2, 16);

            textbox.Draw(spriteBatch, centerPoint - new Vector2(textbox.Bounds.Width / 2, textbox.Bounds.Height / 2));

            int row = 0;
            foreach (string text in textLines)
            {
                Text.DrawCenteredText(spriteBatch, centerPoint - new Vector2(0, 4 * (textLines.Length - 1) + 5), PROMPT_FONT, text, color, 0.03f, row);
                row++;
            }
        }
    }
}
