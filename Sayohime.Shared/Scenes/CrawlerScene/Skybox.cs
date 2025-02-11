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
using Sayohime.Main;

namespace Sayohime.Scenes.CrawlerScene
{
    public class Skybox
    {
        private const float SKYBOX_LENGTH = 5000;
        private static readonly short[] SKYBOX_INDICES = [ 0, 2, 1, 2, 0, 3 ];
        private static readonly Dictionary<Direction, Vector3[]> VERTICES = new Dictionary<Direction, Vector3[]>()
        {   
            {
                Direction.North, new Vector3[]
                {
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH)
                }
            },
            {
                Direction.West, new Vector3[]
                {
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH)
                }
            },
            {
                Direction.East, new Vector3[]
                {
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH)
                }
            },
            {
                Direction.South, new Vector3[]
                {
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH)
                }
            },
            {
                Direction.Up, new Vector3[]
                {
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, -SKYBOX_LENGTH, SKYBOX_LENGTH)
                }
            },
            {
                Direction.Down, new Vector3[]
                {
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH),
                    new (SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, SKYBOX_LENGTH),
                    new (-SKYBOX_LENGTH, SKYBOX_LENGTH, -SKYBOX_LENGTH)
                }
            } 
        };

        private List<RoomWall> wallList = new List<RoomWall>();

        public Skybox(string skyboxName)
        {
            var directionList = Enum.GetValuesAsUnderlyingType(typeof(Direction));
            foreach (Direction direction in directionList)
            {
				if (!Enum.TryParse($"Background_{skyboxName}_{skyboxName}{direction}", out GameSprite sprite)) continue;

                Texture2D skyboxTexture = AssetCache.SPRITES[sprite];
                RoomWall roomWall = new RoomWall(direction, skyboxTexture, VERTICES, 0.0f, 0.0f, 1.0f, 1.0f)
                {
					Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f))
					{
						WallTexture = skyboxTexture
					}
				};

				wallList.Add(roomWall);
			}
		}

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, Vector3 offset)
        {
			graphicsDevice.DepthStencilState = DepthStencilState.None;

			foreach (RoomWall wall in wallList)
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
