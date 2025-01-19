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
    public class ChaseController : Controller
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Trainer trainer;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

		public ChaseController(MapScene iScene, Trainer iTrainer, PriorityLevel iPriorityLevel = PriorityLevel.CutsceneLevel)
            : base(iPriorityLevel)
        {
            mapScene = iScene;
            trainer = iTrainer;

			trainer.PriorityLevel = iPriorityLevel;

			currentTile = mapScene.Tilemap.GetTile(trainer.Center);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            if (trainer.Terminated) { Terminate(); return; }

			if (destinationTile == null)
            {
                if (trainer.HostTile.NeighborList.Contains(mapScene.PartyLeader.HostTile))
                {
					trainer.DesiredVelocity = Vector2.Zero;
					trainer.OrientedAnimation("Idle");

                    Terminate();
				}
                else ChaseParty();
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

		private void ChaseParty()
		{
			//Hero closestHero = mapScene.Party.MinBy(x => Math.Abs(x.TileX - trainer.TileX) + Math.Abs(x.TileY - trainer.TileY));
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
