using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Shaders;
using System.Threading.Tasks;
using Sayohime.SceneObjects.Particles;

namespace Sayohime.Scenes.MapScene
{
    public interface IInteractive
    {
        bool Activate(Hero activator);
        Rectangle Bounds { get; }
        string Label { get; }
        Vector2 LabelPosition { get; }
    }

    public class CaterpillarController : Controller
    {
        private class HeroMovement
        {
            public Hero hero;
            public Tile currentTile;
            public Tile destinationTile;

            public HeroMovement(Hero iHero, Tile iCurrentTile, Tile iDestinationTile)
            {
                hero = iHero;
                currentTile = iCurrentTile;
                destinationTile = iDestinationTile;
            }
        }

        private const float DEFAULT_WALK_LENGTH = 1.0f / 4;

        private MapScene mapScene;

        private List<HeroMovement> movementList;
        private float walkLength;
        private float walkTimeLeft;

        private IInteractive interactable;
        private InteractionPrompt interactionView;

        public CaterpillarController(MapScene iMapScene, float iWalkLength = DEFAULT_WALK_LENGTH)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            walkLength = iWalkLength;

            interactionView = new InteractionPrompt(mapScene);
            mapScene.AddOverlay(interactionView);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (CrossPlatformGame.CurrentScene is not MapScene || mapScene.Party[0].Hide) return;

            InputFrame playerInput = Input.CurrentInput;

            if (mapScene.PriorityLevel == PriorityLevel.GameLevel)
            {
                foreach (Hero hero in mapScene.Party) hero.PriorityLevel = PriorityLevel.GameLevel;

                if (movementList == null)
                {
                    if (mapScene.ProcessAutoEvents()) return;
                    if (TrainerEncounters()) return;

                    if (Input.CurrentInput.CommandPressed(Command.Menu))
                    {
                        /*
                        Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                        StatusScene.StatusScene statusScene = new StatusScene.StatusScene(mapScene.LocationName);
                        statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                        CrossPlatformGame.StackScene(statusScene);

                        return;
                        */
                    }
                    
                    bool moveResult = true;
                    if (playerInput.CommandDown(Command.Up)) moveResult = Move(Orientation.Up);
                    else if (playerInput.CommandDown(Command.Right)) moveResult = Move(Orientation.Right);
                    else if (playerInput.CommandDown(Command.Down)) moveResult = Move(Orientation.Down);
                    else if (playerInput.CommandDown(Command.Left)) moveResult = Move(Orientation.Left);
                    else
                    {
                        foreach (Hero hero in mapScene.Party)
                        {
                            hero.DesiredVelocity = Vector2.Zero;
                            if (hero.AnimatedSprite.Frame % 2 != 0) hero.OrientedAnimation("Idle");
                        }
                    }

                    if (!moveResult)
                    {
						foreach (Hero hero in mapScene.Party) hero.OrientedAnimation("Idle");
                    }
                }
            }

            if (movementList != null)
            {
                foreach (HeroMovement movement in movementList)
                {
                    movement.hero.DesiredVelocity = Vector2.Zero;
                    movement.hero.Reorient(movement.destinationTile.Center - movement.currentTile.Center);
                    if (!mapScene.BattleImminent) movement.hero.OrientedAnimation("Walk");
                }
            }
            else if (playerInput.CommandPressed(Command.Interact) && interactable != null)
            {
                if (interactable.Activate(mapScene.Party[0]))
                {
                    EventTrigger.LastTrigger = interactable as EventTrigger;

                    Idle();
                    interactable = null;
                }
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (movementList != null)
            {
                interactable = null;
                interactionView.Target(interactable);

                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    float interval = 1.0f - walkTimeLeft / walkLength;
                    int heroOffset = (int)(interval * TileMap.TILE_SIZE);

                    // mapScene.GameMap.Weather.ProceedTime(interval);

                    foreach (HeroMovement movement in movementList)
                    {
                        Vector2 directionVector = movement.destinationTile.Center - movement.currentTile.Center;
                        directionVector.Normalize();

                        Vector2 heroPosition = movement.currentTile.Center + directionVector * heroOffset;
                        movement.hero.CenterOn(new Vector2((int)heroPosition.X, (int)heroPosition.Y));
                        // movement.hero.UpdateLight(gameTime);
                    }
                }
                else
                {
                    // mapScene.GameMap.Weather.ProceedTime((float)gameTime.ElapsedGameTime.TotalSeconds + walkTimeLeft);

                    GameProfile.CurrentSave.Steps.Value = GameProfile.CurrentSave.Steps.Value + 1;
                    if (GameProfile.CurrentSave.Steps.Value % 4 == 3)
                    {
                        // poison steps
                    }

                    foreach (HeroMovement movement in movementList)
                    {
                        movement.hero.CenterOn(movement.destinationTile.Center);
                        // movement.hero.UpdateLight(gameTime);
                    }

                    movementList = null;

                    FinishMovement?.Invoke();
                    FinishMovement = null;

					// mapScene.SaveMapPosition();
					mapScene.LoadNeighborChunks();
                    mapScene.DetectNewChunk(mapScene.Tilemap.GetTile(mapScene.PartyLeader.Center));

                    if (mapScene.BattleImminent) Idle();
                }
            }
            else FindInteractables();
        }

        public bool TrainerEncounters()
        {
            Trainer trainer = mapScene.Trainers.FirstOrDefault(x => x.Triggered());
            if (trainer != null)
            {
				mapScene.AddParticle(new EmoteParticle(mapScene, AnimationType.Exclamation, trainer));

				string[] script = new string[] { "Wait 600", "MoveTrainer", "FaceTrainer", $"Conversation {trainer.Name}Fight", "Emote Exclamation Player", "Wait 600", $"Encounter {trainer.Name}" };
                EventController eventController = new EventController(mapScene, script);
                eventController.ActorSubject = trainer;

                mapScene.AddController(eventController);

                trainer.Reorient(mapScene.PartyLeader.Center - trainer.Center);
                trainer.OrientedAnimation("Idle");

                return true;
            }

            return false;
        }

        private void FindInteractables()
        {
            List<IInteractive> interactableList = new List<IInteractive>();
            interactableList.AddRange(mapScene.NPCs.FindAll(x => x.Interactive));
            interactableList.AddRange(mapScene.EventTriggers.FindAll(x => x.Interactive));
			interactableList.AddRange(mapScene.Trainers);

			Hero player = mapScene.PartyLeader;
            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => player.Distance(x.Bounds));
            Rectangle interactZone = player.Bounds;
            int zoneWidth = TileMap.TILE_SIZE;
            int zoneHeight = TileMap.TILE_SIZE;
            interactZone = new Rectangle((int)player.HostTile.Center.X - zoneWidth / 2, (int)player.HostTile.Center.Y - zoneHeight / 2, zoneWidth, zoneHeight);
            switch (player.Orientation)
            {
                case Orientation.Up:
                    interactZone.Y -= zoneHeight;
                    break;
                case Orientation.Right:
                    interactZone.X += zoneHeight;
                    break;
                case Orientation.Down:
                    interactZone.Y += zoneHeight;
                    break;
                case Orientation.Left:
                    interactZone.X -= zoneHeight;
                    break;
            }
            player.InteractionZone = interactZone;
            interactable = sortedInteractableList.FirstOrDefault(x => x.Bounds.Intersects(player.InteractionZone));
            interactionView.Target(interactable);
        }

        public bool Move(Orientation direction, bool reorient = false)
        {
            if (mapScene.BattleImminent) return false;

            Hero leader = mapScene.Party[0];
            leader.Orientation = direction;

            Tile currentTile = mapScene.Tilemap.GetTile(leader.Center);
            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile leaderDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (leaderDestination == null) return false;

            Trainer enemy = leaderDestination.Occupants.FirstOrDefault(x => x is Trainer) as Trainer;
            if (enemy != null)
            {
                Audio.PlaySound(GameSound.Blip);
                return false;
            }

            if (leaderDestination.Blocked || leaderDestination.Occupants.Any(x => !(x is Hero))) return false;

            movementList = new List<HeroMovement>();
            movementList.Add(new HeroMovement(leader, mapScene.Tilemap.GetTile(leader.Center), leaderDestination));

            for (int i = 1; i < mapScene.Party.Count; i++)
            {
                Tile current = mapScene.Tilemap.GetTile(mapScene.Party[i].Center);
                Tile destination = mapScene.Tilemap.GetTile(mapScene.Party[i - 1].Center);
                if (current != destination) movementList.Add(new HeroMovement(mapScene.Party[i], current, destination));

                current.Occupants.Remove(mapScene.Party[i]);
                destination.Occupants.Add(mapScene.Party[i]);
                mapScene.Party[i].HostTile = destination;
            }

            mapScene.Tilemap.ClearFieldOfView();
            foreach (HeroMovement heroMovement in movementList) mapScene.Tilemap.CalculateFieldOfView(heroMovement.destinationTile, MapScene.SIGHT_RANGE);

            currentTile.Occupants.Remove(leader);
            leaderDestination.Occupants.Add(leader);
            leader.HostTile = leaderDestination;

            walkTimeLeft = walkLength;
            interactable = null;
            interactionView.Target(interactable);

            if (reorient)
            {
                foreach (HeroMovement movement in movementList)
                {
                    movement.hero.DesiredVelocity = Vector2.Zero;
                    movement.hero.Reorient(movement.destinationTile.Center - movement.currentTile.Center);
                    movement.hero.OrientedAnimation("Walk");
                }
            }

            return true;
        }

        public void Idle()
        {
            foreach (Hero hero in mapScene.Party) hero.OrientedAnimation("Idle");
        }

        public ScriptParser.UnblockFollowup FinishMovement { get; set; }
    }
}
