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
				case "EndGame": EndGame = true; break;

                case "WaitForText": WaitForText(tokens); return false;
                case "ProceedText": conversationScene.ConversationViewModel.NextDialogue(); break;
                case "SelectionPrompt": SelectionPrompt(tokens); return false;
				case "ChangeConversation": ChangeConversation(tokens); break;
                case "EndConversation": conversationScene.ConversationViewModel.Close(); break;
                case "ChangeScene": ChangeScene(tokens); break;
                case "SetAutoProceed": conversationScene.ConversationViewModel.AutoProceedLength = int.Parse(tokens[1]); break;
                case "SetSkippable": conversationScene.ConversationViewModel.Skippable = bool.Parse(tokens[1]); break;

                case "ChangeMap": MapScene.EventController.ChangeMap(tokens, MapScene.MapScene.Instance); break;
                case "SpawnMonster": MapScene.EventController.SpawnMonster(tokens, MapScene.MapScene.Instance); break;
                case "TurnParty": MapScene.EventController.Turn(tokens, MapScene.MapScene.Instance); break;
                case "MoveParty": MapScene.EventController.Move(tokens, MapScene.MapScene.Instance); break;
                case "WaitParty": MapScene.MapScene.Instance.CaterpillarController.FinishMovement = scriptParser.BlockScript(); return false;
                case "Idle": MapScene.MapScene.Instance.CaterpillarController.Idle(); break;
                case "AnimateHero": MapScene.MapScene.Instance.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2], new AnimationFollowup(() => { })); break;
                case "RestoreMP": foreach (var hero in GameProfile.CurrentSave.Party) hero.Value.MP.Value = hero.Value.MaxMP.Value; break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; MapScene.MapScene.Instance.EventTriggers.Add(EventTrigger.LastTrigger); break;

                case "StoreParty":
                    var storedHero = GameProfile.CurrentSave.Party.ModelList.FirstOrDefault(x => x.Value.Name.Value == tokens[1]).Value;
                    GameProfile.CurrentSave.StoredHero = storedHero;
                    GameProfile.CurrentSave.Party.ModelList.RemoveAll(x => x.Value.Name.Value == tokens[1]);
                    break;

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
                case "Conversation":
					CrossPlatformGame.Transition(new Task<Scene>(() => new ConversationScene(tokens[2])));
					break;

                case "Title":
					CrossPlatformGame.Transition(new Task<Scene>(() => new TitleScene.TitleScene(tokens[2])));
					break;
            }
        }
	}
}
