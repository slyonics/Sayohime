using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Sayohime.Main;
using Sayohime.Models;
using Sayohime.SceneObjects;
using Sayohime.SceneObjects.Maps;

namespace Sayohime.Scenes.MapScene
{
    public class Hero : Actor
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
            WalkUp,
            Faint
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
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.Faint.ToString(), new Animation(0, 4, HERO_WIDTH, HERO_HEIGHT, 3, 800) }
        };

        private MapScene mapScene;

        private SceneObjects.Shaders.Light light;

        public bool Hide { get; set; } = false;
        public HeroModel HeroModel { get; private set; }

        public Hero(MapScene iMapScene, TileMap iTilemap, Vector2 iPosition, HeroModel heroModel, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), heroModel.Sprite.Value.ToString())], HERO_ANIMATIONS, HERO_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;
            HeroModel = heroModel;

            if (HeroModel.FlightHeight.Value > 0)
            {
                SetFlight(HeroModel.FlightHeight.Value, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), HeroModel.ShadowSprite.Value.ToString())]);
            }

            /*
            if (mapScene.Tilemap.Name == "TechHomeworld")
            {
                animatedSprite.Scale = new Vector2(0.5f, 0.5f);
                if (shadowSprite != null) shadowSprite.Scale = new Vector2(0.5f, 0.5f);
            }
            */

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
            if (CrossPlatformGame.CurrentScene is StatusScene.StatusScene) return;

            base.Update(gameTime);

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public override void CenterOn(Vector2 destination)
        {
            base.CenterOn(destination);

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (Hide) return;

            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode"))
                Debug.DrawBox(spriteBatch, InteractionZone);
        }

        public void UpdateSprite()
        {
            var animation = animatedSprite.AnimationName;
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), HeroModel.Sprite.Value.ToString())], HERO_ANIMATIONS);
            animatedSprite.PlayAnimation(animation);
        }

        public Rectangle InteractionZone;

        public int TileX { get => HostTile == null ? 0 : HostTile.TileX; }
        public int TileY { get => HostTile == null ? 0 : HostTile.TileY; }
    }
}
