using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;

namespace Sayohime.Scenes.ConversationScene
{
    public class SelectionViewModel : ViewModel
    {
        private GameFont OPTION_FONT = GameFont.Interface;

        private ConversationScene conversationScene;

        private int selection = -1;
        private bool cancelable;

        public SelectionViewModel(Scene iScene, List<string> options, bool iCancelable = false)
            : base(iScene, PriorityLevel.MenuLevel)
        {
            conversationScene = iScene as ConversationScene;
            cancelable = iCancelable;

            int longestOption = 0;
            foreach (string option in options)
            {
                AvailableOptions.Add(option);
                int optionLength = Text.GetStringLength(OPTION_FONT, option);
                if (optionLength > longestOption) longestOption = optionLength;
            }
            int width = longestOption + 10;
            ButtonSize.Value = new Rectangle(0, 0, longestOption + 6, Text.GetStringHeight(OPTION_FONT) + 2);
            LabelSize.Value = new Rectangle(0, -5, longestOption + 6, ButtonSize.Value.Height);

            int height = ButtonSize.Value.Height * options.Count() + 5;
            WindowSize.Value = new Rectangle(CrossPlatformGame.SCREEN_WIDTH / 2 - width - width - 120, CrossPlatformGame.SCREEN_HEIGHT / 2 - 48 - height, width, height);

            LoadView(GameView.Conversation_SelectionView);
        }

        public SelectionViewModel(Scene iScene, List<string> options, Vector2 offset, bool iCancelable = false)
            : base(iScene, PriorityLevel.MenuLevel)
        {
            conversationScene = iScene as ConversationScene;
            cancelable = iCancelable;

            int longestOption = 0;
            foreach (string option in options)
            {
                AvailableOptions.Add(option);
                int optionLength = Text.GetStringLength(OPTION_FONT, option);
                if (optionLength > longestOption) longestOption = optionLength;
            }
            int width = longestOption + 10;
            ButtonSize.Value = new Rectangle(0, 0, longestOption + 6, Text.GetStringHeight(OPTION_FONT) + 2);
            LabelSize.Value = new Rectangle(0, -5, longestOption + 6, ButtonSize.Value.Height);

            int height = ButtonSize.Value.Height * options.Count() + 5;
            WindowSize.Value = new Rectangle(CrossPlatformGame.SCREEN_WIDTH / 2 - width - width - 120 + (int)offset.X, CrossPlatformGame.SCREEN_HEIGHT / 2 - 48 - height + (int)offset.Y, width, height);

            LoadView(GameView.Conversation_SelectionView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var input = Input.CurrentInput;
            if (input.CommandPressed(Command.Confirm) && selection != -1)
            {
                Audio.PlaySound(GameSound.Cursor);
                Terminate();
            }
            else if (input.CommandPressed(Command.Cancel) && cancelable)
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
            }
        }

        public override void Terminate()
        {
            //conversationScene.ConversationViewModel.Proceed();
            base.Terminate();
        }

        public void SelectOption(object parameter)
        {
            GameProfile.SetSaveData<string>("LastSelection", parameter.ToString());
            Terminate();
        }

        public ModelCollection<string> AvailableOptions { get; set; } = new ModelCollection<string>();

        public ModelProperty<Rectangle> WindowSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
        public ModelProperty<Rectangle> ButtonSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
        public ModelProperty<Rectangle> LabelSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
    }
}
