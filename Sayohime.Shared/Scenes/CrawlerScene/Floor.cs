using Sayohime.Models;
using Sayohime.Main;
using Sayohime.SceneObjects.Maps;
using Sayohime.SceneObjects.Shaders;
using Sayohime.SceneObjects.Widgets;
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
    public class Spawn
    {
        public int RoomX;
        public int RoomY;
        public Direction Direction;
    }

    public class Floor
    {
        private class Tileset
        {
            public Tileset(TilesetDefinition tilesetDefinition)
            {
                TilesetDefinition = tilesetDefinition;
                string tilesetName = tilesetDefinition.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
            }

            public TilesetDefinition TilesetDefinition { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

        public const int MINI_CELL_SIZE = 24;

        private Texture2D minimapPlayer = null;
        private static readonly Rectangle[] minimapSource = [ new Rectangle(0, 0, MINI_CELL_SIZE, MINI_CELL_SIZE), new Rectangle(MINI_CELL_SIZE * 1, 0, MINI_CELL_SIZE, MINI_CELL_SIZE), new Rectangle(MINI_CELL_SIZE * 2, 0, MINI_CELL_SIZE, MINI_CELL_SIZE), new Rectangle(MINI_CELL_SIZE * 3, 0, MINI_CELL_SIZE, MINI_CELL_SIZE) ];
		

		public int TileSize { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        public GameMap GameMap { get; private set; }
        public Definitions Definitions { get; set; }
        public Level Level { get; set; }

        private Dictionary<long, Tileset> tilesets = new Dictionary<long, Tileset>();

        private MapRoom[,] mapRooms = new MapRoom[8, 8];

        private List<MapRoom> visibleTiles = new List<MapRoom>();
        private bool revealAll;

        CrawlerScene parentScene;

        public string LocationName { get; private set; } = "Location";
        public float AmbientLight { get; private set; } = 1;


        public int MinimapStartX { get; set; } = 0;
        public int MinimapStartY { get; set; } = 0;

        public Skybox Skybox { get; private set; }

        public Dictionary<string, Spawn> Spawns { get; private set; } = new Dictionary<string, Spawn>();

        public Floor(CrawlerScene crawlerScene, GameMap gameMap)
        {
            parentScene = crawlerScene;
            minimapPlayer = AssetCache.SPRITES[GameSprite.YouAreHere];

			LoadMap(gameMap);
        }

        public void LoadMap(GameMap iGameMap, string spawnName = "Default")
        {
            GameMap = iGameMap;

            LdtkJson ldtkJson = AssetCache.MAPS[GameMap];
            Definitions = ldtkJson.Defs;
            Level = ldtkJson.Levels[0];
            foreach (TilesetDefinition tilesetDefinition in Definitions.Tilesets)
            {
                tilesets.Add(tilesetDefinition.Uid, new Tileset(tilesetDefinition));
            }

            TileSize = (int)Level.LayerInstances[0].GridSize;
            MapWidth = (int)Level.LayerInstances[0].CWid;
            MapHeight = (int)Level.LayerInstances[0].CHei;
            mapRooms = new MapRoom[MapWidth, MapHeight];


            foreach (LayerInstance layer in Level.LayerInstances.Reverse())
            {
                if (layer.Type == "Entities") continue;

                var tileset = Definitions.Tilesets.First(x => x.Uid == layer.TilesetDefUid);
                foreach (var tile in layer.GridTiles)
                {
                    int x = (int)(tile.Px[0]) / TileSize;
                    int y = (int)(tile.Px[1]) / TileSize;
                    var room = mapRooms[x, y];

                    if (room == null)
                    {
                        room = mapRooms[x, y] = new MapRoom(parentScene, this, x, y);
                    }

                    room.ApplyTile(layer.Identifier, tileset, tile);
                }
            }


            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    mapRooms[x, y]?.BuildNeighbors();
                }
            }

            foreach (LayerInstance layer in Level.LayerInstances)
            {
                if (layer.Type == "Entities")
                {
                    foreach (var entity in layer.EntityInstances) ParseEntity(entity);
                }
            }

            AmbientLight = 0.6f;

            foreach (FieldInstance field in Level.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Music": if (!string.IsNullOrEmpty(field.Value)) Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), field.Value)); break;
                    case "LocationName": if (!string.IsNullOrEmpty(field.Value)) LocationName = (string)field.Value; else LocationName = Level.Identifier; break;
                    case "AmbientLight": AmbientLight = (float)field.Value; break;
                    case "Skybox": if (!string.IsNullOrEmpty(field.Value)) Skybox = new Skybox(field.Value); break;
                }
            }

            FinishMap();
        }

        private void ParseEntity(EntityInstance entity)
        {
            var property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "EnableIf");
            if (property != null && property.Value != null && !GameProfile.GetSaveData<bool>(property.Value)) return;

            property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
            if (property != null && property.Value != null && GameProfile.GetSaveData<bool>(property.Value)) return;

            switch (entity.Identifier)
            {
                case "Light":
                    {
                        int startX = (int)(entity.Px[0] / TileSize);
                        int startY = (int)(entity.Px[1] / TileSize);
                        int endX = (int)(entity.Width / TileSize);
                        int endY = (int)(entity.Height / TileSize);

                        int brightness = 4;
                        foreach (FieldInstance field in entity.FieldInstances)
                        {
                            if (field.Identifier == "Brightness") brightness = (int)field.Value;
                        }

                        Lighting(startX, startY, endX, endY, brightness);
                    }
                    break;

                case "Automatic":
                    {
                        int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
                        int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

                        mapRooms[startX, startY].Script = entity.FieldInstances.First(x => x.Identifier == "Script").Value.Split('\n');
                    }
                    break;

                case "Interactable":
                    {
                        int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
                        int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

                        mapRooms[startX, startY].InteractScript = entity.FieldInstances.First(x => x.Identifier == "Script").Value.Split('\n');
                    }
                    break;

                case "Foe":
                    {
                        Foe foe = new Foe(parentScene, this, entity);
                        parentScene.FoeList.Add(foe);
                    }
                    break;

                case "Chest":
                    {
                        string name = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "Name").Value;
                        if (GameProfile.GetSaveData<bool>(name)) return;

                        Chest chest = new Chest(parentScene, this, entity);
                        parentScene.ChestList.Add(chest);
                    }
                    break;

                case "Npc":
                    {
                        Npc npc = new Npc(parentScene, this, entity);
                        parentScene.NpcList.Add(npc);
                    }
                    break;

                case "Spawn":
                    {
                        string name = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "Name").Value;
                        int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
                        int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);
                        Direction direction = (Direction)Enum.Parse(typeof(Direction), entity.FieldInstances.FirstOrDefault(x => x.Identifier == "Direction").Value);

                        Spawn spawn = new Spawn()
                        {
                            RoomX = startX,
                            RoomY = startY,
                            Direction = direction
                        };

                        Spawns.Add(name, spawn);
                    }
                    break;
            }
        }

        public void FinishMap()
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    mapRooms[x, y]?.SetVertices(x, y);
                }
            }
        }


        void Lighting(int startX, int startY, int width, int height, int brightness)
        {
            int fullBrightness = 0;
            int attenuatedBrightness = brightness;

            MapRoom originRoom = mapRooms[startX + width / 2, startY + height / 2];
            List<MapRoom> visitedRooms = new List<MapRoom>();
            List<MapRoom> roomsToVisit = new List<MapRoom>() { originRoom };
            List<MapRoom> nextRooms = new List<MapRoom>();
            while (attenuatedBrightness > 0)
            {
                visitedRooms.AddRange(roomsToVisit);
                foreach (MapRoom room in roomsToVisit)
                {
                    room.brightnessLevel += attenuatedBrightness;
                    nextRooms.AddRange(room.Neighbors.FindAll(x => !x.Occluding && !visitedRooms.Contains(x) && !nextRooms.Contains(x)));
                }

                roomsToVisit = nextRooms;
                nextRooms = new List<MapRoom>();

                if (fullBrightness > 0) fullBrightness--;
                else attenuatedBrightness--;
            }

            for (int x = 0; x < mapRooms.GetLength(0); x++)
            {
                for (int y = 0; y < mapRooms.GetLength(1); y++)
                {
                    MapRoom mapRoom = mapRooms[x, y];
                    mapRoom?.BlendLighting();
                }
            }
        }


        public void DrawMap(GraphicsDevice graphicsDevice, Panel mapWindow, Matrix viewMatrix, Vector3 cameraPos)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            for (int x = 0; x < mapRooms.GetLength(0); x++)
            {
                for (int y = 0; y < mapRooms.GetLength(1); y++)
                {
                    mapRooms[x, y]?.Draw(viewMatrix);
                }
            }
        }

        public void DrawMiniMap(SpriteBatch spriteBatch, Rectangle bounds, Color color, float depth, int roomX, int roomY, Direction direction)
        {
            MinimapStartX = Math.Max(0, roomX - 5);
            int endX = MinimapStartX + 11;
            if (endX > mapRooms.GetLength(0) - 1)
            {
                endX = mapRooms.GetLength(0) - 1;
                MinimapStartX = Math.Max(0, endX - 11);
            }

            MinimapStartY = Math.Max(0, roomY - 5);
            int endY = MinimapStartY + 11;
            if (endY > mapRooms.GetLength(1) - 1)
            {
                endY = mapRooms.GetLength(1) - 1;
                MinimapStartY = Math.Max(0, endY - 11);
            }

            Vector2 offset = new Vector2(bounds.X, bounds.Y);
            for (int x = MinimapStartX; x < endX; x++)
            {
                for (int y = MinimapStartY; y < endY; y++)
                {
                    MapRoom mapRoom = mapRooms[x, y];
                    spriteBatch.Draw(minimapPlayer, offset, new Rectangle(0, 0, MINI_CELL_SIZE, MINI_CELL_SIZE), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.001f);
                    mapRoom?.DrawMinimap(spriteBatch, offset, depth - 0.002f);

                    offset.Y += MINI_CELL_SIZE;
                }

                offset.Y = bounds.Y;
                offset.X += MINI_CELL_SIZE;
            }

            spriteBatch.Draw(minimapPlayer, new Vector2((roomX - MinimapStartX) * MINI_CELL_SIZE, (roomY - MinimapStartY) * MINI_CELL_SIZE) + new Vector2(bounds.X, bounds.Y), minimapSource[(int)direction], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.004f);
        }

        public List<MapRoom> GetPath(MapRoom startTile, MapRoom endTile)
        {
            List<MapRoom> processedTiles = new List<MapRoom>();
            List<MapRoom> unprocessedTiles = new List<MapRoom> { startTile };
            Dictionary<MapRoom, MapRoom> cameFrom = new Dictionary<MapRoom, MapRoom>();
            Dictionary<MapRoom, int> currentDistance = new Dictionary<MapRoom, int>();
            Dictionary<MapRoom, int> predictedDistance = new Dictionary<MapRoom, int>();

            currentDistance.Add(startTile, 0);
            predictedDistance.Add(startTile, Distance(startTile, endTile));

            while (unprocessedTiles.Count > 0)
            {
                // get the node with the lowest estimated cost to finish
                MapRoom current = (from p in unprocessedTiles orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current.RoomX == endTile.RoomX && current.RoomY == endTile.RoomY)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endTile);
                }

                // move current node from open to closed
                unprocessedTiles.Remove(current);
                processedTiles.Add(current);

                foreach (MapRoom neighbor in current.Neighbors)
                {
                    int tempCurrentDistance = currentDistance[current] + Distance(neighbor, endTile);

                    // if we already know a faster way to this neighbor, use that route and ignore this one
                    if (processedTiles.Contains(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                    // if we don't know a route to this neighbor, or if this is faster, store this route
                    if (!processedTiles.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                    {
                        if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                        else cameFrom.Add(neighbor, current);

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] = currentDistance[neighbor] + Distance(neighbor, endTile);

                        if (!unprocessedTiles.Contains(neighbor)) unprocessedTiles.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private int Distance(MapRoom room1, MapRoom room2)
        {
            return Math.Abs(room1.RoomX - room2.RoomX) + Math.Abs(room1.RoomY - room2.RoomY);
        }

        private static List<MapRoom> ReconstructPath(Dictionary<MapRoom, MapRoom> cameFrom, MapRoom current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<MapRoom> { current };
            }

            List<MapRoom> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public MapRoom GetRoom(int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapRooms.GetLength(0) || y >= mapRooms.GetLength(1)) return null;
            return mapRooms[x, y];
        }


        public void ClearFieldOfView()
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    mapRooms[x, y].Obscured = true;
                }
            }
        }

        public void CalculateFieldOfView(MapRoom sourceTile, float viewRadius)
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
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    // mapRooms[x, y].UpdateVisibility();
                }
            }
        }

        /// <summary>
        /// Recursively casts light into cells.  Operates on a single octant.
        /// Adapted from source code at http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting by Fadden
        /// </summary>
        /// <param name="sourceTile">The player's current tile after moving.</param>
        /// <param name="viewRadius">The view radius; can be a fractional value.</param>
        /// <param name="startColumn">Current column; pass 1 as initial value.</param>
        /// <param name="leftViewSlope">Slope of the left (upper) view edge; pass 1.0 as
        ///   the initial value.</param>
        /// <param name="rightViewSlope">Slope of the right (lower) view edge; pass 0.0 as
        ///   the initial value.</param>
        /// <param name="txfrm">Coordinate multipliers for the octant transform.</param>
        ///
        /// Maximum recursion depth is (Ceiling(viewRadius)).
        private void CastLight(MapRoom sourceTile, float viewRadius, int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            // Used for distance test.
            float viewRadiusSquared = viewRadius * viewRadius;
            int viewCeiling = (int)Math.Ceiling(viewRadius);

            // Set true if the previous cell we encountered was blocked.
            bool prevWasBlocked = false;

            // As an optimization, when scanning past a block we keep track of the
            // rightmost corner (bottom-right) of the last one seen.  If the next cell
            // is empty, we can use this instead of having to compute the top-right corner
            // of the empty cell.
            float savedRightSlope = -1;

            // Outer loop: walk across each column, stopping when we reach the visibility limit.
            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;

                // Inner loop: walk down the current column.  We start at the top, where X==Y.
                //
                // TODO: we waste time walking across the entire column when the view area
                //   is narrow.  Experiment with computing the possible range of cells from
                //   the slopes, and iterate over that instead.
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    // Translate local coordinates to grid coordinates.  For the various octants
                    // we need to invert one or both values, or swap X for Y.
                    int gridX = sourceTile.RoomX + xc * txfrm.xx + yc * txfrm.xy;
                    int gridY = sourceTile.RoomY + xc * txfrm.yx + yc * txfrm.yy;

                    // Range-check the values.  This lets us avoid the slope division for blocks
                    // that are outside the grid.
                    //
                    // Note that, while we will stop at a solid column of blocks, we do always
                    // start at the top of the column, which may be outside the grid if we're (say)
                    // checking the first octant while positioned at the north edge of the map.
                    if (gridX < 0 || gridX >= MapWidth || gridY < 0 || gridY >= MapHeight)
                    {
                        continue;
                    }

                    // Compute slopes to corners of current block.  We use the top-left and
                    // bottom-right corners.  If we were iterating through a quadrant, rather than
                    // an octant, we'd need to flip the corners we used when we hit the midpoint.
                    //
                    // Note these values will be outside the view angles for the blocks at the
                    // ends -- left value > 1, right value < 0.
                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);

                    // Check to see if the block is outside our view area.  Note that we allow
                    // a "corner hit" to make the block visible.  Changing the tests to >= / <=
                    // will reduce the number of cells visible through a corner (from a 3-wide
                    // swath to a single diagonal line), and affect how far you can see past a block
                    // as you approach it.  This is mostly a matter of personal preference.
                    if (rightBlockSlope > leftViewSlope)
                    {
                        // Block is above the left edge of our view area; skip.
                        continue;
                    }
                    else if (leftBlockSlope < rightViewSlope)
                    {
                        // Block is below the right edge of our view area; we're done.
                        break;
                    }

                    var room = mapRooms[gridX, gridY];

                    // This cell is visible, given infinite vision range.  If it's also within
                    // our finite vision range, light it up.
                    //
                    // To avoid having a single lit cell poking out N/S/E/W, use a fractional
                    // viewRadius, e.g. 8.5.
                    //
                    // TODO: we're testing the middle of the cell for visibility.  If we tested
                    //  the bottom-left corner, we could say definitively that no part of the
                    //  cell is visible, and reduce the view area as if it were a wall.  This
                    //  could reduce iteration at the corners.
                    float distanceSquared = xc * xc + yc * yc;
                    if (distanceSquared <= viewRadiusSquared)
                    {
                        if (room != null)
                        {
                            room.Obscured = false;
                            visibleTiles.Add(room);
                        }
                    }

                    bool curBlocked = (room == null || room.Occluding);

                    if (prevWasBlocked)
                    {
                        if (curBlocked)
                        {
                            // Still traversing a column of walls.
                            savedRightSlope = rightBlockSlope;
                        }
                        else
                        {
                            // Found the end of the column of walls.  Set the left edge of our
                            // view area to the right corner of the last wall we saw.
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            // Found a wall.  Split the view area, recursively pursuing the
                            // part to the left.  The leftmost corner of the wall we just found
                            // becomes the right boundary of the view area.
                            //
                            // If this is the first block in the column, the slope of the top-left
                            // corner will be greater than the initial view slope (1.0).  Handle
                            // that here.
                            if (leftBlockSlope <= leftViewSlope)
                            {
                                CastLight(sourceTile, viewRadius, currentCol + 1, leftViewSlope, leftBlockSlope, txfrm);
                            }

                            // Once that's done, we keep searching to the right (down the column),
                            // looking for another opening.
                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                // Open areas are handled recursively, with the function continuing to search to
                // the right (down the column).  If we reach the bottom of the column without
                // finding an open cell, then the area defined by our view area is completely
                // obstructed, and we can stop working.
                if (prevWasBlocked)
                {
                    break;
                }
            }
        }
    }
}
