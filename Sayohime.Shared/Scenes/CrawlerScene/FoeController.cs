using Sayohime.Models;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    /*
    public class FoeController : ScriptController
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private CrawlerScene mapScene;
        private Foe enemy;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

        public FoeController(CrawlerScene iScene, Foe iEnemy)
            : base(iScene, iEnemy.IdleScript, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            enemy = iEnemy;

            currentTile = mapScene.Tilemap.GetTile(enemy.Center);
            enemy.EnemyController = this;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            if (enemy.Terminated) { Terminate(); return; }
            if (!scriptParser.Finished) base.PreUpdate(gameTime);

            if (destinationTile == null)
            {
                enemy.DesiredVelocity = Vector2.Zero;
                enemy.OrientedAnimation("Idle");
            }
            else
            {
                enemy.DesiredVelocity = Vector2.Zero;
                enemy.Reorient(destinationTile.Center - currentTile.Center);
                enemy.OrientedAnimation("Walk");
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (mapScene.PriorityLevel > enemy.PriorityLevel || (enemy.PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0)) return;

            if (destinationTile != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    Vector2 npcPosition = Vector2.Lerp(destinationTile.Center, currentTile.Center, walkTimeLeft / currentWalkLength);
                    enemy.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    enemy.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;

                    if (mapScene.PriorityLevel > this.PriorityLevel || mapScene.BattleImminent) enemy.Idle();
                }
            }
        }

        public bool Move(Orientation direction, float walkLength = DEFAULT_WALK_LENGTH)
        {
            if (mapScene.BattleImminent) return false;

            enemy.Orientation = direction;

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
                enemy.Collides();
                return false;
            }

            if (enemyDestination.Blocked || enemyDestination.Occupants.Count > 0) return false;

            destinationTile = enemyDestination;
            currentWalkLength = walkTimeLeft = walkLength;

            currentTile.Occupants.Remove(enemy);
            destinationTile.Occupants.Add(enemy);
            enemy.HostTile = destinationTile;

            return true;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
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
                    case "$partyAdjacent": return mapScene.Party.Any(x => Math.Abs(x.TileX - enemy.TileX) + Math.Abs(x.TileY - enemy.TileY) == 1).ToString();
                    default: return null;
                }
            }
            else return null;
        }

        private void ChaseParty()
        {
            Hero closestHero = mapScene.Party.MinBy(x => Math.Abs(x.TileX - enemy.TileX) + Math.Abs(x.TileY - enemy.TileY));
            int deltaX = closestHero.TileX - enemy.TileX;
            int deltaY = closestHero.TileY - enemy.TileY;

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
    */
}
