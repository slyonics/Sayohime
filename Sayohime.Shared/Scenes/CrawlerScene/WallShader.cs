using Sayohime.Main;
using Sayohime.SceneObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.MapScene
{
    public class WallShader : Shader
    {

        public WallShader(Matrix projectionMatrix)
            : base(AssetCache.SHADERS[GameShader.Wall].Clone())
        {
            shaderEffect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {

        }

        public Matrix World
        {
            set
            {
                shaderEffect.Parameters["World"].SetValue(value);
                shaderEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(value)));
            }
        }

        public Matrix View { set => shaderEffect.Parameters["View"].SetValue(value); }
        public Texture2D WallTexture { set => shaderEffect.Parameters["WallTexture"].SetValue(value); }
    }
}
