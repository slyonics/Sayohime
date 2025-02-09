using Sayohime.Main;
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
    public class Npc : IBillboard
    {
        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

        public Direction Direction { get; private set; }
        public MapRoom CurrentRoom { get; private set; }
        public MapRoom DestinationRoom { get; private set; }
        public float MoveInterval { get; set; }

        public string Item { get; set; }
        public string Name { get; set; } = "NPC";

        public Npc(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;

            string sprite = "";
            string[] script = null;
            int size = 5;
            int heightOffset = 0;
            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Name": Name = field.Value; break;
                    case "Sprite": sprite = field.Value; break;
                    case "Script": if (!string.IsNullOrEmpty(field.Value)) script = field.Value.Split('\n'); break;
                    case "Size": size = (int)field.Value; break;
                    case "Height": heightOffset = (int)field.Value; break;
				}
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            if (script == null) CurrentRoom.Blocked = true;
            else CurrentRoom.PreEnterScript = CurrentRoom.InteractScript = script;

            var texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "NPCs_" + sprite)];
            float sizeX = texture.Width / 24.0f * size;
            float sizeY = texture.Height / 24.0f * size;
            Billboard = new Billboard(crawlerScene, iFloor, texture, sizeX, sizeY, heightOffset);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
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

        public Vector2 Position
        {
            get
            {
                float x = 10 * CurrentRoom.RoomX;
                float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);

                if (IsMoving)
                {
                    float destX = 10 * DestinationRoom.RoomX;
                    float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                    return new Vector2(MathHelper.Lerp(x, destX, MoveInterval), MathHelper.Lerp(z, destZ, MoveInterval));
                }
                else
                {
                    return new Vector2(x, z);
                }
            }
        }

        public void Destroy()
        {
            crawlerScene.NpcList.Remove(this);
            CurrentRoom.PreEnterScript = null;
            CurrentRoom.InteractScript = null;
        }

        public bool IsMoving { get => DestinationRoom != null; }
    }
}
