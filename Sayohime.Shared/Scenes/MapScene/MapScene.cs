using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ldtk;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Shaders;
using Sayohime.SceneObjects.Controllers;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Sayohime.Scenes.MapScene
{
	public class MapScene : Scene
    {
        public const float SIGHT_RANGE = 20.0f;

        public static MapScene Instance;

        public TileMap Tilemap { get; set; }

        public string LocationName { get; private set; } = "Route Unknown";

        public List<Hero> Party { get; private set; } = new List<Hero>();
        public Hero PartyLeader { get => Party.FirstOrDefault(); }
        public CaterpillarController CaterpillarController { get; private set; }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Trainer> Trainers { get; private set; } = new List<Trainer>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;

        public bool BattleImminent { get; set; }

        public string BattleBackground { get; private set; }

        public GameMusic mapMusic = GameMusic.None;

        public PaletteShader PaletteShader { get; } = new PaletteShader();


		public Chunk CurrentChunk { get; private set; }
		private int entrySteps = 3;

		public ConcurrentQueue<Npc> npcQueue { get; private set; } = new ConcurrentQueue<Npc>();
		public ConcurrentQueue<Trainer> trainerQueue { get; private set; } = new ConcurrentQueue<Trainer>();
		public ConcurrentQueue<EventTrigger> eventQueue { get; private set; } = new ConcurrentQueue<EventTrigger>();


		public MapScene(GameMap gameMap, string sourceMapName)
			: base()
        {
			Instance = this;
			Tilemap = AddEntity(new TileMap(this, gameMap));
			Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

			backgroundShader = entityShader = PaletteShader;

			CurrentChunk = Tilemap.FindHostChunk(sourceMapName);
            LoadChunk(CurrentChunk);
			AddQueuedEntities();

			var spawnZone = EventTriggers.First(x => x.Name == sourceMapName);
            Orientation spawnOrientation = spawnZone.Direction;
            Vector2 spawnPosition = new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Center.Y);
            switch (spawnOrientation)
            {
                case Orientation.Up: spawnPosition.Y -= TileMap.TILE_SIZE; break;
                case Orientation.Right: spawnPosition.X += TileMap.TILE_SIZE; break;
                case Orientation.Down: spawnPosition.Y += TileMap.TILE_SIZE; break;
                case Orientation.Left: spawnPosition.X -= TileMap.TILE_SIZE; break;
            }

			var leaderHero = new Hero(this, Tilemap, spawnPosition, GameSprite.Actors_AdvM, PaletteHue.LightBlue, iOrientation: spawnOrientation);
			leaderHero.CenterOn(spawnPosition);
			Tilemap.GetTile(leaderHero.Center).Occupants.Add(leaderHero);
			leaderHero.HostTile = Tilemap.GetTile(leaderHero.Center);
			Camera.Center(leaderHero.Center);

			Party.Add(leaderHero);
			AddEntity(leaderHero);
			CaterpillarController = AddController(new CaterpillarController(this));

			Tilemap.ClearFieldOfView();
			foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
			Tilemap.UpdateVisibility();

			GameProfile.CurrentSave.LastMap = gameMap.ToString();
			GameProfile.CurrentSave.LastSpawn = sourceMapName;
			GameProfile.Autosave();
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
						case "Trainer":
							if (!EntityEnabled(entity)) continue;
							trainerQueue.Enqueue(new Trainer(this, Tilemap, chunk, entity));
							break;

						case "NPC":
							if (!EntityEnabled(entity)) continue;
							npcQueue.Enqueue(new Npc(this, Tilemap, chunk, entity));
							break;

						case "Interactable":
						case "Automatic":
						case "Travel":
							if (!EntityEnabled(entity)) continue;
							eventQueue.Enqueue(new EventTrigger(this, chunk, entity));
							break;
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
					case "Music": if (!string.IsNullOrEmpty((string)field.Value)) mapMusic = Enum.Parse<GameMusic>((string)field.Value); Audio.PlayMusic(mapMusic); break;
					case "Script": if (!string.IsNullOrEmpty((string)field.Value)) AddController(new EventController(this, ((string)field.Value).Split('\n'))); break;

					case "LocationName": LocationName = (!string.IsNullOrEmpty((string)field.Value)) ? field.Value : Tilemap.Name; break;
					case "Background": if (!string.IsNullOrEmpty((string)field.Value)) BuildParallaxBackground((string)field.Value); break;
					case "BattleBackground": if (!string.IsNullOrEmpty((string)field.Value)) BattleBackground = field.Value; break;
				}
			}

            GameProfile.CurrentSave.LocationName.Value = LocationName.Replace('_', ' ');
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
            Trainers.RemoveAll(x => x.Terminated);

            AddQueuedEntities();

			parallaxBackdrop?.Update(gameTime, Camera);
        }

		public override void DrawBackground(SpriteBatch spriteBatch)
		{
			parallaxBackdrop?.Draw(spriteBatch);
			Tilemap.DrawBackground(spriteBatch, Camera);
		}

		public bool ProcessAutoEvents()
        {
            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds) && !eventTrigger.Interactive && !eventTrigger.TravelZone)
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

		private bool EntityEnabled(EntityInstance entityInstance)
		{
			var property = entityInstance.FieldInstances.FirstOrDefault(x => x.Identifier == "EnableIf");
			if (property != null && property.Value != null && !GameProfile.GetSaveData<bool>((string)property.Value)) return false;

			property = entityInstance.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
			return property == null || property.Value == null || !GameProfile.GetSaveData<bool>((string)property.Value);
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

			Trainer trainer;
			while (trainerQueue.TryDequeue(out trainer))
			{
				if (trainer.IdleScript != null) AddController(new TrainerController(this, trainer));
				Trainers.Add(trainer);
				AddEntity(trainer);
			}

			EventTrigger eventTrigger;
			while (eventQueue.TryDequeue(out eventTrigger))
			{
				EventTriggers.Add(eventTrigger);
			}
		}
	}
}
