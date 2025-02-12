using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Widgets;
using Sayohime.Scenes.MapScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    public enum Direction
    {
        North, East, South, West, Up, Down
    }

    public class CrawlerScene : Scene
    {
		public const int ROOM_LENGTH = 10;

		public static CrawlerScene Instance;

        public string LocationName { get; set; } = "Test Map";

        private MapViewModel mapViewModel;

        public PartyController PartyController { get; private set; }


        private Floor floor;
        public List<Foe> FoeList { get; set; } = new List<Foe>();
        public List<Chest> ChestList { get; set; } = new List<Chest>();
        public List<Npc> NpcList { get; set; } = new List<Npc>();



        public Panel MapPanel { get; set; }

        private int bumpCooldown;

        public CrawlerScene()
        {
            Instance = this;
        }

        public CrawlerScene(GameMap gamemap, int x, int y, Direction dir) : this()
        {
            floor = new Floor(this, gamemap);
            LocationName = floor.LocationName;
            MapFileName = gamemap;

            PartyController = AddController(new PartyController(this, x, y, dir));

            floor.GetRoom(PartyController.RoomX, PartyController.RoomY).EnterRoom(false);

            mapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");
        }

        public CrawlerScene(string iMapName, string spawnName) : this()
        {
            MapFileName = (GameMap)Enum.Parse(typeof(GameMap), iMapName);

            floor = new Floor(this, MapFileName);
            LocationName = floor.LocationName;
            Spawn spawn = floor.Spawns[spawnName];

			PartyController = AddController(new PartyController(this, spawn.RoomX, spawn.RoomY, spawn.Direction));

			floor.GetRoom(PartyController.RoomX, PartyController.RoomY).EnterRoom(false);

            mapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");
        }

        public void ResetPathfinding()
        {
            PartyController?.Path.Clear();
        }

        public void SaveData()
        {
            GameProfile.SetSaveData<GameMap>("LastMap", MapFileName);
            GameProfile.SetSaveData<int>("LastRoomX", PartyController.RoomX);
            GameProfile.SetSaveData<int>("LastRoomY", PartyController.RoomY);
            GameProfile.SetSaveData<Direction>("LastDirection", PartyController.Direction);
            GameProfile.SetSaveData<string>("PlayerLocation", LocationName);

            GameProfile.SaveState();
        }

        public override void BeginScene()
        {
            base.BeginScene();

            // Audio.PlayMusic(GameMusic.AquaDungeon);
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.CurrentInput.CommandPressed(Command.Up) ||
                Input.CurrentInput.CommandPressed(Command.Down) ||
                Input.CurrentInput.CommandPressed(Command.Right) ||
                Input.CurrentInput.CommandPressed(Command.Left))
                ResetPathfinding();

            base.Update(gameTime);

            if (bumpCooldown > 0) bumpCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        /*
        private void InitiateBattle(MapRoom currentRoom, MapRoom destinationRoom)
        {
            TransitionDirection transitionDirection = (direction == Direction.North || direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
            TransitionController transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

            switch (direction)
            {
                case Direction.North: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, 2.95f, t)); break;
                case Direction.East: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, 2.95f, t)); break;
                case Direction.South: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-2.95f, 0, t)); break;
                case Direction.West: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-2.95f, 0, t)); break;
            }

            AddController(transitionController);
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                switch (direction)
                {
                    case Direction.North: cameraPosZ = 2.95f; break;
                    case Direction.East: cameraPosX = 2.95f; break;
                    case Direction.South: cameraPosZ = -2.95f; break;
                    case Direction.West: cameraPosX = -2.95f; break;
                }

                destinationRoom.Foe.Threaten(direction);
            });
        }
        */

        public MapRoom AttemptMoveForward()
        {
			var currentRoom = floor.GetRoom(PartyController.RoomX, PartyController.RoomY);
			var destinationRoom = currentRoom[PartyController.Direction];
			if (destinationRoom == null || destinationRoom.Blocked)
			{
				if (!Activate(destinationRoom)) WallBump();
				return null;
			}

			if (destinationRoom.PreEnterScript != null)
			{
				destinationRoom.ActivatePreScript();
				return null;
			}

			if (destinationRoom.Foe != null)
			{
				// InitiateBattle(currentRoom, destinationRoom);
			}
			else
			{
				MoveFoes(destinationRoom);
			}

            return destinationRoom;
		}
                
        private void WallBump()
        {
            if (bumpCooldown <= 0)
            {
                //Audio.PlaySound(GameSound.wall_bump);
                bumpCooldown = 350;
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender)
        {
            // Messy 1st person 3D dungeon crawler renderer

            float cameraX = PartyController.CameraX;
			Vector3 cameraUp = new Vector3(0, -1, 0);
			Vector3 cameraPos = new Vector3(PartyController.CameraPosX + ROOM_LENGTH * PartyController.RoomX, 0, PartyController.CameraPosZ + ROOM_LENGTH * (floor.MapHeight - PartyController.RoomY));
			Matrix viewMatrix = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateRotationY(cameraX)), cameraUp);

			graphicsDevice.SetRenderTarget(null);
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.Clear(new Color(0.0f, 1.0f, 0.5f, 0.0f));
			graphicsDevice.Viewport = new Viewport(10, 10, 580, 360);

			floor.Skybox?.Draw(graphicsDevice, viewMatrix, cameraPos);
			floor.DrawMap(graphicsDevice, mapViewModel.GetWidget<Panel>("MapPanel"), viewMatrix, cameraPos);

			graphicsDevice.BlendState = BlendState.AlphaBlend;

			List<IBillboard> billboardList = [.. FoeList, .. ChestList, .. NpcList];
            foreach (IBillboard billboard in billboardList.OrderByDescending(x => Vector3.Distance(cameraPos, x.Position)))
            {
                billboard.Draw(graphicsDevice, viewMatrix, cameraX);
            }

			/*
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);
            spriteBatch.End();
            */

			graphicsDevice.Viewport = new Viewport(0, 0, 880, 540);

			var miniMapPanel = MapViewModel.GetWidget<Panel>("MiniMapPanel");
			Rectangle miniMapBounds = miniMapPanel.InnerBounds;
			miniMapBounds.X += (int)miniMapPanel.Position.X;
			miniMapBounds.Y += (int)miniMapPanel.Position.Y;
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
			floor.DrawMiniMap(spriteBatch, miniMapBounds, Color.White, 0.6f, PartyController.RoomX, PartyController.RoomY, PartyController.Direction);
			spriteBatch.End();
		}

        public bool Activate(MapRoom roomAhead)
        {
            if (roomAhead == null) return false;

            return roomAhead.Activate(PartyController.Direction);
        }

        public void MoveFoes(MapRoom playerDestination)
        {
            foreach (Foe foe in FoeList)
            {
                foe.Move(playerDestination);
            }
        }


        public MapViewModel MapViewModel { get => mapViewModel; }




        public GameMap MapFileName { get; set; }

        public Floor Floor { get => floor; }
    }
}
