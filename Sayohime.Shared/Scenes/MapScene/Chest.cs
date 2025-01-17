using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;

using ldtk;

namespace Sayohime.Scenes.MapScene
{
    public class ChestController : Controller
    {
        Chest chest;
        MapScene mapScene;

        public ChestController(MapScene iMapScene, Chest iChest)
            : base(PriorityLevel.CutsceneLevel)
        {
            mapScene = iMapScene;
            chest = iChest;

            if (chest.SpriteName == "Sword") chest.AnimatedSprite.PlayAnimation("SwordOpening", ChestOpened);
            else chest.AnimatedSprite.PlayAnimation("Opening", ChestOpened);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            chest.AnimatedSprite.Update(gameTime);
        }

        public void ChestOpened()
        {
            string item = chest.Item;
            string[] script = new string[] { "GiveItem " + item, "SetFlag " + chest.Name + "Opened True" };
            EventController eventController = new EventController(mapScene, script);
            mapScene.AddController(eventController);

            Terminate();
        }
    }

    public class Chest : Npc
    {
        protected enum ChestAnimation
        {
            Closed,
            Opening,
            Open
        }


        public static readonly Rectangle CHEST_BOUNDS = new Rectangle(-8, -16, 16, 16);

        private static readonly Dictionary<string, Animation> CHEST_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { ChestAnimation.Closed.ToString(), new Animation(0, 0, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { ChestAnimation.Opening.ToString(), new Animation(new Rectangle[] { new Rectangle(0,16,16,16), new Rectangle(0,32,16,16),new Rectangle(0,48,16,16) }, new int[] { 200, 200, 50 }) },
            { ChestAnimation.Open.ToString(), new Animation(0, 3, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },

            { "SwordClosed", new Animation(new Rectangle[] { new Rectangle(0,0,16,16), new Rectangle(0,16,16,16), new Rectangle(0,32,16,16), new Rectangle(0,48,16,16) }, 500) },
            { "SwordOpening".ToString(), new Animation(new Rectangle[] { new Rectangle(0,0,1,1)}, 1000) },
            { "SwordOpen", new Animation(new Rectangle[] { new Rectangle(0,0,1,1) }, 1000) }
        };

        private MapScene mapScene;

        public string Name { get; set; }
        public string Item { get; set; }

        public string SpriteName { get; set; } = "Chest";

        public Chest(MapScene iMapScene, TileMap iTilemap, EntityInstance entityInstance)
            : base(iMapScene, iTilemap, entityInstance, Orientation.Down)
        {
            mapScene = iMapScene;

            Label = "Take";

            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Name": Name = field.Value; break;
                    case "Sprite": if (field.Value != null) SpriteName = field.Value; break;
                    case "Item": Item = field.Value; break;
                }
            }

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + SpriteName)], CHEST_ANIMATIONS);


            CenterOn(iTilemap.GetTile(new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height / 2)).Center);
            tilemap.GetTile(Center).Occupants.Add(this);

            if (Models.GameProfile.GetSaveData<bool>(Name + "Opened"))
            {
                Opened = true;
                if (SpriteName == "Sword") AnimatedSprite.PlayAnimation("SwordOpen");
                else AnimatedSprite.PlayAnimation("Open");
            }
            else if (SpriteName == "Sword") AnimatedSprite.PlayAnimation("SwordClosed");
        }

        public override bool Activate(Hero activator)
        {
            ChestController eventController = new ChestController(mapScene, this);
            mapScene.AddController(eventController);
            controllerList.Add(eventController);

            Opened = true;

            Audio.PlaySound(GameSound.Chest);

            return true;
        }

        public bool Opened { get; set; }

        public override bool Interactive { get => !Opened; }
    }
}
