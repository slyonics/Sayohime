using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Shaders;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sayohime.Main;

namespace Sayohime.SceneObjects.Maps
{
    public enum TerrainType
    {
        None,

        Shore,
        Grassland,
        Forest,
        Mountain
    }

    public class Tile
    {
        private class TileSprite
        {
            public Rectangle source;
            public Texture2D atlas;
            public Color color = Color.White;
            public int height;
            public Vector2 offset;

            public Rectangle[] anims;
            public int animationTime;
            public int animFrame;
            public int animTimeLeft;
        }

        private TileMap parentMap;
        private Vector2 position;
        private Vector2 center;
        private List<TileSprite> backgroundSprites = new List<TileSprite>();
        private Dictionary<int, List<TileSprite>> entitySprites = new Dictionary<int, List<TileSprite>>();

        private List<Tile> neighborList = new List<Tile>();
		public Tile visibilityNeighbor;

		public TerrainType TerrainType { get; private set; }
        public Chunk ParentChunk { get ; private set; }

		private bool wall;
		private bool roof;
		private bool blockSight;
        private bool obscured;
        private float visibilityInterval;

        public Tile(TileMap iTileMap, Chunk iChunk, int iTileX, int iTileY)
        {
            parentMap = iTileMap;
			ParentChunk = iChunk;
            TileX = iTileX;
            TileY = iTileY;

            position = new Vector2(TileX * TileMap.TILE_SIZE, TileY * TileMap.TILE_SIZE);
            center = position + new Vector2(TileMap.TILE_SIZE / 2, TileMap.TILE_SIZE / 2);
        }

        public void Update(GameTime gameTime)
        {
            // TODO: synchronize animation frames across all tiles

            foreach (TileSprite backgroundSprite in backgroundSprites)
            {
                if (backgroundSprite.anims != null)
                {
                    backgroundSprite.animTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                    while (backgroundSprite.animTimeLeft < 0)
                    {
                        backgroundSprite.animTimeLeft += backgroundSprite.animationTime;
                        backgroundSprite.animFrame++;
                        if (backgroundSprite.animFrame >= backgroundSprite.anims.Length) backgroundSprite.animFrame = 0;
                        backgroundSprite.source = backgroundSprite.anims[backgroundSprite.animFrame];
                    }
                }
            }

            UpdateVisibility(gameTime);
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            float depth = 0.9f;
            foreach (TileSprite backgroundSprite in backgroundSprites)
            {
                //spriteBatch.Draw(backgroundSprite.atlas, position + backgroundSprite.offset - camera.Position - new Vector2(camera.CenteringOffsetX, camera.CenteringOffsetY), backgroundSprite.source, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);

                backgroundSprite.color.B = Brightness;
                
                spriteBatch.Draw(backgroundSprite.atlas, position + backgroundSprite.offset - camera.Position - new Vector2(camera.CenteringOffsetX, camera.CenteringOffsetY), backgroundSprite.source, backgroundSprite.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                depth -= 0.001f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (KeyValuePair<int, List<TileSprite>> tileSprites in entitySprites)
            {
                float depth = camera.GetDepth(position.Y + TileMap.TILE_SIZE / 2 + tileSprites.Key * TileMap.TILE_SIZE);
                if (depth <= 0) depth = 0.0f;

                foreach (TileSprite tileSprite in tileSprites.Value)
                {
					//spriteBatch.Draw(tileSprite.atlas, position + tileSprite.offset, tileSprite.source, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);

					tileSprite.color.B = Brightness;

					spriteBatch.Draw(tileSprite.atlas, position + tileSprite.offset, tileSprite.source, tileSprite.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                    depth -= 0.0001f;
                }
            }
        }

        public void ApplyTileLayer(TileInstance tile, TilesetDefinition tileset, LayerInstance layer, Rectangle source, Texture2D atlas)
        {
            bool entityTile = false;
            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas
            };

            var customData = tileset.CustomData.FirstOrDefault(x => x.TileId == tile.T);
            if (customData != null)
            {
                foreach (string line in customData.Data.Split('\n'))
                {
					if (customData.Data.Contains("Height"))
					{
						ApplyEntityTile(tile, layer, source, atlas, int.Parse(customData.Data.Split(' ').Last()), tileSprite.color);
						return;
					}
					else
                    {
                        switch (line)
                        {
                            case "Encounter":
                                Encounter = true;
                                break;

                            case "Blocked":
                                Blocked = true;
                                break;

                                case "Savepoint":
                                    Savepoint = true;
                                    int[] tokens = new int[4] { 0, 1, 2, 1 };
                                    tileSprite.animationTime = 200;
                                    tileSprite.anims = new Rectangle[tokens.Length];
                                    for (int i = 0; i < tokens.Length; i++)
                                    {
                                        tileSprite.anims[i] = new Rectangle(source.X + source.Width * tokens[i], source.Y, source.Width, source.Height);
                                    }

                                    break;
                        }
                    }
                }
            }

            var terrain = tileset.EnumTags.FirstOrDefault(x => x.TileIds.Contains(tile.T));
            if (terrain != null)
            {
                int height = -1;
                switch (terrain.EnumValueId)
                {
					case "SnowForest":
					case "Forest": blockSight = true; break;

					case "SnowMountain":
					case "Mountain": blockSight = true; Blocked = true; break;

					case "Shore":
                        Blocked = true;

                        int[] tokens = new int[4] { 0, 7, 14, 7 };
                        tileSprite.animationTime = 700;
                        tileSprite.anims = new Rectangle[tokens.Length];
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            tileSprite.anims[i] = new Rectangle(source.X + source.Width * tokens[i], source.Y, source.Width, source.Height);
                        }
                        break;

                    case "Floor": break;
                    case "Wall": Blocked = true; blockSight = true; wall = true; break;
                    case "Roof": Blocked = true; blockSight = true; roof = true; break;

					case "ShortObstacle": Blocked = true; break;
					case "Obstacle": Blocked = true; blockSight = true; break;
				}

                if (Enum.IsDefined(typeof(TerrainType), terrain.EnumValueId))
                {
                    TerrainType = (TerrainType)Enum.Parse(typeof(TerrainType), terrain.EnumValueId);
                }

                if (height >= 0)
                {
                    ApplyEntityTile(tile, layer, source, atlas, height, tileSprite.color);
                    return;
                }
            }

            tileSprite.offset = new Vector2(layer.PxTotalOffsetX, layer.PxTotalOffsetY);

            if (tileSprite.anims != null)
            {
                tileSprite.animTimeLeft = tileSprite.animationTime;
            }

            backgroundSprites.Add(tileSprite);
        }

        public void ApplyEntityTile(TileInstance tile, LayerInstance layer, Rectangle source, Texture2D atlas, int height, Color color)
        {
            List<TileSprite> tileSprites;
            if (!entitySprites.TryGetValue(height, out tileSprites))
            {
                tileSprites = new List<TileSprite>();
                entitySprites.Add(height, tileSprites);
            }

            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas,
                height = height,
                color = color
            };

            tileSprite.offset = new Vector2(layer.PxTotalOffsetX, layer.PxTotalOffsetY);

            tileSprites.Add(tileSprite);
        }

        public void AssignNeighbors()
        {
            neighborList.Clear();

            if (TileX > 0) neighborList.Add(parentMap.GetTile(TileX - 1, TileY));
            if (TileY > 0) neighborList.Add(parentMap.GetTile(TileX, TileY - 1));
            if (TileY < parentMap.Rows - 1) neighborList.Add(parentMap.GetTile(TileX, TileY + 1));
            if (TileX < parentMap.Columns - 1) neighborList.Add(parentMap.GetTile(TileX + 1, TileY));

            /*
            if (TileX > 0 && TileY > 0) neighborList.Add(parentMap.GetTile(TileX - 1, TileY - 1));
            if (TileX > 0 && TileY < parentMap.Rows - 1) neighborList.Add(parentMap.GetTile(TileX - 1, TileY + 1));
            if (TileX < parentMap.Columns - 1 && TileY > 0) neighborList.Add(parentMap.GetTile(TileX + 1, TileY - 1));
            if (TileX < parentMap.Columns - 1 && TileY < parentMap.Rows - 1) neighborList.Add(parentMap.GetTile(TileX + 1, TileY + 1));
            */

            neighborList.RemoveAll(x => x == null);

			if (wall || roof)
			{
				visibilityNeighbor = BottommostWall();
				if (roof && visibilityNeighbor == null)
				{
					var downLeft = parentMap.GetTile(TileX - 1, TileY + 1);
					var left = parentMap.GetTile(TileX - 1, TileY);
					if (downLeft != null && downLeft.wall) visibilityNeighbor = downLeft.BottommostWall();
					else if (left != null && left.wall) visibilityNeighbor = left.BottommostWall();
					else
					{
						var downRight = parentMap.GetTile(TileX + 1, TileY + 1);
						var right = parentMap.GetTile(TileX + 1, TileY);
						if (downRight != null && downRight.wall) visibilityNeighbor = downRight.BottommostWall();
						else if (right != null && right.wall) visibilityNeighbor = right.BottommostWall();
					}
				}
			}
		}

		public void UpdateVisibility(GameTime gameTime)
		{
			bool correctedObscured = obscured;
			if (visibilityNeighbor != null)
			{
				correctedObscured = obscured && visibilityNeighbor.Obscured;
			}

			if (correctedObscured) visibilityInterval = Math.Max(0.0f, visibilityInterval - gameTime.ElapsedGameTime.Milliseconds / 200.0f);
			else visibilityInterval = Math.Min(1.0f, visibilityInterval + gameTime.ElapsedGameTime.Milliseconds / 200.0f);
		}

		public void UpdateVisibility()
		{
			bool correctedObscured = obscured;
			if (visibilityNeighbor != null)
			{
				correctedObscured = obscured && visibilityNeighbor.Obscured;
			}

			if (correctedObscured) visibilityInterval = 0.0f;
			else visibilityInterval = 1.0f;
		}

		public Tile BottommostWall()
		{
			var tileBelow = parentMap.GetTile(TileX, TileY + 1);
			if (tileBelow == null || !tileBelow.wall)
			{
				return wall ? this : null;
			}
			return tileBelow.BottommostWall();
		}

		public int TileX { get; private set; }
        public int TileY { get; private set; }
        public Vector2 Center { get => center; }
        public Vector2 Bottom { get => center + new Vector2(0, TileMap.TILE_SIZE / 2); }

        public List<Tile> NeighborList { get => neighborList; }

        public bool Encounter { get; private set; }
        public bool Savepoint { get; private set; }

        public bool Blocked { get; private set; }
        public List<Actor> Occupants { get; private set; } = new List<Actor>();

        public bool BlockSight { set => blockSight = value; get => blockSight; }
        public bool Obscured
        {
            set => obscured = value;
            get
            {
                if (entitySprites.Any(x => x.Key > 0))
                {
                    var belowTile = parentMap.GetTile(TileX, TileY + 1);
                    if (belowTile != null) return belowTile.Obscured;
                }

                return obscured;
            }
        }
        public byte Brightness { get => (byte)(128 * visibilityInterval); }
    }
}
