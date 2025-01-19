using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Shaders;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using System.Diagnostics.Metrics;
using Sayohime.SceneObjects.Particles;

namespace Sayohime.Scenes.MapScene
{
    public class EventController : ScriptController
    {
        private MapScene mapScene;

        public bool EndGame { get; private set; }
        public Actor ActorSubject { get; set; }

        public EventController(MapScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens, mapScene); break;
                case "Conversation": Conversation(tokens, scriptParser); break;
                case "GiveItem": GiveItem(tokens); break;
                case "MoveParty": Move(tokens, mapScene); break;
                case "TurnParty": Turn(tokens, mapScene); break;
                case "WaitParty": mapScene.CaterpillarController.FinishMovement = scriptParser.BlockScript(); return true;
                case "Idle": Idle(tokens); break;
                case "Animate": mapScene.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2]); break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; mapScene.EventTriggers.Add(EventTrigger.LastTrigger); break;
                case "DeleteTrigger": EventTrigger.LastTrigger.Terminated = true; mapScene.EventTriggers.Remove(EventTrigger.LastTrigger); break;
                case "Encounter": Encounter(tokens); break;
                case "Emote": Emote(tokens); break;

				case "Shop": Shop(tokens); break;

				case "MoveTrainer": MoveTrainer(); return true;
                case "FaceTrainer": FaceTrainer(); break;

                case "AnimateNpc":
                    {
						var npc = mapScene.NPCs.First(x => x.Name == tokens[1]);
                        npc.OrientedAnimation(tokens[2]);
                        npc.PriorityLevel = PriorityLevel.CutsceneLevel;
                    }
                    break;
				case "MoveNpc": mapScene.AddController(new PathingController(mapScene, mapScene.NPCs.First(x => x.Name == tokens[1]), mapScene.Tilemap.GetTile(int.Parse(tokens[2]) + (int)(mapScene.CurrentChunk.OffsetPx.X / 16), int.Parse(tokens[3]) + (int)(mapScene.CurrentChunk.OffsetPx.Y / 16)))); break;
                case "TurnNpc":
                    {
                        var npc = mapScene.NPCs.First(x => x.Name == tokens[1]);
                        npc.Orientation = Enum.Parse<Orientation>(tokens[2]);
                        npc.Idle();
                    }
                    break;
                case "RemoveNpc":
                    {
                        var npc = mapScene.NPCs.First(x => x.Name == tokens[1]);
                        mapScene.NPCs.Remove(npc);
                        mapScene.EntityList.Remove(npc);
                        npc.HostTile.Occupants.Remove(npc);
                    }
                    break;
                case "WaitForActors": WaitForActors(); return true;

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.StartsWith("$saveData."))
            {
                return GameProfile.GetSaveData<string>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] == '$')
            {
                switch (parameter)
                {
                    //case "$pokeCount": return GameProfile.CurrentSave.Roster.Count.ToString();
                    default: return null;
                }
            }
            else return null;
        }

        public static void ChangeMap(string[] tokens, MapScene mapScene)
        {
			Task<Scene> sceneTask = new Task<Scene>(() => new MapScene(Enum.Parse<GameMap>(tokens[1]), tokens[2]));
			TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
			Pinwheel pinwheel = new Pinwheel(Graphics.PURE_BLACK, transitionController.TransitionProgress);
			transitionController.UpdateTransition += new Action<float>(t => pinwheel.Amount = t);

			CrossPlatformGame.Transition(mapScene, sceneTask, transitionController, pinwheel);
		}

        public static void Conversation(string[] tokens, ScriptParser scriptParser)
        {
			var convoScene = (tokens.Length == 2) ? new ConversationScene.ConversationScene(tokens[1]) : new ConversationScene.ConversationScene(String.Join(' ', tokens.Skip(1)));
			convoScene.OnRemoval += new Action(scriptParser.BlockScript());
			CrossPlatformGame.StackScene(convoScene);
		}

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = ItemRecord.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1)));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.CurrentSave.AddInventory(item.Name, 1);
            else GameProfile.CurrentSave.AddInventory(item.Name, -1);

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = [ new DialogueRecord() { Text = "Found @" + item.Icon + " " + item.Name + "!"} ]
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            Audio.PlaySound(GameSound.GetItem);
        }

        public static void Move(string[] tokens, MapScene mapScene)
        {
            mapScene.CaterpillarController.Move((Orientation)Enum.Parse(typeof(Orientation), tokens[1]), true);
            foreach (Hero hero in mapScene.Party) hero.PriorityLevel = PriorityLevel.CutsceneLevel;
        }

        public static void Turn(string[] tokens, MapScene mapScene)
        {
            Hero hero = mapScene.Party[int.Parse(tokens[1])];
            hero.Orientation = (Orientation)Enum.Parse(typeof(Orientation), tokens[2]);
            hero.OrientedAnimation("Idle");
        }

        public void Idle(string[] tokens)
        {
            mapScene.CaterpillarController.Idle();
        }

        public void Encounter(string[] tokens)
        {
            /*
			mapScene.BattleImminent = true;

			Task<Scene> sceneTask = new Task<Scene>(() => new BattleScene.BattleScene(mapScene, tokens[1]));
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            Pinwheel pinwheel = new Pinwheel(Graphics.PURE_BLACK, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => pinwheel.Amount = t);

            CrossPlatformGame.Transition(mapScene, sceneTask, transitionController, pinwheel);
            */
        }

        public void Emote(string[] tokens)
        {
            Entity entity = null;
            switch (tokens[2])
            {
                case "Player": entity = mapScene.PartyLeader; break;
            }

            mapScene.AddParticle(new EmoteParticle(mapScene, Enum.Parse<AnimationType>(tokens[1]), entity));
        }

		public void Shop(string[] tokens)
		{
            /*
			ShopScene.ShopScene shopScene = new ShopScene.ShopScene(tokens[1]);
			shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
			CrossPlatformGame.StackScene(shopScene);
            */
		}

		public void MoveTrainer()
        {
            ChaseController chaseController = new ChaseController(mapScene, (Trainer)ActorSubject);
            mapScene.AddController(chaseController);

            var unblock = scriptParser.BlockScript();
            chaseController.OnTerminated += new TerminationFollowup(() => unblock());

        }

        public void FaceTrainer()
        {
			mapScene.PartyLeader.Reorient(ActorSubject.Center - mapScene.PartyLeader.Center);
			mapScene.PartyLeader.OrientedAnimation("Idle");
		}

        public void WaitForActors()
        {
            foreach (var controllerList in mapScene.ControllerStack)
            {
                foreach (var controller in controllerList)
                {
                    var pathController = controller as PathingController;
                    if (pathController != null)
                    {
                        var unblock = scriptParser.BlockScript();
                        pathController.OnTerminated += new TerminationFollowup(() => unblock());
                    }
                }
            }
        }
    }
}
