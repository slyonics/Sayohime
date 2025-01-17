using System;

using ldtk;

using Microsoft.Xna.Framework;

using Sayohime.SceneObjects.Maps;

namespace Sayohime.Scenes.MapScene
{
    public class EventTrigger : IInteractive
    {
        public static EventTrigger LastTrigger { get; set; }

        private MapScene mapScene;
        private EntityInstance entity;

        public string[] Script { get; set; }

        public bool TravelZone { get; set; }
        public bool DefaultTravelZone { get; set; } = true;

        public EventTrigger(MapScene iMapScene, EntityInstance iEntity)
        {
            mapScene = iMapScene;
            entity = iEntity;
            
            Bounds = new Rectangle((int)entity.Px[0], (int)entity.Px[1], (int)entity.Width, (int)entity.Height);

            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Name": Name = field.Value; break;
                    case "Script": Script = field.Value.Split('\n'); break;
                    case "Label": Label = field.Value; break;
                    case "Direction": Direction = (Orientation)Enum.Parse(typeof(Orientation), field.Value); break;
                    case "NoDefault": DefaultTravelZone = false; break;
                }
            }

            switch (entity.Identifier)
            {
                case "Automatic":
                    Interactive = false;
                    break;

                case "Travel":
                    Interactive = true;
                    TravelZone = true;
                    Script = ["ChangeMap " + Name];
                    break;

                case "Interactable":
                    Interactive = true;
                    break;
            }
        }

        public bool Activate(Hero hero)
        {
            if (!Interactive) return false;

            mapScene.AddController(new EventController(mapScene, Script));

            return true;
        }

        public string Name { get; private set; }
        public Rectangle Bounds { get; private set; }
        public Orientation Direction { get; set; }
        public bool Interactive { get; set; }
        public bool Terminated { get; set; }

        public string Label { get; set; } = "Trigger";
        public Vector2 LabelPosition { get => new Vector2(Bounds.Center.X, Bounds.Center.Y - Bounds.Height); }
    }
}
