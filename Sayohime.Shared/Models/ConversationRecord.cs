using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Sayohime.Models
{
    public class ConversationRecord
    {
		public static List<ConversationRecord> CONVERSATIONS { get; set; }
		public static readonly Rectangle DEFAULT_CONVO_BOUNDS = new Rectangle(-145, 30, 290, 60);

		public string Name { get; set; }
        public string Background { get; set; }
        public DialogueRecord[] DialogueRecords { get; set; }
        public string[] StartScript { get; set; }
		public string[] EndScript { get; set; }
		public Rectangle Bounds { get; set; } = DEFAULT_CONVO_BOUNDS;

        public bool SkipTransitions { get; set; } = false;
		public bool TerminateOnEnd { get; set; } = true;
        public bool ShowOnStart { get; set; } = true;
		public bool Skippable { get; set; } = true;
		public int AutoProceedDuration { get; set; } = -1;

		public ConversationRecord()
        {

        }

		public ConversationRecord(DialogueRecord dialogueRecord)
		{
			DialogueRecords = [ dialogueRecord ];
		}

		public ConversationRecord(string conversationText)
		{
            DialogueRecords = [ new DialogueRecord() { Text = conversationText } ];
		}

		public ConversationRecord(ConversationRecord clone)
        {
            Name = clone.Name;
            Background = clone.Background;
            DialogueRecords = (DialogueRecord[])clone.DialogueRecords.Clone();
            Bounds = clone.Bounds;
			TerminateOnEnd = clone.TerminateOnEnd;
			Skippable = clone.Skippable;
            AutoProceedDuration = clone.AutoProceedDuration;
        }
	}

    public class DialogueRecord
    {
        public string Speaker { get; set; }
        public string Portrait { get; set; }
        public string Text { get; set; }
        public string[] Script { get; set; }
	}
}
