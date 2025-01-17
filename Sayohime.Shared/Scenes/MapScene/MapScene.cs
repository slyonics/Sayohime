using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ldtk;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Shaders;
using System.Diagnostics.Metrics;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework.Input;

namespace Sayohime.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public const float SIGHT_RANGE = 20.0f;

        public static MapScene Instance;

        public TileMap Tilemap { get; set; }

        public string LocationName { get; private set; }

        public List<Hero> Party { get; private set; } = new List<Hero>();
        public Hero PartyLeader { get => Party.FirstOrDefault(); }
        public CaterpillarController CaterpillarController { get; private set; }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;

        public bool BattleImminent { get; set; }

        public string BattleBackground { get; private set; }
		public GameMusic mapMusic = GameMusic.None;

		public PaletteShader PaletteShader { get; } = new PaletteShader();

		public Chunk CurrentChunk { get; private set; }
		private int entrySteps = 3;

		public ConcurrentQueue<Npc> npcQueue { get; private set; } = new ConcurrentQueue<Npc>();
		public ConcurrentQueue<Enemy> enemyQueue { get; private set; } = new ConcurrentQueue<Enemy>();
		public ConcurrentQueue<EventTrigger> eventQueue { get; private set; } = new ConcurrentQueue<EventTrigger>();


		public MapScene(GameMap gameMap, string sourceMapName)
        {
			Instance = this;
			Tilemap = AddEntity(new TileMap(this, gameMap));
			GameProfile.CurrentSave.LastMap = gameMap.ToString();
			Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

			backgroundShader = entityShader = PaletteShader;

			CurrentChunk = Tilemap.FindHostChunk(sourceMapName);
			LoadChunk(CurrentChunk);
			AddQueuedEntities();

			var spawnZone = EventTriggers.First(x => x.Name == sourceMapName);
			GameProfile.CurrentSave.LastSpawn = spawnZone.Name;
            Orientation spawnOrientation = spawnZone.Direction;
			Vector2 spawnPosition = new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Center.Y);
			switch (spawnOrientation)
			{
				case Orientation.Up: spawnPosition.Y -= TileMap.TILE_SIZE; break;
				case Orientation.Right: spawnPosition.X += TileMap.TILE_SIZE; break;
				case Orientation.Down: spawnPosition.Y += TileMap.TILE_SIZE; break;
				case Orientation.Left: spawnPosition.X -= TileMap.TILE_SIZE; break;
			}

			var leaderHero = new Hero(this, Tilemap, spawnPosition, GameProfile.CurrentSave.Party.First().Value, spawnOrientation);
			leaderHero.CenterOn(spawnPosition);
			Party.Add(leaderHero);
            CaterpillarController = AddController(new CaterpillarController(this));
            foreach (var partymember in GameProfile.CurrentSave.Party.Skip(1))
            {
                Hero follower = new Hero(this, Tilemap, spawnPosition, partymember.Value, spawnOrientation);
				follower.CenterOn(spawnPosition);
				Party.Add(follower);
            }
            foreach (var partymember in Party.Reverse<Hero>())
            {
                AddEntity(partymember);
            }

			Camera.Center(leaderHero.Center);

			Tilemap.ClearFieldOfView();
			foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
			Tilemap.UpdateVisibility();

			if (sourceMapName == "Airship")
			{
				Party[0].Hide = true;

				var airship = new Airship(this, Tilemap, Party[0].Position, Orientation.Left);
				airship.Idle();
				airship.SetTargetAltitude(0);
				airship.LandAction = new Action(() =>
				{
					Audio.PlayMusic(GameMusic.NewDestinations);
					Party[0].Hide = false;
					CaterpillarController.Move(Orientation.Left, true);
					CaterpillarController.FinishMovement = new ScriptParser.UnblockFollowup(() =>
					{
						Party[0].Orientation = Orientation.Right;
						Party[0].Idle();
						var pilot = new Npc(this, Tilemap, 95, 17, "Pilot", Orientation.Left);
						AddEntity(pilot);
						ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("AirshipOutro");
						CrossPlatformGame.StackScene(conversationScene);
					});
				});
				AddEntity(airship);
			}
			else if (gameMap == GameMap.Tower && sourceMapName == "Default")
			{
                BeginSceneAnnounceless();
                Party[1].Position += new Vector2(16, 0);
                Party[1].UpdateBounds();
                Party[1].HostTile = Tilemap.GetTile(Party[1].Center);
                Party[0].Orientation = SceneObjects.Maps.Orientation.Right;
                Party[0].OrientedAnimation("Idle");
                Party[1].Orientation = SceneObjects.Maps.Orientation.Left;
                Party[1].OrientedAnimation("Idle");
                OverlayList.FirstOrDefault(x => x is AnnouncePopup)?.Terminate();
            }
		}

        public MapScene(GameMap gameMap, int spawnX, int spawnY, Orientation spawnOrientation)
        {
            Instance = this;
            Tilemap = AddEntity(new TileMap(this, gameMap));
            GameProfile.CurrentSave.LastMap = gameMap.ToString();
            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

            backgroundShader = entityShader = PaletteShader;

            CurrentChunk = Tilemap.FindHostChunk(spawnX, spawnY);
            LoadChunk(CurrentChunk);
            AddQueuedEntities();

            Vector2 spawnPosition = new Vector2(spawnX * TileMap.TILE_SIZE + (TileMap.TILE_SIZE / 2), spawnY * TileMap.TILE_SIZE + (TileMap.TILE_SIZE / 2));
            var leaderHero = new Hero(this, Tilemap, spawnPosition, GameProfile.CurrentSave.Party.First().Value, spawnOrientation);
            leaderHero.CenterOn(spawnPosition);
            Party.Add(leaderHero);
            CaterpillarController = AddController(new CaterpillarController(this));
            foreach (var partymember in GameProfile.CurrentSave.Party.Skip(1))
            {
                Hero follower = new Hero(this, Tilemap, spawnPosition, partymember.Value, spawnOrientation);
                follower.CenterOn(spawnPosition);
                Party.Add(follower);
            }
            foreach (var partymember in Party.Reverse<Hero>())
            {
                AddEntity(partymember);
            }

            Camera.Center(leaderHero.Center);

            Tilemap.ClearFieldOfView();
            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();
        }

        public void LoadNeighborChunks()
		{
			Rectangle nearbyArea = new Rectangle(Camera.View.X / TileMap.TILE_SIZE - 4, Camera.View.Y / TileMap.TILE_SIZE - 4, Camera.View.Width / TileMap.TILE_SIZE + 8, Camera.View.Height / TileMap.TILE_SIZE + 8);
			List<Chunk> chunksToLoad = Tilemap.FindUnloadedNeighborChunks(nearbyArea);
			foreach (Chunk chunk in chunksToLoad) Task.Run(() => LoadChunk(chunk));
		}

		public void LoadChunk(Chunk chunk)
		{
			chunk.LoadChunk();

			var entityLayers = chunk.Level.LayerInstances.Where(x => x.Type == "Entities");
			foreach (var entityLayer in entityLayers)
			{
				foreach (EntityInstance entity in entityLayer.EntityInstances)
				{
					switch (entity.Identifier)
					{
						case "Enemy":
							Enemy enemy = new Enemy(this, Tilemap, entity);
							if (enemy.IdleScript != null)
							{
								EnemyController enemyController = new EnemyController(this, enemy);
								AddController(enemyController);
							}
							Enemies.Add(enemy);
							AddEntity(enemy);
							break;

						case "NPC":
							{
								var property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
								if (property != null && property.Value != null && GameProfile.GetSaveData<bool>(property.Value)) continue;

								Npc npc = new Npc(this, Tilemap, entity);
								if (npc.Behavior != null)
								{
									NpcController npcController = new NpcController(this, npc);
									AddController(npcController);
								}
								NPCs.Add(npc);
								AddEntity(npc);
								break;
							}

						case "Chest":
							Chest chest = new Chest(this, Tilemap, entity);
							NPCs.Add(chest);
							AddEntity(chest);
							break;

						case "Interactable":
						case "Automatic":
						case "Travel":
							{
								var property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "EnableIf");
								if (property != null && property.Value != null && !GameProfile.GetSaveData<bool>(property.Value)) continue;

								property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
								if (property != null && property.Value != null && GameProfile.GetSaveData<bool>(property.Value)) continue;

								EventTriggers.Add(new EventTrigger(this, entity));
								break;
							}
					}
				}
			}
		}

		public void DetectNewChunk(Tile hostTile)
		{
			if (hostTile.ParentChunk == CurrentChunk) entrySteps = 3;
			else
			{
				entrySteps--;
				if (entrySteps <= 0)
				{
					entrySteps = 3;
					CurrentChunk = hostTile.ParentChunk;
					EnterChunk(CurrentChunk);
					AddOverlay(new AnnouncePopup(this, LocationName));

					GameProfile.Autosave();
				}
			}
		}

		public void EnterChunk(Chunk chunk)
		{
			foreach (FieldInstance field in chunk.Level.FieldInstances)
			{
				switch (field.Identifier)
				{
					case "Music": if (!string.IsNullOrEmpty(field.Value) && (Tilemap.Name != GameMap.Overworld.ToString() || Audio.CurrentMusic != GameMusic.BeyondtheHills))
						{
							mapMusic = (GameMusic)Enum.Parse(typeof(GameMusic), field.Value);
							Audio.PlayMusic(mapMusic);
						}
						break;
					case "Script": if (!string.IsNullOrEmpty(field.Value)) AddController(new EventController(this, field.Value.Split('\n'))); break;

					case "LocationName": if (!string.IsNullOrEmpty(field.Value)) LocationName = field.Value; else LocationName = Tilemap.Name; break;
					case "Background": if (!string.IsNullOrEmpty(field.Value)) BuildParallaxBackground(field.Value); break;
					case "BattleBackground": if (!string.IsNullOrEmpty(field.Value)) BattleBackground = field.Value; break;
				}
			}

			GameProfile.CurrentSave.LocationName.Value = LocationName.Replace('_', ' ');
		}

		public void SaveMapPosition()
        {
            GameProfile.SetSaveData<int>("LastPositionX", (int)PartyLeader.HostTile.TileX);
            GameProfile.SetSaveData<int>("LastPositionY", (int)PartyLeader.HostTile.TileY);
        }

		public override void BeginScene()
		{
			sceneStarted = true;

			TransitionController transitionController = new TransitionController(TransitionDirection.In, 600);
			transitionController.UpdateTransition += new Action<float>(t => PaletteShader.SetGlobalBrightness(t));
			transitionController.OnTerminated += new TerminationFollowup(() => AddOverlay(new AnnouncePopup(this, LocationName)));
			AddController(transitionController);

			EnterChunk(CurrentChunk);
		}

		public void BeginSceneAnnounceless()
		{
			sceneStarted = true;

			/*
			TransitionController transitionController = new TransitionController(TransitionDirection.In, 600);
			transitionController.UpdateTransition += new Action<float>(t => PaletteShader.SetGlobalBrightness(t));
			AddController(transitionController);
			*/

			PaletteShader.SetGlobalBrightness(1.0f);

			EnterChunk(CurrentChunk);
		}

		public override void ResumeScene()
		{
			base.ResumeScene();

			Audio.PlayMusic(mapMusic);

			BattleImminent = false;
		}

		public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            NPCs.RemoveAll(x => x.Terminated);
            Enemies.RemoveAll(x => x.Terminated);

            parallaxBackdrop?.Update(gameTime, Camera);
        }

        public bool ProcessAutoEvents()
        {
            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds) && !eventTrigger.Interactive)
                {
                    eventTriggered = true;
                    eventTrigger.Terminated = true;
                    EventTrigger.LastTrigger = eventTrigger;
                    AddController(new EventController(this, eventTrigger.Script));
                }
            }
            EventTriggers.RemoveAll(x => x.Terminated);

            return eventTriggered;
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            parallaxBackdrop?.Draw(spriteBatch);

            Tilemap.DrawBackground(spriteBatch, Camera);
        }

        private void BuildParallaxBackground(string background)
        {
            string[] tokens = background.Split(' ');

            parallaxBackdrop = new ParallaxBackdrop(tokens[0], tokens.Skip(1).Select(x => float.Parse(x)).ToArray());
        }

		private void AddQueuedEntities()
		{
			Npc npc;
			while (npcQueue.TryDequeue(out npc))
			{
				if (npc.Behavior != null) AddController(new NpcController(this, npc));
				NPCs.Add(npc);
				AddEntity(npc);
			}

			Enemy trainer;
			while (enemyQueue.TryDequeue(out trainer))
			{
				if (trainer.IdleScript != null) AddController(new EnemyController(this, trainer));
				Enemies.Add(trainer);
				AddEntity(trainer);
			}

			EventTrigger eventTrigger;
			while (eventQueue.TryDequeue(out eventTrigger))
			{
				EventTriggers.Add(eventTrigger);
			}
		}

		public void RemoveNearbyEnemies()
        {
            foreach (Hero hero in Party)
            {
                Tile hostTile = Tilemap.GetTile(hero.Center);

                foreach (Actor occupant in hostTile.Occupants) if (occupant is Enemy) occupant.Terminate();
                hostTile.Occupants.RemoveAll(x => x.Terminated);

                foreach (Tile neighbor in hostTile.NeighborList)
                {
                    foreach (Actor occupant in neighbor.Occupants) if (occupant is Enemy) occupant.Terminate();
                    neighbor.Occupants.RemoveAll(x => x.Terminated);
                }
            }
        }

        public void SpawnMonster(int x, int y, string sprite, string encounter, Orientation orientation = Orientation.Down)
        {
            Enemy enemy = new Enemy(this, Tilemap, x, y, sprite, encounter, orientation);
            if (enemy.IdleScript != null)
            {
                EnemyController enemyController = new EnemyController(this, enemy);
                AddController(enemyController);
            }
            Enemies.Add(enemy);
            AddEntity(enemy);
        }
    }
}
