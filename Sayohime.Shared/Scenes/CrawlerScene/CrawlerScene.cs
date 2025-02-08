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
		private const int ROOM_LENGTH = 10;

		public static CrawlerScene Instance;

        public string LocationName { get; set; } = "Test Map";

        private MapViewModel mapViewModel;

        private MovementController movementController;

        private float cameraX = 0.0f;
        private float cameraPosX = 0.0f;
        private float cameraPosZ = 0.0f;

        private Floor floor;
        public List<Foe> FoeList { get; set; } = new List<Foe>();
        public List<Chest> ChestList { get; set; } = new List<Chest>();
        public List<Npc> NpcList { get; set; } = new List<Npc>();

        public int roomX = -1;
        public int roomY = -1;
        private Direction direction;

        public Panel MapPanel { get; set; }

        private int bumpCooldown;

        public float BillboardRotation { get; private set; }

        public CrawlerScene()
        {
            Instance = this;
        }

        public CrawlerScene(GameMap gamemap, int x, int y, Direction dir) : this()
        {


            floor = new Floor(this, gamemap);
            LocationName = floor.LocationName;
            MapFileName = gamemap;

            roomX = x;
            roomY = y;
            direction = dir;
            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            floor.GetRoom(roomX, roomY).EnterRoom(false);

            movementController = AddController(new MovementController(this));

            mapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");
        }

        public CrawlerScene(string iMapName, string spawnName) : this()
        {
            MapFileName = (GameMap)Enum.Parse(typeof(GameMap), iMapName);

            floor = new Floor(this, MapFileName);
            LocationName = floor.LocationName;
            Spawn spawn = floor.Spawns[spawnName];
            roomX = spawn.RoomX;
            roomY = spawn.RoomY;
            direction = spawn.Direction;
            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            floor.GetRoom(roomX, roomY).EnterRoom(false);

            movementController = AddController(new MovementController(this));

            mapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");
        }

        public void ResetPathfinding()
        {
            movementController?.Path.Clear();
        }

        public void SaveData()
        {
            GameProfile.SetSaveData<GameMap>("LastMap", MapFileName);
            GameProfile.SetSaveData<int>("LastRoomX", roomX);
            GameProfile.SetSaveData<int>("LastRoomY", roomY);
            GameProfile.SetSaveData<Direction>("LastDirection", direction);
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

        public void TurnLeft()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t =>
            {
                cameraX = MathHelper.Lerp(((float)(Math.PI * ((int)direction - 1) / 2.0f)), (float)(Math.PI * (int)direction / 2.0f), t);
                if (t <= 0.5f)
                {
                    var dir = (direction == Direction.North) ? Direction.West : direction - 1;
                    BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
                }
            });

            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.North) direction = Direction.West; else direction--;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void TurnRight()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t =>
            {
                cameraX = MathHelper.Lerp(((float)(Math.PI * (int)direction / 2.0f)), (float)(Math.PI * ((int)direction + 1) / 2.0f), t);
                if (t >= 0.5f)
                {
                    var dir = (direction == Direction.West) ? Direction.North : direction + 1;
                    BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
                }
            });

            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.West) direction = Direction.North; else direction++;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void MoveForward()
        {
            var currentRoom = floor.GetRoom(roomX, roomY);
            var destinationRoom = currentRoom[direction];
            if (destinationRoom == null || destinationRoom.Blocked)
            {
                if (!Activate()) WallBump();
                return;
            }

            if (destinationRoom.PreEnterScript != null)
            {
                destinationRoom.ActivatePreScript();
                return;
            }

            if (destinationRoom.Foe != null)
            {
                InitiateBattle(currentRoom, destinationRoom);
            }
            else
            {
                TransitionDirection transitionDirection = (direction == Direction.North || direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
                TransitionController transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

                MoveFoes(destinationRoom);

                switch (direction)
                {
                    case Direction.North: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, ROOM_LENGTH, t)); break;
                    case Direction.East: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, ROOM_LENGTH, t)); break;
                    case Direction.South: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-ROOM_LENGTH, 0, t)); break;
                    case Direction.West: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-ROOM_LENGTH, 0, t)); break;
                }

                AddController(transitionController);
                transitionController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    cameraPosX = cameraPosZ = 0;
                    roomX = destinationRoom.RoomX;
                    roomY = destinationRoom.RoomY;
                    destinationRoom.EnterRoom();
                });
            }
        }

        public void MoveBackward()
        {
            Direction opposite = direction + 2;
            if (opposite > Direction.West) opposite -= 4;

            var currentRoom = floor.GetRoom(roomX, roomY);
            var destinationRoom = currentRoom[opposite];
            if (destinationRoom == null || destinationRoom.Blocked)
            {
                if (!Activate()) WallBump();
                return;
            }

            if (destinationRoom.PreEnterScript != null)
            {
                destinationRoom.ActivatePreScript();
                return;
            }

            if (destinationRoom.Foe != null)
            {
                InitiateBattle(currentRoom, destinationRoom);
            }
            else
            {
                TransitionDirection transitionDirection = (opposite == Direction.North || opposite == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
                TransitionController transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

                MoveFoes(destinationRoom);

                switch (opposite)
                {
                    case Direction.North: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, ROOM_LENGTH, t)); break;
                    case Direction.East: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, ROOM_LENGTH, t)); break;
                    case Direction.South: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-ROOM_LENGTH, 0, t)); break;
                    case Direction.West: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-ROOM_LENGTH, 0, t)); break;
                }

                AddController(transitionController);
                transitionController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    cameraPosX = cameraPosZ = 0;
                    roomX = destinationRoom.RoomX;
                    roomY = destinationRoom.RoomY;
                    destinationRoom.EnterRoom();
                });
            }
        }

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

        public void MoveTo(MapRoom destinationRoom)
        {
            Direction requiredDirection;
            if (destinationRoom.RoomX > roomX) requiredDirection = Direction.East;
            else if (destinationRoom.RoomX < roomX) requiredDirection = Direction.West;
            else if (destinationRoom.RoomY > roomY) requiredDirection = Direction.South;
            else requiredDirection = Direction.North;

            if (requiredDirection == direction) MoveForward();
            else
            {
                if (requiredDirection == direction + 1 || (requiredDirection == Direction.North && direction == Direction.West)) TurnRight();
                else TurnLeft();
            }
        }

        public void FinishMovement()
        {
            var currentRoom = floor.GetRoom(roomX, roomY);
            var destinationRoom = currentRoom[direction];

            TransitionDirection transitionDirection = (direction == Direction.North || direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
            var transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.TransitionLevel);

            switch (direction)
            {
                case Direction.North: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(2.95f, ROOM_LENGTH, t)); break;
                case Direction.East: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(2.95f, ROOM_LENGTH, t)); break;
                case Direction.South: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-ROOM_LENGTH, -2.95f, t)); break;
                case Direction.West: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-ROOM_LENGTH, -2.95f, t)); break;
            }

            AddController(transitionController);
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                cameraPosX = cameraPosZ = 0;
                roomX = destinationRoom.RoomX;
                roomY = destinationRoom.RoomY;
                destinationRoom.EnterRoom();
            });
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
			graphicsDevice.Viewport = new Viewport(10, 10, 580, 360);
			graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

			graphicsDevice.SetRenderTarget(null);
			if (CrossPlatformGame.SceneStack.Count == 0 || CrossPlatformGame.SceneStack.First() == this)
			{
				graphicsDevice.Clear(ClearOptions.DepthBuffer, Graphics.PURE_BLACK, 1.0f, 0);
			}
			graphicsDevice.BlendState = BlendState.Opaque;

			Vector3 cameraUp = new Vector3(0, -1, 0);
			Vector3 cameraPos = new Vector3(cameraPosX + ROOM_LENGTH * roomX, 0, cameraPosZ + ROOM_LENGTH * (floor.MapHeight - roomY));
			Matrix viewMatrix = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateRotationY(cameraX)), cameraUp);
			floor.DrawMap(graphicsDevice, mapViewModel.GetWidget<Panel>("MapPanel"), viewMatrix, cameraPos, cameraX);

			graphicsDevice.BlendState = BlendState.AlphaBlend;

			List<IBillboard> billboardList = [.. FoeList, .. ChestList, .. NpcList];
            foreach (IBillboard billboard in billboardList.OrderByDescending(x => Vector2.Distance(new Vector2(cameraPosX + ROOM_LENGTH * roomX, cameraPosZ + ROOM_LENGTH * (floor.MapHeight - roomY)), x.Position)))
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
			floor.DrawMiniMap(spriteBatch, miniMapBounds, Color.White, 0.6f, roomX, roomY, direction);
			spriteBatch.End();
		}

        public bool Activate()
        {
            var roomAhead = floor.GetRoom(roomX, roomY)[direction];
            if (roomAhead == null) return false;

            return roomAhead.Activate(direction);
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
