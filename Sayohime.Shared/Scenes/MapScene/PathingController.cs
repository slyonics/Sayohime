using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using System.Collections.Generic;

namespace Sayohime.Scenes.MapScene
{
    public class PathingController : Controller
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Actor actor;
        private List<Tile> tilePath;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

		public PathingController(MapScene iScene, Actor iActor, Tile destination, PriorityLevel iPriorityLevel = PriorityLevel.CutsceneLevel)
            : base(iPriorityLevel)
        {
            mapScene = iScene;
            actor = iActor;

			actor.PriorityLevel = iPriorityLevel;

            currentTile = actor.HostTile;
            tilePath = mapScene.Tilemap.GetPath(currentTile, destination, 500);
            tilePath.Remove(currentTile);
		}

        public override void PreUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            if (actor.Terminated) { Terminate(); return; }

			if (destinationTile == null)
            {
                if (tilePath.Count == 0)
                {
					actor.DesiredVelocity = Vector2.Zero;
					actor.OrientedAnimation("Idle");

                    Terminate();
				}
                else ContinuePath();
            }
            else
            {
                actor.DesiredVelocity = Vector2.Zero;
                actor.Reorient(destinationTile.Center - currentTile.Center);
                actor.OrientedAnimation("Walk");
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (mapScene.PriorityLevel > actor.PriorityLevel || (actor.PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0)) return;

            if (destinationTile != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    Vector2 npcPosition = Vector2.Lerp(destinationTile.Center, currentTile.Center, walkTimeLeft / currentWalkLength);
                    actor.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    actor.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;

                    if (mapScene.PriorityLevel > this.PriorityLevel || mapScene.BattleImminent) actor.Idle();
                }
            }
        }

		private void ContinuePath()
		{
            currentTile = actor.HostTile;
            destinationTile = tilePath.First();
			currentWalkLength = walkTimeLeft = DEFAULT_WALK_LENGTH;
			tilePath.RemoveAt(0);

			int deltaX = destinationTile.TileX - currentTile.TileX;
			int deltaY = destinationTile.TileY - currentTile.TileY;

			if (Math.Abs(deltaX) >= Math.Abs(deltaY))
			{
				if (deltaX < 0) actor.Orientation = Orientation.Left;
				else actor.Orientation = Orientation.Right;
			}
			else
			{
				if (deltaY < 0) actor.Orientation = Orientation.Up;
				else actor.Orientation = Orientation.Down;
			}

			currentTile.Occupants.Remove(actor);
			destinationTile.Occupants.Add(actor);
			actor.HostTile = destinationTile;
		}
	}
}
