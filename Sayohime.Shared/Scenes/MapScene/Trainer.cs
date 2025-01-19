using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using ldtk;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Particles;
using Sayohime.SceneObjects.Shaders;
using Sayohime.Models;

namespace Sayohime.Scenes.MapScene
{
    public class Trainer : Actor, IInteractive
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

        public const int NPC_WIDTH = 20;
        public const int NPC_HEIGHT = 24;

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

        private bool defeated;

        public Trainer(MapScene iMapScene, TileMap iTilemap, Chunk chunk, EntityInstance entityInstance, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            PaletteHue color = PaletteHue.Green;
            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Name": Name = (string)field.Value; break;
                    case "Sprite": animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse<GameSprite>("Actors_" + (string)field.Value)], NPC_ANIMATIONS); break;
                    case "Color": color = Enum.Parse<PaletteHue>((string)field.Value); break;
                    case "Idle": if (!string.IsNullOrEmpty((string)field.Value)) IdleScript = ((string)field.Value).Split('\n'); break;
                    case "Orientation": Orientation = Enum.Parse<Orientation>(field.Value); break;
                }
            }

            defeated = GameProfile.GetSaveData<bool>($"Trainer{Name}Defeated");
            if (defeated)
            {
                IdleScript = null;
            }

			Idle();

            CenterOn(iTilemap.GetTile(new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height / 2) + chunk.OffsetPx).Center);

            tilemap.GetTile(Center).Occupants.Add(this);
            HostTile = tilemap.GetTile(Center);
        }

        public override void Update(GameTime gameTime)
        {
            if (PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0) return;

            base.Update(gameTime);
        }

        public bool Triggered()
        {
            if (defeated) return false;
            if (HostTile.Obscured) return false;
            if (!mapScene.Camera.View.Intersects(SpriteBounds)) return false;

            switch (Orientation)
            {
                case Orientation.Up:
                    if (HostTile.TileX != mapScene.PartyLeader.HostTile.TileX || HostTile.TileY <= mapScene.PartyLeader.HostTile.TileY) return false;
                    for (int y = HostTile.TileY; y >= mapScene.PartyLeader.HostTile.TileY; y--)
                    {
                        var tile = tilemap.GetTile(HostTile.TileX, y);
                        if (tile == null || tile.Blocked) return false;
                    }
                    return true;

                case Orientation.Right:
                    if (HostTile.TileY != mapScene.PartyLeader.HostTile.TileY || HostTile.TileX >= mapScene.PartyLeader.HostTile.TileX) return false;
                    for (int x = HostTile.TileX; x <= mapScene.PartyLeader.HostTile.TileX; x++)
                    {
                        var tile = tilemap.GetTile(x, HostTile.TileY);
                        if (tile == null || tile.Blocked) return false;
                    }
                    return true;

                case Orientation.Down:
                    if (HostTile.TileX != mapScene.PartyLeader.HostTile.TileX || HostTile.TileY >= mapScene.PartyLeader.HostTile.TileY) return false;
                    for (int y = HostTile.TileY; y <= mapScene.PartyLeader.HostTile.TileY; y++)
                    {
                        var tile = tilemap.GetTile(HostTile.TileX, y);
                        if (tile == null || tile.Blocked) return false;
                    }
                    return true;

                case Orientation.Left:
                    if (HostTile.TileY != mapScene.PartyLeader.HostTile.TileY || HostTile.TileX <= mapScene.PartyLeader.HostTile.TileX) return false;
                    for (int x = HostTile.TileX; x >= mapScene.PartyLeader.HostTile.TileX; x--)
                    {
                        var tile = tilemap.GetTile(x, HostTile.TileY);
                        if (tile == null || tile.Blocked) return false;
                    }
                    return true;
            }

            return false;
        }

		public virtual bool Activate(Hero activator)
		{
			if (defeated)
			{
				Reorient(mapScene.PartyLeader.Center - this.Center);
                Idle();

				mapScene.AddController(new EventController(mapScene, [$"Conversation {Name}Talk"]));
			}
            else
            {
				Reorient(mapScene.PartyLeader.Center - this.Center);
				Idle();

				mapScene.CaterpillarController.TrainerEncounters();
			}

			return true;
		}

        public void Defeat()
        {
            defeated = true;
            TrainerController?.Terminate();
            TrainerController = null;

            mapScene.BattleImminent = false;

			GameProfile.SetSaveData<bool>($"Trainer{Name}Defeated", true);

            if (Name == "Scarecrow")
            {
				mapScene.AddController(new EventController(mapScene, ["Conversation Outro"]));
			}
		}

		public string Name { get; private set; }

        public string[] IdleScript { get; private set; } = null;
        public string[] CollideScript { get; private set; } = null;

		public string Label { get; protected set; } = "Trainer";
		public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - animatedSprite.SpriteBounds().Height - 4); }

		public TrainerController TrainerController { get; set; }

        public int TileX { get => HostTile.TileX; }
        public int TileY { get => HostTile.TileY; }
    }
}
