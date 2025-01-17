using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Widgets;

namespace Sayohime.Scenes.ConversationScene
{
    public class ConversationViewModel : ViewModel, ISkippableWait
    {
        private ConversationScene conversationScene;

        private ConversationRecord conversationRecord;
        private DialogueRecord currentDialogue;
        private int dialogueIndex;

        private CrawlText crawlText;

        public bool AutoProceed { get; set; }
        public int AutoProceedLength { get; set; } = -1;
        public bool Skippable { get; set; } = true;
        public bool TerminateOnEnd { get; set; } = true;

		public event Action OnDialogueScrolled;

		public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Actors_Blank : Enum.Parse<GameSprite>(currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Actors_Blank;
            Dialogue.Value = currentDialogue.Text;
            Window.Value = conversationRecord.Bounds;
			TerminateOnEnd = conversationRecord.TerminateOnEnd;
			Skippable = conversationRecord.Skippable;
            AutoProceedLength = conversationRecord.AutoProceedDuration;
            AutoProceed = AutoProceedLength > 0;

			if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);

			if (conversationRecord.SkipTransitions) LoadView(GameView.Conversation_FastConversationView);
			else LoadView(GameView.Conversation_ConversationView);

            crawlText = GetWidget<CrawlText>("ConversationText");

			if (AutoProceed && !Skippable)
			{
				parentScene.AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, false, AutoProceedLength));
			}
        }

		public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (conversationScene.PriorityLevel > PriorityLevel.GameLevel) return;

            if (crawlText.ReadyToProceed)
            {
                if (!ReadyToProceed.Value && !conversationScene.IsScriptRunning())
                {
                    if (!AutoProceed && Skippable && TerminateOnEnd) ShowProceed.Value = true;
                    ReadyToProceed.Value = true;
                }

                OnDialogueScrolled?.Invoke();
                OnDialogueScrolled = null;
            }

            if (!Closed && !ChildList.Any(x => x.Transitioning))
            {
                if (Input.CurrentInput.CommandPressed(Command.Confirm) && Skippable)
                {
                    Proceed();
                }
            }

            if (terminated)
            {
                parentScene.EndScene();
            }
        }

        public void Proceed()
        {
            if (!crawlText.ReadyToProceed)
            {
                crawlText.FinishText();
                conversationScene.FinishDialogue();
            }
            else NextDialogue();
        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            if (!Skippable) return;

            switch (clickWidget.Name)
            {
                case "ConversationText":
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        conversationScene.FinishDialogue();
                    }
                    else if (!AutoProceed) NextDialogue();
                    break;
            }
        }

        public void NextDialogue()
        {
            dialogueIndex++;
            if (dialogueIndex >= conversationRecord.DialogueRecords.Length)
            {
                EndConversation();
                return;
            }

            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Dialogue.Value = currentDialogue.Text;
            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Actors_Blank : (GameSprite)Enum.Parse(typeof(GameSprite), currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Actors_Blank;
            ShowProceed.Value = ReadyToProceed.Value = false;

            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);

            if (AutoProceed && !Skippable)
            {
                parentScene.AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, false, AutoProceedLength));
            }
        }

        public void EndConversation()
        {
            if (conversationRecord.EndScript != null)
            {
                conversationScene.AddController(new ConversationController(conversationScene, conversationRecord.EndScript)).OnTerminated += new TerminationFollowup(() =>
                {
					if (TerminateOnEnd) Close();
				});
			}
            else if (TerminateOnEnd) Close();
        }

        public void ChangeConversation(ConversationRecord newConversationRecord)
        {
            dialogueIndex = 0;

            conversationRecord = newConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Actors_Blank : (GameSprite)Enum.Parse(typeof(GameSprite), currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Actors_Blank;
            Dialogue.Value = currentDialogue.Text;

            ShowProceed.Value = ReadyToProceed.Value = false;

            conversationScene.ConversationController?.Terminate();
            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);
            else conversationScene.ConversationController = null;

        }

        public void Notify(SkippableWaitController sender)
        {
            Proceed();
        }

        public void TriggerScrollCallback()
        {
            OnDialogueScrolled?.Invoke();
        }

        public ModelProperty<Rectangle> Window { get; set; } = new ModelProperty<Rectangle>(ConversationRecord.DEFAULT_CONVO_BOUNDS);
        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<GameFont> ConversationFont { get; set; } = new ModelProperty<GameFont>(GameFont.Main);
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
        public ModelProperty<GameSprite> Portrait { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_Blank);

        public ModelProperty<bool> ShowPortrait { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<bool> ShowProceed { get; set; } = new ModelProperty<bool>(false);
	}
}
