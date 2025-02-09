using Sayohime.Models;
using Sayohime.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework;
using Sayohime.Main;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Scenes.CrawlerScene
{
    public class MapRoom
    {
        public class RoomWall
        {
            public Direction Orientation { get; set; }
            public VertexPositionTexture[] Quad { get; set; }
            public Vector4[] Lighting { get; set; } = [ new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f) ];
            public Texture2D Texture { get; set; }
            public WallShader Shader { get; set; }

            public RoomWall(Direction iOrientation, Texture2D iTexture, float startU, float startV, float endU, float endV, float heightOffset = 0.0f)
            {
                Orientation = iOrientation;
                Texture = iTexture;

                VertexPositionTexture[] quad =
				[
					new VertexPositionTexture(VERTICES[Orientation][0] - new Vector3(0, heightOffset, 0), new Vector2(startU, startV)),
					new VertexPositionTexture(VERTICES[Orientation][1] - new Vector3(0, heightOffset, 0), new Vector2(startU, endV)),
					new VertexPositionTexture(VERTICES[Orientation][2] - new Vector3(0, heightOffset, 0), new Vector2(endU, endV)),
					new VertexPositionTexture(VERTICES[Orientation][3] - new Vector3(0, heightOffset, 0), new Vector2(endU, startV)),
				];
				Quad = quad;
            }

            public RoomWall(Direction iOrientation, Texture2D iTexture, Dictionary<Direction, Vector3[]> vertices, float startU, float startV, float endU, float endV)
            {
                Orientation = iOrientation;
                Texture = iTexture;

                VertexPositionTexture[] quad =
				[
					new VertexPositionTexture(vertices[Orientation][0], new Vector2(startU, startV)),
					new VertexPositionTexture(vertices[Orientation][1], new Vector2(startU, endV)),
					new VertexPositionTexture(vertices[Orientation][2], new Vector2(endU, endV)),
					new VertexPositionTexture(vertices[Orientation][3], new Vector2(endU, startV)),
				];
				Quad = quad;
            }
        }

        private const int WALL_HALF_LENGTH = 5;
        private const int CAM_HEIGHT = -1;
        private static readonly short[] INDICES = [0, 2, 1, 2, 0, 3];
        private static readonly Dictionary<Direction, Vector3[]> VERTICES = new Dictionary<Direction, Vector3[]>()
        {   {
                Direction.North, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.West, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.East, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                Direction.South, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                Direction.Up, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.Down, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
        } };

        private static Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.MiniMap];
        private static readonly Rectangle[] minimapSource = [new Rectangle(0, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 1, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 2, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 3, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE)];
        private static Texture2D enemyIndicator = AssetCache.SPRITES[GameSprite.FoeMarker];
        private static readonly Rectangle[] enemySource = [new Rectangle(0, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 1, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 2, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), new Rectangle(Floor.MINI_CELL_SIZE * 3, 0, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE)];

        public int RoomX { get; set; }
        public int RoomY { get; set; }


        public bool Blocked { get; set; }
        public bool Occluding { get; set; }

        public bool Obscured { get; set; } = true;

        public string[] Script { get; set; }
        public string[] PreEnterScript { get; set; }
        public string[] InteractScript { get; set; }

        //public WallShader WallEffect { get; private set; }
        private GraphicsDevice graphicsDevice = CrossPlatformGame.GameInstance.GraphicsDevice;

        private CrawlerScene parentScene;
        private Floor parentFloor;
        private Matrix translationMatrix;

        private bool door = false;
        int waypointTile;

        private Dictionary<Direction, RoomWall> wallList = [];
		private Dictionary<Direction, RoomWall> upperWallList = [];

		public int brightnessLevel = 0;
        private float[] lightVertices;

        public Foe Foe { get; set; }

        public Chest Chest { get; set; }

        public MapRoom(CrawlerScene mapScene, Floor iFloor, int x, int y)
        {
            parentScene = mapScene;
            parentFloor = iFloor;
            RoomX = x;
            RoomY = y;
            waypointTile = 1;
        }

        public void ApplyTile(string layerName, TilesetDefinition tileset, TileInstance tile)
        {

            switch (layerName)
            {
                case "Walls":
                    {
                        Blocked = true;
                        Occluding = true;

                        string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;

                        var westWall = this[Direction.West];
                        if (RoomX > 0 && westWall != null && !westWall.Occluding)
                            westWall.ApplyWall(Direction.East, SpriteAtlas, startU, startV, endU, endV);

                        var eastWall = this[Direction.East];
                        if (RoomX < parentFloor.MapWidth - 1 && eastWall != null && !eastWall.Occluding)
                            eastWall.ApplyWall(Direction.West, SpriteAtlas, startU, startV, endU, endV);

                        var northWall = this[Direction.North];
                        if (RoomY > 0 && northWall != null && !northWall.Occluding)
                            northWall.ApplyWall(Direction.South, SpriteAtlas, startU, startV, endU, endV);

                        var southWall = this[Direction.South];
                        if (RoomY < parentFloor.MapHeight - 1 && southWall != null && !southWall.Occluding)
                            southWall.ApplyWall(Direction.North, SpriteAtlas, startU, startV, endU, endV);
                    }
                    break;

                case "UpperWalls":
                    {
						string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
						var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
						float startU = tile.Src[0] / (float)SpriteAtlas.Width;
						float startV = tile.Src[1] / (float)SpriteAtlas.Height;
						float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
						float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;

						var westWall = this[Direction.West];
						if (RoomX > 0 && westWall != null && !westWall.Occluding)
							westWall.ApplyUpperWall(Direction.East, SpriteAtlas, startU, startV, endU, endV, 10.0f);

						var eastWall = this[Direction.East];
						if (RoomX < parentFloor.MapWidth - 1 && eastWall != null && !eastWall.Occluding)
							eastWall.ApplyUpperWall(Direction.West, SpriteAtlas, startU, startV, endU, endV, 10.0f);

						var northWall = this[Direction.North];
						if (RoomY > 0 && northWall != null && !northWall.Occluding)
							northWall.ApplyUpperWall(Direction.South, SpriteAtlas, startU, startV, endU, endV, 10.0f);

						var southWall = this[Direction.South];
						if (RoomY < parentFloor.MapHeight - 1 && southWall != null && !southWall.Occluding)
							southWall.ApplyUpperWall(Direction.North, SpriteAtlas, startU, startV, endU, endV, 10.0f);
					}

                    break;

                case "Ceiling":
                    {
                        string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;
                        wallList.Add(Direction.Up, new RoomWall(Direction.Up, SpriteAtlas, startU, startV, endU, endV));
                    }
                    break;

                case "Floor":
                    {
                        string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;
                        wallList.Add(Direction.Down, new RoomWall(Direction.Down, SpriteAtlas, startU, startV, endU, endV));

                        var terrain = tileset.EnumTags.FirstOrDefault(x => x.TileIds.Contains(tile.T));
                        if (terrain != null)
                        {
                            switch (terrain.EnumValueId)
                            {
                                case "Blocked":
                                    Blocked = true;
                                    break;
                            }
                        }
                    }
                    break;
            }


            ResetMinimapIcon();
        }

        public void ApplyWall(Direction direction, Texture2D texture2D, float startU, float startV, float endU, float endV)
        {
            if (wallList.TryGetValue(direction, out var wall))
            {
                wall.Texture = texture2D;
                throw new Exception();
            }
            else
            {
                wallList.Add(direction, new RoomWall(direction, texture2D, startU, startV, endU, endV));
            }
        }

		public void ApplyUpperWall(Direction direction, Texture2D texture2D, float startU, float startV, float endU, float endV, float heightOffset)
		{
			if (upperWallList.TryGetValue(direction, out var wall))
			{
				wall.Texture = texture2D;
				throw new Exception();
			}
			else
			{
				upperWallList.Add(direction, new RoomWall(direction, texture2D, startU, startV, endU, endV, heightOffset));
			}
		}

		public void SetVertices(int x, int y)
        {
            translationMatrix = Matrix.CreateTranslation(new Vector3(10 * x, 0, 10 * (parentFloor.MapHeight - y)));

            BuildShader();
        }

        private void BuildShader()
        {
            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                wall.Value.Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f));
                wall.Value.Shader.World = translationMatrix;
                wall.Value.Shader.WallTexture = wall.Value.Texture;

                Vector4 brightness;

                switch (wall.Value.Orientation)
                {
                    case Direction.Up: brightness = new Vector4(Brightness(lightVertices[2]), Brightness(lightVertices[3]), Brightness(lightVertices[0]), Brightness(lightVertices[1])); break;
                    case Direction.North: brightness = new Vector4(Brightness(lightVertices[1]), Brightness(lightVertices[0]), Brightness(lightVertices[3]), Brightness(lightVertices[2])); break;
                    case Direction.East: brightness = new Vector4(Brightness(lightVertices[3]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[0])); break;
                    case Direction.West: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[2]), Brightness(lightVertices[1]), Brightness(lightVertices[3])); break;
                    default: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[3])); break;
                }

                wall.Value.Shader.Brightness = brightness;
            }

			foreach (KeyValuePair<Direction, RoomWall> wall in upperWallList)
			{
				wall.Value.Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f));
				wall.Value.Shader.World = translationMatrix;
				wall.Value.Shader.WallTexture = wall.Value.Texture;

				Vector4 brightness;

				switch (wall.Value.Orientation)
				{
					case Direction.Up: brightness = new Vector4(Brightness(lightVertices[2]), Brightness(lightVertices[3]), Brightness(lightVertices[0]), Brightness(lightVertices[1])); break;
					case Direction.North: brightness = new Vector4(Brightness(lightVertices[1]), Brightness(lightVertices[0]), Brightness(lightVertices[3]), Brightness(lightVertices[2])); break;
					case Direction.East: brightness = new Vector4(Brightness(lightVertices[3]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[0])); break;
					case Direction.West: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[2]), Brightness(lightVertices[1]), Brightness(lightVertices[3])); break;
					default: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[3])); break;
				}

				wall.Value.Shader.Brightness = brightness;
			}
		}

        public float Brightness(float x)
        {
            return Math.Min(1.0f, Math.Max(x / 4.0f, parentFloor.AmbientLight));
        }

        public void BlendLighting()
        {
            lightVertices = [0.25f, 0.25f, 0.25f, 0.25f];

            int[] neighborBrightness = new int[9];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (RoomX + x < 0 || RoomY + y < 0 || RoomX + x >= parentFloor.MapWidth || RoomY + y >= parentFloor.MapHeight) neighborBrightness[i] = brightnessLevel;
                    else
                    {
                        MapRoom mapRoom = parentFloor.GetRoom(RoomX + x, RoomY + y);
                        neighborBrightness[i] = mapRoom == null || mapRoom.Blocked ? brightnessLevel : mapRoom.brightnessLevel;
                    }
                    i++;
                }
            }

            List<MapRoom> nw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.North])) { nw.Add(this[Direction.North]); if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.West])) nw.Add(this[Direction.North][Direction.West]); }
            if (Neighbors.Contains(this[Direction.West])) { nw.Add(this[Direction.West]); nw.Add(this[Direction.West]); if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.North])) nw.Add(this[Direction.West][Direction.North]); }
            lightVertices[0] = (float)nw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> ne = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.North])) { ne.Add(this[Direction.North]); if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.East])) ne.Add(this[Direction.North][Direction.East]); }
            if (Neighbors.Contains(this[Direction.East])) { ne.Add(this[Direction.East]); ne.Add(this[Direction.East]); if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.North])) ne.Add(this[Direction.East][Direction.North]); }
            lightVertices[1] = (float)ne.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> sw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.South])) { sw.Add(this[Direction.South]); if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.West])) sw.Add(this[Direction.South][Direction.West]); }
            if (Neighbors.Contains(this[Direction.West])) { sw.Add(this[Direction.West]); sw.Add(this[Direction.West]); if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.South])) sw.Add(this[Direction.West][Direction.South]); }
            lightVertices[2] = (float)sw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> se = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.South])) { se.Add(this[Direction.South]); if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.East])) se.Add(this[Direction.South][Direction.East]); }
            if (Neighbors.Contains(this[Direction.East])) { se.Add(this[Direction.East]); se.Add(this[Direction.East]); if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.South])) se.Add(this[Direction.East][Direction.South]); }
            lightVertices[3] = (float)se.Distinct().Average(x => x.brightnessLevel);
        }

        public void Draw(Matrix viewMatrix)
        {
            if (Occluding) return;

            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                DrawWall(wall.Value, viewMatrix);
            }

			foreach (KeyValuePair<Direction, RoomWall> wall in upperWallList)
			{
				DrawWall(wall.Value, viewMatrix);
			}
		}

        public void DrawWall(RoomWall wall, Matrix viewMatrix)
        {
            wall.Shader.View = viewMatrix;
            foreach (EffectPass pass in wall.Shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, wall.Quad, 0, 4, INDICES, 0, 2);
            }
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Vector2 offset, float depth)
        {
            if (Obscured) return;

            Color color = Occluding ? Color.CornflowerBlue : Color.White;
            spriteBatch.Draw(minimapSprite, offset, minimapSource[waypointTile], color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);

            if (Foe != null)
                spriteBatch.Draw(enemyIndicator, offset, enemySource[(int)Foe.Direction], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.002f);
        }
        public void ResetMinimapIcon()
        {
            waypointTile = Occluding ? 0 : 1;
            if (door) waypointTile = 2;
        }

        public void BuildNeighbors()
        {
            if (!wallList.ContainsKey(Direction.West))
            {
                if (RoomX > 0 && parentFloor.GetRoom(RoomX - 1, RoomY) != null && !parentFloor.GetRoom(RoomX - 1, RoomY).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX - 1, RoomY));
                //else wallList.Add(Direction.West, new RoomWall() { Orientation = Direction.West, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.East))
            {
                if (RoomX < parentFloor.MapWidth - 1 && parentFloor.GetRoom(RoomX + 1, RoomY) != null && !parentFloor.GetRoom(RoomX + 1, RoomY).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX + 1, RoomY));
                //else wallList.Add(Direction.East, new RoomWall() { Orientation = Direction.East, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.North))
            {
                if (RoomY > 0 && parentFloor.GetRoom(RoomX, RoomY - 1) != null && !parentFloor.GetRoom(RoomX, RoomY - 1).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX, RoomY - 1));
                //else wallList.Add(Direction.North, new RoomWall() { Orientation = Direction.North, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.South))
            {
                if (RoomY < parentFloor.MapHeight - 1 && parentFloor.GetRoom(RoomX, RoomY + 1) != null && !parentFloor.GetRoom(RoomX, RoomY + 1).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX, RoomY + 1));
                //else if (!wallList.ContainsKey(Direction.South)) wallList.Add(Direction.South, new RoomWall() { Orientation = Direction.South, Texture = AssetCache.SPRITES[defaultWall] });
            }
        }

        public bool Activate(Direction direction)
        {
            if (InteractScript == null) return false;
            else
            {
                EventController eventController = new EventController(parentScene, InteractScript, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();

                return true;
            }


            /*
            string[] script;
            if (ActivateScript.TryGetValue(direction, out script))
            {
                EventController eventController = new EventController(parentScene, script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();

                return true;
            }
            else return false;
            */
        }

        public void ActivatePreScript()
        {
            if (PreEnterScript != null)
            {
                EventController eventController = new EventController(parentScene, PreEnterScript, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }
        }

        public void EnterRoom(bool finishedMove = true)
        {
            parentFloor.CalculateFieldOfView(this, 8);

            if (Script != null)
            {
                EventController eventController = new EventController(parentScene, Script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }
        }



        public void SetAsWaypoint()
        {
            waypointTile = 3;
        }


        public List<MapRoom> Neighbors { get; private set; } = new List<MapRoom>();

        public MapRoom this[Direction key]
        {
            get
            {
                switch (key)
                {
                    case Direction.North: return parentFloor.GetRoom(RoomX, RoomY - 1);
                    case Direction.East: return parentFloor.GetRoom(RoomX + 1, RoomY);
                    case Direction.South: return parentFloor.GetRoom(RoomX, RoomY + 1);
                    case Direction.West: return parentFloor.GetRoom(RoomX - 1, RoomY);
                    default: return null;
                }
            }
        }
    }
}
