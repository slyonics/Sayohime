using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;

namespace Sayohime.Scenes.MapScene
{
    public class TrainerController : ScriptController
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Trainer trainer;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;


		public TrainerController(MapScene iScene, Trainer iTrainer)
            : base(iScene, iTrainer.IdleScript, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            trainer = iTrainer;

            currentTile = mapScene.Tilemap.GetTile(trainer.Center);
            trainer.TrainerController = this;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            if (trainer.Terminated) { Terminate(); return; }
            if (!scriptParser.Finished) base.PreUpdate(gameTime);

			if (destinationTile == null)
            {
                trainer.DesiredVelocity = Vector2.Zero;
                trainer.OrientedAnimation("Idle");
            }
            else
            {
                trainer.DesiredVelocity = Vector2.Zero;
                trainer.Reorient(destinationTile.Center - currentTile.Center);
                trainer.OrientedAnimation("Walk");
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (mapScene.PriorityLevel > trainer.PriorityLevel || (trainer.PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0)) return;

            if (destinationTile != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    Vector2 npcPosition = Vector2.Lerp(destinationTile.Center, currentTile.Center, walkTimeLeft / currentWalkLength);
                    trainer.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    trainer.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;

                    if (mapScene.PriorityLevel > this.PriorityLevel || mapScene.BattleImminent) trainer.Idle();
                }
            }
        }

        public bool Move(Orientation direction, float walkLength = DEFAULT_WALK_LENGTH)
        {
            if (mapScene.BattleImminent) return false;

            trainer.Orientation = direction;

            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile enemyDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (enemyDestination == null) return false;

            Hero hero = enemyDestination.Occupants.FirstOrDefault(x => x is Hero) as Hero;
            if (hero != null)
            {
                return false;
            }

            if (enemyDestination.Blocked || enemyDestination.Occupants.Count > 0) return false;

            destinationTile = enemyDestination;
            currentWalkLength = walkTimeLeft = walkLength;

            currentTile.Occupants.Remove(trainer);
            destinationTile.Occupants.Add(trainer);
            trainer.HostTile = destinationTile;

            return true;
        }

        private void Turn(string[] tokens)
        {
            switch (tokens[1])
            {
                case "Clockwise":
                    if (trainer.Orientation == Orientation.Left) trainer.Orientation = Orientation.Up;
                    else trainer.Orientation++;
                    break;

                case "CounterClockwise":
                    if (trainer.Orientation == Orientation.Up) trainer.Orientation = Orientation.Left;
                    else trainer.Orientation--;
                    break;
            }

            trainer.Idle();
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Turn": Turn(tokens); break;
                case "Wander": if (!mapScene.BattleImminent) Move((Orientation)Rng.RandomInt(0, 3), int.Parse(tokens[1]) / 1000.0f); break;
				case "ChaseParty": ChaseParty(); break;
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
                    case "$partyAdjacent": return mapScene.Party.Any(x => Math.Abs(x.TileX - trainer.TileX) + Math.Abs(x.TileY - trainer.TileY) == 1).ToString();
                    default: return null;
                }
            }
            else return null;
		}

		private void ChaseParty()
        {
            Hero closestHero = mapScene.PartyLeader;
			int deltaX = closestHero.TileX - trainer.TileX;
            int deltaY = closestHero.TileY - trainer.TileY;

            if (Math.Abs(deltaX) >= Math.Abs(deltaY))
            {
                if (deltaX < 0) Move(Orientation.Left);
                else Move(Orientation.Right);
            }
            else
            {
                if (deltaY < 0) Move(Orientation.Up);
                else Move(Orientation.Down);
            }
        }
    }
}
