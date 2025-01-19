using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.Scenes.MapScene;
using Sayohime.SceneObjects.Shaders;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Sayohime.Scenes.ConversationScene
{
    public class ConversationController : ScriptController
    {
        private ConversationScene conversationScene;

        public bool EndGame { get; private set; }

        public ConversationController(ConversationScene iScene, string script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            conversationScene = iScene;
        }

        public ConversationController(ConversationScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            conversationScene = iScene;
        }

        public bool ScriptCommandsLeft
        {
            get
            {
                return scriptParser.ScriptCommands != null && scriptParser.ScriptCommands.Count > 0;
            }
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "AddPortrait": AddPortrait(tokens); break;
                case "RemovePortrait": RemovePortrait(tokens); break;
                case "PortraitSprite": PortraitSprite(tokens); break;
                case "PortraitPosition": PortraitPosition(tokens); break;
                case "PortraitColor": PortraitColor(tokens); break;
				case "PortraitBrightness": PortraitBrightness(tokens); break;
				case "PortraitScale": PortraitScale(tokens); break;
                case "PortraitVelocity": PortraitVelocity(tokens); break;
				case "WaitForPortrait": WaitForPortrait(tokens); return false;

				case "EndGame": EndGame = true; break;

                case "WaitForText": WaitForText(tokens); return false;
                case "ProceedText": conversationScene.ConversationViewModel.NextDialogue(); break;
                case "SelectionPrompt": SelectionPrompt(tokens); return false;
				case "ChangeConversation": ChangeConversation(tokens); break;
                case "EndConversation": conversationScene.ConversationViewModel.Close(); break;
                case "ChangeScene": ChangeScene(tokens); break;
                case "SetAutoProceed": conversationScene.ConversationViewModel.AutoProceedLength = int.Parse(tokens[1]); break;
                case "SetSkippable": conversationScene.ConversationViewModel.Skippable = bool.Parse(tokens[1]); break;

				case "GiveItem": GiveItem(tokens); break;

				default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$leftPortraitX": return (((int)(CrossPlatformGame.SCREEN_WIDTH / 1.6) - 60) / 2).ToString();
                case "$rightPortraitX": return (CrossPlatformGame.SCREEN_WIDTH - ((int)(CrossPlatformGame.SCREEN_WIDTH / 1.6) - 60) / 2).ToString();
                case "$portraitY": return ((int)(CrossPlatformGame.SCREEN_HEIGHT)).ToString();
                case "$portraitScaleX": return (CrossPlatformGame.SCREEN_WIDTH / 1920.0f / 1.6f).ToString();
                case "$portraitScaleY": return (CrossPlatformGame.SCREEN_HEIGHT / 1080.0f / 1.6f).ToString();
                default: return base.ParseParameter(parameter);
            }
        }

        private void AddPortrait(string[] tokens)
        {
            string name = tokens[1];
            string sprite = tokens[2];
            int positionX = int.Parse(tokens[3]);
            int positionY = int.Parse(tokens[4]);
            byte brightness = byte.Parse(tokens[5]);

			Portrait portrait = new Portrait(conversationScene, name, sprite, new Vector2(positionX, positionY), brightness);
            conversationScene.AddPortrait(portrait);
        }

        private void RemovePortrait(string[] tokens)
        {
            /*
            if (tokens.Length > 3)
            {
                string name = tokens[1];
                int endX = int.Parse(tokens[2]);
                int endY = int.Parse(tokens[3]);
                float transitionLength = (tokens.Length > 4) ? float.Parse(tokens[4]) : 1.0f;

                Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
                portrait.Remove(new Vector2(endX, endY), transitionLength);
            }
            else
            {
                string name = tokens[1];
                float transitionLength = (tokens.Length > 2) ? float.Parse(tokens[2]) : 1.0f;

                Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
                portrait?.Remove(transitionLength);
            }
            */
        }

        private void PortraitSprite(string[] tokens)
        {
            string name = tokens[1];
            string sprite = tokens[2];

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.AnimatedSprite.SpriteTexture = AssetCache.SPRITES[Enum.Parse<GameSprite>($"Portraits_Front_{sprite}")];
        }

        private void PortraitPosition(string[] tokens)
        {
            string name = tokens[1];
            Vector2 end = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            float transitionLength = (tokens.Length > 4) ? float.Parse(tokens[4]) : 1.0f;

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
			Vector2 start = portrait.Position;

			TransitionController transitionController = conversationScene.AddController(new TransitionController(TransitionDirection.In, (int)(transitionLength * 1000), PriorityLevel.CutsceneLevel, false));
            transitionController.UpdateTransition += new Action<float>(t =>
            {
                portrait.Position = Vector2.Lerp(start, end, t);
            });

			portrait.PortraitControllers.Add(transitionController);
		}

        private void PortraitColor(string[] tokens)
        {
            string name = tokens[1];
            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);

            /*
            if (tokens[3] == "1") portrait.AnimatedSprite.SetColor1(Enum.Parse<PaletteHue>(tokens[2]));
            else portrait.AnimatedSprite.SetColor2(Enum.Parse<PaletteHue>(tokens[2]));
            */
		}

        private void PortraitBrightness(string[] tokens)
        {
			string name = tokens[1];
			byte end = byte.Parse(tokens[2]);
			float transitionLength = (tokens.Length > 3) ? float.Parse(tokens[3]) : 1.0f;

			Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
			byte start = portrait.AnimatedSprite.Brightness;

			TransitionController transitionController = conversationScene.AddController(new TransitionController(TransitionDirection.In, (int)(transitionLength * 1000), PriorityLevel.TransitionLevel, false));
			transitionController.UpdateTransition += new Action<float>(t =>
			{
				portrait.AnimatedSprite.SetBrightness((byte)MathHelper.Lerp(start, end, t));
			});

            portrait.PortraitControllers.Add(transitionController);
		}


		public void PortraitScale(string[] tokens)
        {
            string name = tokens[1];
            float scaleX = float.Parse(tokens[2]);
            float scaleY = float.Parse(tokens[3]);

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.AnimatedSprite.Scale = new Vector2(scaleX, scaleY);
        }

        public void PortraitVelocity(string[] tokens)
        {
            string name = tokens[1];
            float scaleX = float.Parse(tokens[2]);
            float scaleY = float.Parse(tokens[3]);

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.Velocity = new Vector2(scaleX, scaleY);
        }

        public void WaitForPortrait(string[] tokens)
        {
			Portrait portrait = conversationScene.Portraits.Find(x => x.Name == tokens[1]);
            foreach (var controller in portrait.PortraitControllers)
            {
                var unblock = scriptParser.BlockScript();
				controller.OnTerminated += new TerminationFollowup(() => unblock());
            }
		}


		private void WaitForText(string[] tokens)
        {
            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            conversationScene.ConversationViewModel.OnDialogueScrolled += new Action(followup);
        }

        private void SelectionPrompt(string[] tokens)
        {
            List<string> options = new List<string>();
            string skipLine;
            do
            {
                skipLine = scriptParser.DequeueNextCommand();
                options.Add(skipLine);
            } while (skipLine != "End");
            options.RemoveAt(options.Count - 1);

            SelectionViewModel selectionViewModel = new SelectionViewModel(conversationScene, options);
            conversationScene.AddOverlay(selectionViewModel);

            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            selectionViewModel.OnTerminated += new Action(followup);
        }

		private void ChangeConversation(string[] tokens)
        {
            var conversationData = ConversationRecord.CONVERSATIONS.FirstOrDefault(x => x.Name == tokens[1]);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) scriptParser.RunScript(conversationData.DialogueRecords[0].Script);

            conversationScene.ConversationViewModel.ChangeConversation(conversationData);
        }

        private void ChangeScene(string[] tokens)
        {
            switch (tokens[1])
            {
                case "MapScene":

                    TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
                    transitionController.UpdateTransition += new Action<float>(t => conversationScene.PaletteShader.SetGlobalBrightness(t));

                    CrossPlatformGame.Transition(conversationScene, new Task<Scene>(() => new MapScene.MapScene(Enum.Parse<GameMap>(tokens[2]), tokens[3])), transitionController, conversationScene.PaletteShader);
                    break;
            }
        }

        public void GiveItem(string[] tokens)
        {
            var name = string.Join(' ', tokens.Skip(1));
            GameProfile.CurrentSave.AddInventory(name, 1);
        }
	}
}
