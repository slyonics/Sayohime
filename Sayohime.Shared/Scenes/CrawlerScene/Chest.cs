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
    public class Chest : IBillboard
    {
        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

        public Direction Direction { get; private set; }
        public MapRoom CurrentRoom { get; private set; }
        public MapRoom DestinationRoom { get; private set; }
        public float MoveInterval { get; set; }

        public string Item { get; set; }
        public string Name { get; set; } = "Chest";

        public Chest(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;


            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Item": Item = field.Value; break;
                    case "Name": Name = field.Value; break;
                }
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            CurrentRoom.Chest = this;
            CurrentRoom.PreEnterScript = CurrentRoom.InteractScript = new string[] { "GiveItem " + Item, "SetFlag " + Name + " True", "RemoveChest " + Name };

            var texture = AssetCache.SPRITES[GameSprite.NPCs_Chest];
            float sizeX = texture.Width / 24.0f * 12;
            float sizeY = texture.Height / 24.0f * 12;
            Billboard = new Billboard(crawlerScene, iFloor, texture, sizeX, sizeY);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            float x = 10 * CurrentRoom.RoomX;
            float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);
            float brightness = CurrentRoom.Brightness(CurrentRoom.brightnessLevel);

            Billboard.Draw(viewMatrix, x, z, cameraX, brightness);
        }

        public Vector3 Position
        {
            get
            {
                float x = 10 * CurrentRoom.RoomX;
                float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);
                return new Vector3(x, 0, z);
            }
        }

        public void Destroy()
        {
            crawlerScene.ChestList.Remove(this);
            CurrentRoom.Chest = null;
            CurrentRoom.PreEnterScript = null;
            CurrentRoom.InteractScript = null;
        }
    }
}
