using Sayohime.Main;
using Sayohime.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayohime.SceneObjects.Maps
{
	public class TileMap : Entity
    {
        private class Tileset
        {
            public Tileset(TilesetDefinition tilesetDefinition)
            {
                TilesetDefinition = tilesetDefinition;
                string tilesetName = tilesetDefinition.RelPath.Replace("../Sprites/", "").Replace(".png", "").Replace('/', '_');
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
            }

            public TilesetDefinition TilesetDefinition { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

		public const int TILE_SIZE = 16;

		private Scene mapScene;

        private GameMap gameMap;

        private Definitions definitions;
        private Dictionary<long, Tileset> tilesets = new Dictionary<long, Tileset>();
		private List<Chunk> chunkList = new List<Chunk>();
		private Tile[,] tiles;

        private List<Tile> visibleTiles = new List<Tile>();
        private bool revealAll;

        public string Name { get => gameMap.ToString(); }

		public int Columns { get; set; }
		public int Rows { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public bool RevealAll { set => revealAll = value; get => revealAll; }

		public TileMap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            mapScene = iScene;
            gameMap = iGameMap;
            PriorityLevel = PriorityLevel.CutsceneLevel;

            LdtkJson ldtkJson = AssetCache.MAPS[gameMap];

            Rectangle worldArea = new Rectangle();
            foreach (var level in ldtkJson.Levels)
            {
                var chunk = new Chunk(level, this);
                chunkList.Add(chunk);
                worldArea = Rectangle.Union(worldArea, chunk.TileBounds);
            }

            Columns = worldArea.Width;
            Rows = worldArea.Height;
            Width = TILE_SIZE * Columns;
            Height = TILE_SIZE * Rows;

            tiles = new Tile[Columns, Rows];
			definitions = ldtkJson.Defs;
			foreach (TilesetDefinition tilesetDefinition in definitions.Tilesets)
            {
                tilesets.Add(tilesetDefinition.Uid, new Tileset(tilesetDefinition));
            }
        }

        public Chunk FindHostChunk(string spawnName)
        {
            return chunkList.First(x => x.Level.LayerInstances.Any(
                                   y => y.Type == "Entities" && y.EntityInstances.Any(
                                   z => z.Identifier == "Travel" && z.FieldInstances.Any(
                                   w => w.Identifier == "Name" && (string)w.Value == spawnName))));
        }

        public Chunk FindHostChunk(int tileX, int tileY)
        {
            int px = tileX * TILE_SIZE;
            int py = tileY * TILE_SIZE;

            return chunkList.First(x => x.Level.LayerInstances.Any(
                                   y => y.PxTotalOffsetX <= px && y.PxTotalOffsetX + y.CWid <= px &&
                                        y.PxTotalOffsetY <= py && y.PxTotalOffsetY + y.CHei <= py));
        }

        public List<Chunk> FindUnloadedNeighborChunks(Rectangle neighborhood)
        {

            //var neighborList = ((MapScene)mapScene).CurrentChunk.Level.Neighbours;
            return null; // chunkList.FindAll(x => !x.Loaded && neighborList.Any(y => x.Level.Iid == y.LevelIid) && x.TileBounds.Intersects(neighborhood));
		}

		public void LoadLayers(LayerInstance[] layers, Chunk chunk)
        {
            foreach (LayerInstance layer in layers.Reverse())
            {
                if (layer.Type != "Entities" && layer.Type != "AutoLayer") LoadTileLayer(layer, chunk);
            }
        }

        private void LoadTileLayer(LayerInstance layer, Chunk chunk)
        {
            var tileset = definitions.Tilesets.First(x => x.Uid == layer.TilesetDefUid);

            foreach (var tile in layer.GridTiles)
            {
                int x = (int)(tile.Px[0] / TILE_SIZE) + chunk.TileBounds.X;
                int y = (int)(tile.Px[1] / TILE_SIZE) + chunk.TileBounds.Y;
				tiles[x, y].ApplyTileLayer(tile, tileset, layer, new Rectangle((int)tile.Src[0], (int)tile.Src[1], TILE_SIZE, TILE_SIZE), tilesets[layer.TilesetDefUid.Value].SpriteAtlas);
            }

            foreach (var tile in layer.AutoLayerTiles)
            {
				int x = (int)(tile.Px[0] / TILE_SIZE) + chunk.TileBounds.X;
				int y = (int)(tile.Px[1] / TILE_SIZE) + chunk.TileBounds.Y;
				tiles[x, y].ApplyTileLayer(tile, tileset, layer, new Rectangle((int)tile.Src[0], (int)tile.Src[1], TILE_SIZE, TILE_SIZE), tilesets[layer.TilesetDefUid.Value].SpriteAtlas);
            }

            // tiles[x, y].ApplyEntityTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas, height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			int startTileX = Math.Max((mapScene.Camera.View.Left / TILE_SIZE) - 3, 0);
			int startTileY = Math.Max((mapScene.Camera.View.Top / TILE_SIZE) - 3, 0);
			int endTileX = Math.Min((mapScene.Camera.View.Right / TILE_SIZE) + 2, Columns - 1);
			int endTileY = Math.Min((mapScene.Camera.View.Bottom / TILE_SIZE) + 2, Rows - 1);

			for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y]?.Update(gameTime);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((camera.View.Left / TILE_SIZE) - 1, 0);
            int startTileY = Math.Max((camera.View.Top / TILE_SIZE) - 1, 0);
            int endTileX = Math.Min((camera.View.Right / TILE_SIZE), Columns - 1);
            int endTileY = Math.Min((camera.View.Bottom / TILE_SIZE), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y]?.DrawBackground(spriteBatch, camera);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((camera.View.Left / TILE_SIZE) - 1, 0);
            int startTileY = Math.Max((camera.View.Top / TILE_SIZE) - 1, 0);
            int endTileX = Math.Min((camera.View.Right / TILE_SIZE), Columns - 1);
            int endTileY = Math.Min((camera.View.Bottom / TILE_SIZE), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y]?.Draw(spriteBatch, camera);
                }
            }
        }

        public void InitTile(Chunk chunk, int tileX, int tileY)
        {
            tiles[tileX, tileY] = new Tile(this, chunk, tileX, tileY);
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public Tile GetTile(Vector2 position)
        {
            int tileX = (int)(position.X / TILE_SIZE);
            int tileY = (int)(position.Y / TILE_SIZE);

            if (tileX < 0 || tileY < 0 || tileX >= Columns || tileY >= Rows) return null;

            return tiles[tileX, tileY];
        }

        public List<Tile> GetPath(Tile startTile, Tile endTile, int searchLimit)
        {
            List<Tile> processedNodes = new List<Tile>();
            List<Tile> unprocessedNodes = new List<Tile> { startTile };
            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
            Dictionary<Tile, int> currentDistance = new Dictionary<Tile, int>();
            Dictionary<Tile, int> predictedDistance = new Dictionary<Tile, int>();

            int searchCount = 0;

            currentDistance.Add(startTile, 0);
            predictedDistance.Add(startTile, Math.Abs(startTile.TileX - endTile.TileX) + Math.Abs(startTile.TileY - endTile.TileY));

            while (unprocessedNodes.Count > 0 && searchCount < searchLimit)
            {
                searchCount++;

                Tile current = (from p in unprocessedNodes orderby predictedDistance[p] ascending select p).First();
                if (current == endTile) return ReconstructPath(cameFrom, endTile);

                unprocessedNodes.Remove(current);
                processedNodes.Add(current);

                foreach (Tile neighbor in current.NeighborList)
                {
                    if (!neighbor.Blocked && neighbor.Occupants.Count == 0)
                    {
                        int tempCurrentDistance = currentDistance[current] + Math.Abs(current.TileX - neighbor.TileX) + Math.Abs(current.TileY - neighbor.TileY);
                        if (currentDistance.ContainsKey(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                        if (!processedNodes.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                        {
                            if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                            else cameFrom.Add(neighbor, current);

                            currentDistance[neighbor] = tempCurrentDistance;
                            predictedDistance[neighbor] = currentDistance[neighbor] + Math.Abs(current.TileX - neighbor.TileX) + Math.Abs(current.TileY - neighbor.TileY);

							if (!unprocessedNodes.Contains(neighbor)) unprocessedNodes.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<Tile> { current };
            }

            List<Tile> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public void ClearFieldOfView()
        {
			int startTileX = Math.Max((mapScene.Camera.View.Left / TILE_SIZE) - 3, 0);
			int startTileY = Math.Max((mapScene.Camera.View.Top / TILE_SIZE) - 3, 0);
			int endTileX = Math.Min((mapScene.Camera.View.Right / TILE_SIZE) + 2, Columns - 1);
			int endTileY = Math.Min((mapScene.Camera.View.Bottom / TILE_SIZE) + 2, Rows - 1);

			for (int y = startTileY; y <= endTileY; y++)
			{
				for (int x = startTileX; x <= endTileX; x++)
				{
                    var tile = GetTile(x, y);
                    if (tile != null) tile.Obscured = !revealAll;
                }
            }
        }

        public void CalculateFieldOfView(Tile sourceTile, float viewRadius)
        {
            if (revealAll) return;

            sourceTile.Obscured = false;
            for (int txidx = 0; txidx < OctantTransform.s_octantTransform.Length; txidx++)
            {
                CastLight(sourceTile, viewRadius, 1, 1.0f, 0.0f, OctantTransform.s_octantTransform[txidx]);
            }
        }

        public void UpdateVisibility()
        {
			int startTileX = Math.Max((mapScene.Camera.View.Left / TILE_SIZE) - 3, 0);
			int startTileY = Math.Max((mapScene.Camera.View.Top / TILE_SIZE) - 3, 0);
			int endTileX = Math.Min((mapScene.Camera.View.Right / TILE_SIZE) + 2, Columns - 1);
			int endTileY = Math.Min((mapScene.Camera.View.Bottom / TILE_SIZE) + 2, Rows - 1);

			for (int y = startTileY; y <= endTileY; y++)
			{
				for (int x = startTileX; x <= endTileX; x++)
				{
                    tiles[x, y]?.UpdateVisibility();
                }
            }
        }

        private void CastLight(Tile sourceTile, float viewRadius, int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            float viewRadiusSquared = viewRadius * viewRadius;
            int viewCeiling = (int)Math.Ceiling(viewRadius);
            bool prevWasBlocked = false;
            float savedRightSlope = -1;

            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    int gridX = sourceTile.TileX + xc * txfrm.xx + yc * txfrm.xy;
                    int gridY = sourceTile.TileY + xc * txfrm.yx + yc * txfrm.yy;                    
                    if (gridX < 0 || gridX >= Columns || gridY < 0 || gridY >= Rows) continue;

                    var gridTile = tiles[gridX, gridY];
                    if (gridTile == null) continue;

                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);
                    if (rightBlockSlope > leftViewSlope) continue; 
                    else if (leftBlockSlope < rightViewSlope) break;

                    float distanceSquared = xc * xc + yc * yc;
                    if (distanceSquared <= viewRadiusSquared)
                    {
                        gridTile.Obscured = false;
                        visibleTiles.Add(gridTile);
                    }

                    bool curBlocked = gridTile.BlockSight;
                    if (prevWasBlocked)
                    {
                        if (curBlocked) savedRightSlope = rightBlockSlope;
                        else
                        {
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            if (leftBlockSlope <= leftViewSlope)
                            {
                                CastLight(sourceTile, viewRadius, currentCol + 1, leftViewSlope, leftBlockSlope, txfrm);
                            }

                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                if (prevWasBlocked) break;
            }
        }
    }
}
