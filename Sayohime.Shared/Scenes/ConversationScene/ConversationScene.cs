using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using System.Runtime.InteropServices;
using Sayohime.SceneObjects.Shaders;

namespace Sayohime.Scenes.ConversationScene
{
    public class ConversationScene : Scene
    {
        private ConversationRecord conversationData;
		private Texture2D backgroundSprite;

		public ConversationViewModel ConversationViewModel { get; private set; }
		public ConversationController ConversationController { get; set; }
        
        int safetyShutdown = 100;


		public ConversationScene(ConversationRecord iConversationData)
			: base()
		{
            conversationData = iConversationData;

            if (conversationData.ShowOnStart)
            {
                ConversationViewModel = AddOverlay(new ConversationViewModel(this, conversationData));
            }

            if (!string.IsNullOrEmpty(conversationData.Background))
            {
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
            }
        }

		public ConversationScene(string conversationName)
            : this(ConversationRecord.CONVERSATIONS.FirstOrDefault(x => x.Name == conversationName))
        {
            
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            if (conversationData.StartScript != null)
            {
                ConversationController conversationController = AddController(new ConversationController(this, conversationData.StartScript));
                conversationController.OnTerminated += new TerminationFollowup(() =>
                {
                    if (!conversationData.ShowOnStart) ConversationViewModel = AddOverlay(new ConversationViewModel(this, conversationData));
					if (!string.IsNullOrEmpty(conversationData.Background)) base.BeginScene();
                });
            }
            else
            {
                if (!conversationData.ShowOnStart) ConversationViewModel = AddOverlay(new ConversationViewModel(this, conversationData));
				if (!string.IsNullOrEmpty(conversationData.Background)) base.BeginScene();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ControllerStack.Any(x => x.Count > 0)) safetyShutdown = 100;
            else
            {
                if (safetyShutdown <= 0) EndScene();
                safetyShutdown -= gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            if (backgroundSprite != null)
            {
                spriteBatch.Draw(backgroundSprite, new Rectangle(0, 0, CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }
        }

        public void FinishDialogue()
        {
            
        }

        public void RunScript(string[] script)
        {
            ConversationController = AddController(new ConversationController(this, script));
        }

        public bool IsScriptRunning()
        {
            return ConversationController != null && !ConversationController.Terminated && ConversationController.ScriptCommandsLeft;
        }
    }
}
