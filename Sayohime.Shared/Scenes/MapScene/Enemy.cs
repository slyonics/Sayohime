using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ldtk;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Particles;

namespace Sayohime.Scenes.MapScene
{
    public class Enemy : Actor
    {
        protected enum NpcAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int NPC_WIDTH = 16;
        public const int NPC_HEIGHT = 16;

        public static readonly Rectangle NPC_BOUNDS = new Rectangle(-8, -16, 16, 16);

        private static readonly Dictionary<string, Animation> NPC_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { NpcAnimation.IdleDown.ToString(), new Animation(1, 0, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleLeft.ToString(), new Animation(1, 1, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleRight.ToString(), new Animation(1, 2, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleUp.ToString(), new Animation(1, 3, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.WalkDown.ToString(), new Animation(0, 0, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkLeft.ToString(), new Animation(0, 1, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkRight.ToString(), new Animation(0, 2, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkUp.ToString(), new Animation(0, 3, NPC_WIDTH, NPC_HEIGHT, 4, 240) }
        };

        private MapScene mapScene;

        public Enemy(MapScene iMapScene, TileMap iTilemap, EntityInstance entityInstance, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Sprite": animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + field.Value)], NPC_ANIMATIONS); break;
                    case "Idle": if (!string.IsNullOrEmpty(field.Value)) IdleScript = field.Value.Split('\n'); break;
                    case "Collide": CollideScript = field.Value.Split('\n'); break;
                }
            }

            CenterOn(iTilemap.GetTile(new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height / 2)).Center);

            tilemap.GetTile(Center).Occupants.Add(this);
            HostTile = tilemap.GetTile(Center);
        }

        public Enemy(MapScene iMapScene, TileMap iTilemap, int x, int y, string spriteName, string encounterName, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + spriteName)], NPC_ANIMATIONS);
            IdleScript = new string[] { "Wait 2000", "Wander 800", "Repeat" };
            CollideScript = new string[] { "Sound Encounter", "Wait 700", "Encounter " + encounterName };

            CenterOn(iTilemap.GetTile(x, y).Center);

            tilemap.GetTile(Center).Occupants.Add(this);
            HostTile = tilemap.GetTile(Center);
        }

        public override void Update(GameTime gameTime)
        {
            if (PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0) return;

            base.Update(gameTime);
        }

        public void Collides()
        {
            priorityLevel = PriorityLevel.CutsceneLevel;

            mapScene.BattleImminent = true;

            mapScene.AddParticle(new EmoteParticle(mapScene, AnimationType.Exclamation, mapScene.PartyLeader));

            EventController eventController = new EventController(mapScene, CollideScript);
            mapScene.AddController(eventController);
        }

        public string[] IdleScript { get; private set; } = null;
        public string[] CollideScript { get; private set; } = null;

        public EnemyController EnemyController { get; set; }
        public int TileX { get => HostTile.TileX; }
        public int TileY { get => HostTile.TileY; }
    }
}
