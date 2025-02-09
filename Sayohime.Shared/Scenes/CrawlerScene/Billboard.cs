using Sayohime.Main;
using Sayohime.SceneObjects.Maps;
using Sayohime.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    public class Billboard
    {
        public VertexPositionTexture[] Quad { get; set; }
        public Texture2D Texture { get; set; }
        public WallShader Shader { get; set; }

		private float height = -2;
        private static readonly short[] INDICES = new short[] { 0, 2, 1, 2, 0, 3 };

        private GraphicsDevice graphicsDevice = CrossPlatformGame.GameInstance.GraphicsDevice;

        private CrawlerScene parentScene;

        public Billboard(CrawlerScene mapScene, Floor iFloor, Texture2D sprite, float sizeWidth, float sizeHeight, float heightOffset = 0)
        {
            parentScene = mapScene;

            height = 4.0f - sizeHeight - heightOffset;

            Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 320 / 200.0f, 0.7f, 10000.0f));
            Shader.WallTexture = Texture = sprite;

            float startU = 0.0f;
            float startV = 0.0f;
            float endU = 1.0f;
            float endV = 1.0f;
            Vector3[] VERTICES = new Vector3[]
            {
                    new Vector3(-sizeWidth / 2, 0, 0),
                    new Vector3(-sizeWidth / 2, sizeHeight, 0),
                    new Vector3(sizeWidth / 2, sizeHeight, 0),
                    new Vector3(sizeWidth / 2, 0, 0)
            };
            VertexPositionTexture[] quad = new VertexPositionTexture[4];
            quad[0] = new VertexPositionTexture(VERTICES[0], new Vector2(startU, startV));
            quad[1] = new VertexPositionTexture(VERTICES[1], new Vector2(startU, endV));
            quad[2] = new VertexPositionTexture(VERTICES[2], new Vector2(endU, endV));
            quad[3] = new VertexPositionTexture(VERTICES[3], new Vector2(endU, startV));
            Quad = quad;
        }

        public float Brightness(float x) { return Math.Min(1.0f, Math.Max(x / 4.0f, parentScene.Floor.AmbientLight)); }

        public void Draw(Matrix viewMatrix, float x, float z, float rotation, float brightness)
        {
            Shader.World = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(new Vector3(x, height, z));
            Shader.View = viewMatrix;
            Shader.Brightness = new Vector4(brightness);

            foreach (EffectPass pass in Shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Quad, 0, 4, INDICES, 0, 2);
            }
        }
    }
}
