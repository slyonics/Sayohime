using ldtk;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sayohime.SceneObjects.Maps
{
    public class Chunk
    {
        public Vector2 OffsetPx { get; set; }
        public Rectangle TileBounds { get; set; }

        public Level Level { get; set; }

        public bool Loaded { get; private set; } = false;

        private TileMap tileMap;

        public Chunk(Level iLevel, TileMap iTileMap)
        {
            Level = iLevel;
            tileMap = iTileMap;
            int tileSize = TileMap.TILE_SIZE;

            OffsetPx = new Vector2(iLevel.WorldX, iLevel.WorldY);
            TileBounds = new Rectangle((int)(iLevel.WorldX / tileSize), (int)(iLevel.WorldY / tileSize), (int)(iLevel.PxWid / tileSize) + 1, (int)(iLevel.PxHei / tileSize) + 1);
        }

        public void LoadChunk()
        {
            if (Loaded) return;
            Loaded = true;

            for (int y = TileBounds.Top; y < TileBounds.Bottom - 1; y++)
            {
                for (int x = TileBounds.Left; x < TileBounds.Right - 1; x++)
                {
                    tileMap.InitTile(this, x, y);
                }
            }

            tileMap.LoadLayers(Level.LayerInstances, this);

			for (int y = TileBounds.Top - 1; y < TileBounds.Bottom + 1; y++)
			{
				for (int x = TileBounds.Left - 1; x < TileBounds.Right + 1; x++)
				{
                    tileMap.GetTile(x, y)?.AssignNeighbors();
                }
            }
        }
    }
}
