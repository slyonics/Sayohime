using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;

namespace Sayohime.Scenes.MapScene
{
    public class NpcController : ScriptController
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Npc npc;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

        public NpcController(MapScene iScene, Npc iNpc)
            : base(iScene, iNpc.Behavior, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            npc = iNpc;

            currentTile = mapScene.Tilemap.GetTile(npc.Center);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (!scriptParser.Finished && destinationTile == null) base.PreUpdate(gameTime);
            if (scriptParser.Finished && destinationTile == null) return;

            if (destinationTile == null)
            {
                npc.DesiredVelocity = Vector2.Zero;
                npc.OrientedAnimation("Idle");
            }
            else
            {
                npc.DesiredVelocity = Vector2.Zero;
                npc.Reorient(destinationTile.Center - currentTile.Center);
                npc.OrientedAnimation("Walk");
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (mapScene.PriorityLevel > npc.PriorityLevel || (npc.PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0)) return;

            if (destinationTile != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    Vector2 npcPosition = Vector2.Lerp(destinationTile.Center, currentTile.Center, walkTimeLeft / currentWalkLength);
                    npc.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    npc.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;

                    if (mapScene.PriorityLevel > this.PriorityLevel || mapScene.BattleImminent) npc.Idle();
                }
            }
        }

        public bool Move(Orientation direction, float walkLength = DEFAULT_WALK_LENGTH)
        {
            npc.Orientation = direction;

            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile npcDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (npcDestination == null) return false;
            if (npcDestination.Blocked || npcDestination.Occupants.Count > 0) return false;
            //if (((CaterpillarController)mapScene.Party[0].ControllerList.Find(x => x is CaterpillarController)).OccupiedTile == npcDestination) return false;

            destinationTile = npcDestination;
            currentWalkLength = walkTimeLeft = walkLength;

            currentTile.Occupants.Remove(npc);
            destinationTile.Occupants.Add(npc);
            npc.HostTile = destinationTile;

            return true;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Wander": if (!mapScene.BattleImminent) Move((Orientation)Rng.RandomInt(0, 3), int.Parse(tokens[1]) / 1000.0f); break;
                case "Animate": npc.PlayAnimation(tokens[1]); break;
                default: return false;
            }

            return true;
        }
    }
}
