using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Widgets;
using Sayohime.Scenes.ConversationScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    public class EventController : ScriptController
    {
        private CrawlerScene crawlerScene;

        public bool EndGame { get; private set; }

        MapRoom mapRoom = null;


        public EventController(CrawlerScene iScene, string[] script, MapRoom iMapRoom)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            crawlerScene = iScene;
            mapRoom = iMapRoom;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "GameEvent": GameEvent(tokens); break;
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens); /*Audio.PlaySound(GameSound.sfx_stairs_down);*/ break;
                case "DisableEvent": mapRoom.Script = null; break;
                case "Conversation": Conversation(tokens); break;
                case "Encounter": Encounter(tokens, scriptParser); break;
                case "GiveItem": GiveItem(tokens); break;
                case "RemoveChest": RemoveChest(tokens); break;
                case "RemoveNpc": RemoveNpc(tokens); break;
                case "ShowPortrait": ShowPortrait(tokens); break;
                case "MoveBackward": crawlerScene.MoveBackward(); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.Contains("Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else return base.ParseParameter(parameter);
        }

        private void GameEvent(string[] tokens)
        {
            switch (tokens[1])
            {

            }
        }

        private void ChangeMap(string[] tokens)
        {
            /*
            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 6) CrossPlatformGame.Transition(sceneType, tokens[2], int.Parse(tokens[3]), int.Parse(tokens[4]), (Direction)Enum.Parse(typeof(Direction), tokens[5]));
            else if (tokens.Length == 3) CrossPlatformGame.Transition(typeof(CrawlerScene), tokens[1], tokens[2]);
            else if (tokens.Length == 2) CrossPlatformGame.Transition(sceneType);
            else CrossPlatformGame.Transition(sceneType, tokens[2]);
            */
        }

        private void Conversation(string[] tokens)
        {
            if (tokens.Length == 2)
            {


                ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
                conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(conversationScene);

            }
            else
            {
                var convoRecord = new ConversationRecord()
                {
                    DialogueRecords = new DialogueRecord[] { new DialogueRecord() { Text = String.Join(' ', tokens.Skip(1)) } }
                };

                var convoScene = new ConversationScene.ConversationScene(convoRecord);
                convoScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(convoScene);
            }
        }

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = ItemRecord.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1)));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.CurrentSave.AddInventory(item.Name, 1);
            else GameProfile.CurrentSave.AddInventory(item.Name, -1);

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = new DialogueRecord[]
                {
                    new DialogueRecord() { Text = "Found " + item.Name + "!"}
                }
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            // Audio.PlaySound(GameSound.sfx_item_pickup);
        }

        public static void Encounter(string[] tokens, ScriptParser scriptParser)
        {
            /*
            BattleScene.BattleScene battleScene = new BattleScene.BattleScene(tokens[1], null);
            battleScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(battleScene, true);
            */
        }

        public void RemoveChest(string[] tokens)
        {
            var chest = crawlerScene.ChestList.FirstOrDefault(x => x.Name == tokens[1]);
            chest?.Destroy();
            crawlerScene.ChestList.Remove(chest);
        }

        public void RemoveNpc(string[] tokens)
        {
            var npc = crawlerScene.NpcList.FirstOrDefault(x => x.Name == tokens[1]);
            npc?.Destroy();
            crawlerScene.NpcList.Remove(npc);
        }

        public void ShowPortrait(string[] tokens)
        {

        }
    }
}
