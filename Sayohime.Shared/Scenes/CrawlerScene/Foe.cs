using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Controllers;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    public interface IBillboard
    {
        public Vector3 Position { get; }
        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX);
    }

    public class Foe : IBillboard
    {
        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

        public Direction Direction { get; private set; }
        public MapRoom CurrentRoom { get; private set; }
        public MapRoom DestinationRoom { get; private set; }
        public float MoveInterval { get; set; }

        public string Encounter { get; set; }

        private bool wandering = true;

        public Foe(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;

            string sprite = "";
            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Encounter": Encounter = field.Value; break;
                    case "Sprite": sprite = field.Value; break;
                    case "Wandering": if (field.Value != null) wandering = field.Value; break;
                }
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            CurrentRoom.Foe = this;

            var texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + sprite)];
            float sizeX = texture.Width / 24.0f;
            float sizeY = texture.Height / 24.0f;
            Billboard = new Billboard(crawlerScene, iFloor, texture, sizeX, sizeY);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            //var battleScene = CrossPlatformGame.CurrentScene as BattleScene.BattleScene;
            //if (battleScene != null && battleScene.Foe == this) return;

            float x = 10 * CurrentRoom.RoomX;
            float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);
            float brightness = CurrentRoom.Brightness(CurrentRoom.brightnessLevel);

            if (IsMoving)
            {
                float destX = 10 * DestinationRoom.RoomX;
                float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                float destBrightness = CurrentRoom.Brightness(DestinationRoom.brightnessLevel);

                Billboard.Draw(viewMatrix, MathHelper.Lerp(x, destX, MoveInterval), MathHelper.Lerp(z, destZ, MoveInterval), cameraX, MathHelper.Lerp(brightness, destBrightness, MoveInterval));
            }
            else
            {
                Billboard.Draw(viewMatrix, x, z, cameraX, brightness);
            }
        }



        public void Move(MapRoom playerDestination)
        {
            if (!wandering) return;

            var moveDirection = (Direction)Rng.RandomInt(0, 3);

            var newRoom = CurrentRoom[moveDirection];
            if (newRoom == null || newRoom.Blocked || newRoom.Chest != null || newRoom == playerDestination) return;

            Direction = moveDirection;
            MoveInterval = 0.0f;
            DestinationRoom = newRoom;

            TransitionController controller = new TransitionController(TransitionDirection.In, 300, PriorityLevel.CutsceneLevel);
            crawlerScene.AddController(controller);
            controller.UpdateTransition += new Action<float>(t => MoveInterval = t);
            controller.FinishTransition += new Action<TransitionDirection>(t => { CurrentRoom.Foe = null; CurrentRoom = DestinationRoom; CurrentRoom.Foe = this; DestinationRoom = null; });
        }

        public void Threaten(Direction moveDirection)
        {
            Direction = moveDirection + 2;
            if (Direction > Direction.West) Direction = moveDirection - 2;

            MoveInterval = 0.0f;
            DestinationRoom = CurrentRoom[Direction];

            TransitionController controller = new TransitionController(TransitionDirection.In, 300, PriorityLevel.CutsceneLevel);
            crawlerScene.AddController(controller);
            controller.UpdateTransition += new Action<float>(t => MoveInterval = t * 0.28f);
            controller.FinishTransition += new Action<TransitionDirection>(t =>
            {
                MoveInterval = 0.28f;

                /*
                BattleScene.BattleScene battleScene = new BattleScene.BattleScene(Encounter, this);
                CrossPlatformGame.StackScene(battleScene, true);

                battleScene.OnTerminated += new TerminationFollowup(() =>
                {
                    if (GameProfile.CurrentSave.Party.Any(x => x.Value.HP.Value > 0))
                    {
                        crawlerScene.FinishMovement();
                    }
                });
                */
            });
        }

        public void Destroy()
        {
            crawlerScene.FoeList.Remove(this);
            CurrentRoom.Foe = null;
        }

        public Vector3 Position
        {
            get
            {
                float x = 10 * CurrentRoom.RoomX;
                float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);

                if (IsMoving)
                {
                    float destX = 10 * DestinationRoom.RoomX;
                    float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                    return new Vector3(MathHelper.Lerp(x, destX, MoveInterval), 0, MathHelper.Lerp(z, destZ, MoveInterval));
                }
                else
                {
                    return new Vector3(x, 0, z);
                }
            }
        }

        public bool IsMoving { get => DestinationRoom != null; }
    }
}
