using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Shaders;

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
                case "Encounter": Encounter(tokens, mapScene, scriptParser, this); return true;
                case "Shop": Shop(tokens); break;
                case "Pawn": Pawn(tokens); break;
                case "GiveItem": GiveItem(tokens); break;
                case "Inn": Inn(tokens); break;
                case "RestoreParty": RestoreParty(); break;
                case "MoveParty": Move(tokens, mapScene); break;
                case "TurnParty": Turn(tokens, mapScene); break;
                case "WaitParty": mapScene.CaterpillarController.FinishMovement = scriptParser.BlockScript(); return true;
                case "Idle": Idle(tokens); break;
                case "Animate": mapScene.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2]); break;
                case "SpawnMonster": SpawnMonster(tokens, mapScene); break;
                case "RecruitEnvi": RecruitEnvi(); break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; mapScene.EventTriggers.Add(EventTrigger.LastTrigger); break;
                case "ChangeClass": ChangeClass(tokens); break;
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

                    default: return null;
                }
            }
            else return null;
        }

        public static void ChangeMap(string[] tokens, MapScene mapScene)
        {
            var sceneTask = new Task<Scene>(() => new MapScene((GameMap)Enum.Parse(typeof(GameMap), tokens[1]), mapScene.Tilemap.Name));
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            transitionController.UpdateTransition += new Action<float>(t => mapScene.PaletteShader.SetGlobalBrightness(t));

            CrossPlatformGame.Transition(mapScene, sceneTask, transitionController, null);
        }

        public static void SpawnMonster(string[] tokens, MapScene mapScene)
        {
            mapScene.SpawnMonster(int.Parse(tokens[1]), int.Parse(tokens[2]), tokens[4], tokens[5], (Orientation)Enum.Parse(typeof(Orientation), tokens[3]));
        }

        public static void Conversation(string[] tokens, ScriptParser scriptParser)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);
        }

        public static void Encounter(string[] tokens, MapScene mapScene, ScriptParser scriptParser, Controller controller)
        {
            mapScene.BattleImminent = true;

            BattleScene.BattleScene battleScene = null;

            var unblocker = scriptParser.BlockScript();

            var terrain = mapScene.Tilemap.GetTile(mapScene.PartyLeader.Center).TerrainType;
            if (terrain == TerrainType.None) battleScene = new BattleScene.BattleScene(tokens[1], mapScene.BattleBackground);
            else battleScene = new BattleScene.BattleScene(tokens[1], mapScene.Tilemap.GetTile(mapScene.PartyLeader.Center).TerrainType);
            battleScene.OnTerminated += new TerminationFollowup(() =>
            {
                mapScene.BattleImminent = false;
                unblocker();

                if (!GameProfile.CurrentSave.Party.Any(x => x.Value.HP.Value > 0))
                {
                    controller.OnTerminated += new TerminationFollowup(() =>
                    {
                        if (!GameProfile.GetSaveData<bool>("IntroComplete"))
                        {
                            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("FoolOutro");
                            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                            CrossPlatformGame.StackScene(conversationScene);
                        }
                        else
                        {
                            Audio.StopMusic();
                            foreach (Hero hero in mapScene.Party) hero.PlayAnimation("Faint");
                            CrossPlatformGame.Transition(new Task<Scene>(() => new TitleScene.TitleScene()));
                        }
                    });
                }
            });

            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            Pinwheel pinwheel = new Pinwheel(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => pinwheel.Amount = t);
                        
            Audio.PlaySound(GameSound.BattleStart);
            mapScene.AddController(transitionController);
            CrossPlatformGame.TransitionShader = pinwheel;

            transitionController.OnTerminated += new TerminationFollowup(() =>
            {
                mapScene.RemoveNearbyEnemies();
                pinwheel.Terminate();
                CrossPlatformGame.StackScene(battleScene);
            });
        }

        public void Shop(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene(tokens[1]);
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void Pawn(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene();
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = new ItemRecord(ItemRecord.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1))));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.CurrentSave.AddInventory(item.Name, 1);
            else GameProfile.CurrentSave.AddInventory(item.Name, -1);

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = new DialogueRecord[]
                {
                    new DialogueRecord() { Text = "Found @" + item.Icon + " " + item.Name + "!"}
                }
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            Audio.PlaySound(GameSound.GetItem);
        }

        public void Inn(string[] tokens)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("Inn");
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());

            TransitionController transitionOutController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFadeOut = new SceneObjects.Shaders.ColorFade(Color.Black, transitionOutController.TransitionProgress);
            transitionOutController.UpdateTransition += new Action<float>(t => colorFadeOut.Amount = t);
            transitionOutController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                Audio.PauseMusic(true);
                Audio.PlaySound(GameSound.Cure);
                Task.Delay(1500).ContinueWith(t => Audio.PauseMusic(false));

                transitionOutController.Terminate();
                colorFadeOut.Terminate();
                TransitionController transitionInController = new TransitionController(TransitionDirection.In, 600);
                ColorFade colorFadeIn = new SceneObjects.Shaders.ColorFade(Color.Black, transitionInController.TransitionProgress);
                transitionInController.UpdateTransition += new Action<float>(t => colorFadeIn.Amount = t);
                transitionInController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    colorFadeIn.Terminate();
                });
                mapScene.AddController(transitionInController);
                mapScene.SceneShader = colorFadeIn;

                CrossPlatformGame.StackScene(conversationScene);
            });

            mapScene.AddController(transitionOutController);
            mapScene.SceneShader = colorFadeOut;

            RestoreParty();
        }

        public void RestoreParty()
        {
            foreach (var partyMember in GameProfile.CurrentSave.Party)
            {
                partyMember.Value.HP.Value = partyMember.Value.MaxHP.Value;
                partyMember.Value.MP.Value = partyMember.Value.MaxMP.Value;
                partyMember.Value.StatusAilments.Clear();
            }
        }

        public void RecruitEnvi()
        {
            GameProfile.CurrentSave.AddInventory("Dart", 5);
            HeroModel envi = new HeroModel(HeroType.Envi, ClassType.Warrior, 3);
            envi.Equip("Oak Club");
            envi.Equip("Bracers");
            envi.Equip("Hide Armor");
            GameProfile.CurrentSave.Party.ModelList.Insert(0, new ModelProperty<HeroModel>(envi));

            Npc enviNpc = mapScene.EntityList.First(x => x is Npc && ((Npc)x).Label == "Large Woman") as Npc;
            Vector2 heroPosition = enviNpc.Position;
            mapScene.NPCs.Remove(enviNpc);
            mapScene.EntityList.Remove(enviNpc);
            enviNpc.HostTile.Occupants.Remove(enviNpc);

            Hero hero = new Hero(mapScene, mapScene.Tilemap, heroPosition, envi);
            mapScene.AddEntity(hero);
            mapScene.Party.Insert(0, hero);
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

        public void ChangeClass(string[] tokens)
        {
            var dialogueRecords = new List<DialogueRecord>
			{
				new DialogueRecord()
				{
					Text = $"Do you want to change {tokens[1]} to a different character class?",
					Script = [ "DisableEnd", "WaitForText", "SelectionPrompt", "Fool", "Warrior", "Hunter", "Scholar", "End" ]
				}
			};

            var convoRecord = new ConversationRecord()
            {
                DialogueRecords = dialogueRecords.ToArray()
            };
            var convoScene = new ConversationScene.ConversationScene(convoRecord);
            CrossPlatformGame.StackScene(convoScene, true);
            convoScene.OnTerminated += new TerminationFollowup(() =>
            {
                HeroModel heroModel = GameProfile.CurrentSave.Party.First(x => x.Value.Name.Value == tokens[1]).Value;
                heroModel.ChangeClass((ClassType)Enum.Parse(typeof(ClassType), GameProfile.GetSaveData<string>("LastSelection")));
                var hero = mapScene.Party.First(x => x.HeroModel == heroModel);
                hero.UpdateSprite();
            });
        }
    }
}
