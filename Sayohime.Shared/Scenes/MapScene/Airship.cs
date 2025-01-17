using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Sayohime.Main;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;

namespace Sayohime.Scenes.MapScene
{
    public class Airship : Actor
    {
        protected enum HeroAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int HERO_WIDTH = 16;
        public const int HERO_HEIGHT = 16;

        public static readonly Rectangle HERO_BOUNDS = new Rectangle(-8, -16, 16, 16);

        private static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.IdleDown.ToString(), new Animation(1, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleLeft.ToString(), new Animation(1, 1, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleRight.ToString(), new Animation(1, 2, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleUp.ToString(), new Animation(1, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.WalkDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkLeft.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) }
        };

        private int ALTITUDE_INTERVAL = 250;

        private MapScene mapScene;

        private SceneObjects.Shaders.Light light;

        private int targetAltitude;
        private int altitudeCooldown = 30;

        public Action LandAction { get; set; }

        public Airship(MapScene iMapScene, TileMap iTilemap, Vector2 iPosition, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, AssetCache.SPRITES[GameSprite.Actors_Airship], HERO_ANIMATIONS, HERO_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            targetAltitude = 8;
            SetFlight(targetAltitude, AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
            OrientedAnimation("Walk");

            if (mapScene.SceneShader != null && mapScene.SceneShader is SceneObjects.Shaders.DayNight)
            {
                light = new SceneObjects.Shaders.Light(position - new Vector2(0, 6), 0.0f);
                light.Color = Color.AntiqueWhite;
                light.Intensity = 50;
                (mapScene.SceneShader as SceneObjects.Shaders.DayNight).Lights.Add(light);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (flightHeight != targetAltitude)
            {
                altitudeCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                while (altitudeCooldown <= 0)
                {
                    altitudeCooldown += ALTITUDE_INTERVAL;
                    if (flightHeight > targetAltitude) flightHeight--;
                    else flightHeight++;

                    if (flightHeight > 0 && shadowSprite == null) SetFlight(flightHeight, AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
                    if (flightHeight == targetAltitude)
                    {
                        if (flightHeight == 0)
                        {
                            shadowSprite = null;
                            Idle();

                            LandAction?.Invoke();
                            LandAction = null;
                        }
                        break;
                    }
                }
            }

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public override void CenterOn(Vector2 destination)
        {
            base.CenterOn(destination);

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public void SetTargetAltitude(int newAltitude)
        {
            targetAltitude = newAltitude;
        }

        public int TileX { get => HostTile == null ? 0 : HostTile.TileX; }
        public int TileY { get => HostTile == null ? 0 : HostTile.TileY; }
    }
}
