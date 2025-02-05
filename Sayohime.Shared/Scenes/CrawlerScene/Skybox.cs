using Sayohime.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sayohime.Scenes.CrawlerScene.MapRoom;

namespace Sayohime.Scenes.CrawlerScene
{
    public class Skybox
    {
        private const float WALL_HALF_LENGTH = 5000;
        private static readonly short[] SKYBOX_INDICES = new short[] { 0, 2, 1, 2, 0, 3 };
        private static readonly Dictionary<Direction, Vector3[]> VERTICES = new Dictionary<Direction, Vector3[]>()
        {   {
                Direction.North, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH) }
            }, {
                Direction.West, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH) }
            }, {
                Direction.East, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH) }
            }, {
                Direction.South, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH) }
            }, {
                Direction.Up, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH, WALL_HALF_LENGTH) }
            }, {
                Direction.Down, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH, -WALL_HALF_LENGTH) }
        } };

        private List<MapRoom.RoomWall> wallList = new List<MapRoom.RoomWall>();


        public Skybox(Texture2D skyboxTexture)
        {
            for (int i = 0; i < 6; i++)
            {
                MapRoom.RoomWall roomWall = new RoomWall((Direction)i, skyboxTexture, VERTICES, 0.0f, 0.0f, 1.0f, 1.0f)
                {
                    Lighting = new Vector4[] { new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f) },
                    Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f))
                    {
                        WallTexture = skyboxTexture,
                        Brightness = new Vector4(1.0f)
                    }
                };

                wallList.Add(roomWall);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, Vector3 offset)
        {
            foreach (MapRoom.RoomWall wall in wallList)
            {
                wall.Shader.World = Matrix.CreateTranslation(offset);
                wall.Shader.View = viewMatrix;
                foreach (EffectPass pass in wall.Shader.Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, wall.Quad, 0, 4, SKYBOX_INDICES, 0, 2);
                }
            }
        }
    }
}
