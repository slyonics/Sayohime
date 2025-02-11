using System.Collections.Generic;
using System.Linq;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using System;

using Microsoft.Xna.Framework;
using System.Drawing;

namespace Sayohime.Scenes.CrawlerScene
{
    public class PartyController : Controller
    {
        private CrawlerScene crawlerScene;

		// Position and Heading for navigating TileMap corridor mazes
		public int RoomX { get; private set; } = -1;
		public int RoomY { get; private set; } = -1;
		public Direction Direction { get; private set; }

		// Position and Heading for 3D camera rendering
		public float CameraX { get; private set; } = 0.0f;
		public float CameraPosX { get; private set; } = 0.0f;
		public float CameraPosZ { get; private set; } = 0.0f;
		public float BillboardRotation { get; private set; }

		public PartyController(CrawlerScene iScene, int x, int y, Direction dir)
			: base(PriorityLevel.GameLevel)
        {
            crawlerScene = iScene;

			RoomX = x;
			RoomY = y;
			Direction = dir;
			CameraX = (float)(Math.PI * (int)Direction / 2.0f);
		}

        public override void PreUpdate(GameTime gameTime)
        {
            if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;

            InputFrame inputFrame = Input.CurrentInput;
            if (inputFrame.CommandDown(Command.Left)) { Path.Clear(); TurnLeft(); }
            else if (inputFrame.CommandDown(Command.Right)) { Path.Clear(); TurnRight(); }
            else if (inputFrame.CommandDown(Command.Up)) { Path.Clear(); MoveForward(); }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm))
            {
                Path.Clear();



				crawlerScene.Activate(crawlerScene.Floor.GetRoom(RoomX, RoomY)[Direction]);

                /*
                mapViewModel.ModelProperties["MapActor"].Value = "Actors_Commando";

                Task.Run(() => Activator.CreateInstance(typeof(MatchScene.MatchScene))).ContinueWith(t =>
                {
                    CrossPlatformGame.StackScene((Scene)t.Result);
                });
                */
            }
            else if (Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Path.Clear();


                    //StatusScene.StatusScene statusScene = new StatusScene.StatusScene(mapScene.LocationName, true);
                    //CrossPlatformGame.StackScene(statusScene, true);

                //mapScene.AddView(new MenuViewModel(mapScene));

                /*

                Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                StatusScene.StatusScene statusScene = new StatusScene.StatusScene();
                statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                CrossPlatformGame.StackScene(statusScene);

                */

                return;
            }
            /*else if (Path.Count > 0)
            {
                MapRoom nextRoom = Path.First();
                if (mapScene.roomX == nextRoom.RoomX && mapScene.roomY == nextRoom.RoomY) Path.RemoveAt(0);
                else mapScene.MoveTo(nextRoom);
            }*/
        }

		public void MoveTo(MapRoom destinationRoom)
		{
			Direction requiredDirection;
			if (destinationRoom.RoomX > RoomX) requiredDirection = Direction.East;
			else if (destinationRoom.RoomX < RoomX) requiredDirection = Direction.West;
			else if (destinationRoom.RoomY > RoomY) requiredDirection = Direction.South;
			else requiredDirection = Direction.North;

			if (requiredDirection == Direction) MoveForward();
			else
			{
				if (requiredDirection == Direction + 1 || (requiredDirection == Direction.North && Direction == Direction.West)) TurnRight();
				else TurnLeft();
			}
		}

		public void TurnLeft()
		{
			TransitionController transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
			crawlerScene.AddController(transitionController);

			transitionController.UpdateTransition += new Action<float>(t =>
			{
				CameraX = MathHelper.Lerp((float)(Math.PI * ((int)Direction - 1) / 2.0f), (float)(Math.PI * (int)Direction / 2.0f), t);
				if (t <= 0.5f)
				{
					var dir = (Direction == Direction.North) ? Direction.West : Direction - 1;
					BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
				}
			});

			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (Direction == Direction.North) Direction = Direction.West; else Direction--;
				CameraX = (float)(Math.PI * (int)Direction / 2.0f);
			});
		}

		public void TurnRight()
		{
			TransitionController transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
			crawlerScene.AddController(transitionController);

			transitionController.UpdateTransition += new Action<float>(t =>
			{
				CameraX = MathHelper.Lerp(((float)(Math.PI * (int)Direction / 2.0f)), (float)(Math.PI * ((int)Direction + 1) / 2.0f), t);
				if (t >= 0.5f)
				{
					var dir = (Direction == Direction.West) ? Direction.North : Direction + 1;
					BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
				}
			});

			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (Direction == Direction.West) Direction = Direction.North; else Direction++;
				CameraX = (float)(Math.PI * (int)Direction / 2.0f);
			});
		}

		public void MoveForward()
		{
			var destinationRoom = crawlerScene.AttemptMoveForward();

			TransitionDirection transitionDirection = (Direction == Direction.North || Direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			TransitionController transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

			switch (Direction)
			{
				case Direction.North: transitionController.UpdateTransition += new Action<float>(t => CameraPosZ = MathHelper.Lerp(0, CrawlerScene.ROOM_LENGTH, t)); break;
				case Direction.East: transitionController.UpdateTransition += new Action<float>(t => CameraPosX = MathHelper.Lerp(0, CrawlerScene.ROOM_LENGTH, t)); break;
				case Direction.South: transitionController.UpdateTransition += new Action<float>(t => CameraPosZ = MathHelper.Lerp(-CrawlerScene.ROOM_LENGTH, 0, t)); break;
				case Direction.West: transitionController.UpdateTransition += new Action<float>(t => CameraPosX = MathHelper.Lerp(-CrawlerScene.ROOM_LENGTH, 0, t)); break;
			}

			crawlerScene.AddController(transitionController);
			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;
				destinationRoom.EnterRoom();
			});
		}

		/*
		public void MoveBackward()
		{
			Direction opposite = Direction + 2;
			if (opposite > Direction.West) opposite -= 4;

			var currentRoom = floor.GetRoom(RoomX, RoomY);
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
					RoomX = destinationRoom.RoomX;
					RoomY = destinationRoom.RoomY;
					destinationRoom.EnterRoom();
				});
			}
		}
		*/

		public void FinishMovement()
		{
			var currentRoom = crawlerScene.Floor.GetRoom(RoomX, RoomY);
			var destinationRoom = currentRoom[Direction];

			TransitionDirection transitionDirection = (Direction == Direction.North || Direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			var transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.TransitionLevel);

			switch (Direction)
			{
				case Direction.North: transitionController.UpdateTransition += new Action<float>(t => CameraPosZ = MathHelper.Lerp(2.95f, CrawlerScene.ROOM_LENGTH, t)); break;
				case Direction.East: transitionController.UpdateTransition += new Action<float>(t => CameraPosX = MathHelper.Lerp(2.95f, CrawlerScene.ROOM_LENGTH, t)); break;
				case Direction.South: transitionController.UpdateTransition += new Action<float>(t => CameraPosZ = MathHelper.Lerp(-CrawlerScene.ROOM_LENGTH, -2.95f, t)); break;
				case Direction.West: transitionController.UpdateTransition += new Action<float>(t => CameraPosX = MathHelper.Lerp(-CrawlerScene.ROOM_LENGTH, -2.95f, t)); break;
			}

			crawlerScene.AddController(transitionController);
			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;
				destinationRoom.EnterRoom();
			});
		}

		public List<MapRoom> Path { get; set; } = new List<MapRoom>();
    }
}
