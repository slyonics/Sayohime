using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Models
{
    public class ItemModel
    {
        public ItemModel(AbilityRecord itemRecord)
        {
            ItemRecord = itemRecord;
            Quantity.Value = -1;
            CastingCost.Value = itemRecord.Cost;
        }

        public ItemModel(ItemRecord itemRecord)
        {
            ItemRecord = itemRecord;
            Quantity.Value = -1;
        }

        public ItemModel(string itemName, int quantity)
        {
            ItemRecord = new ItemRecord(Models.ItemRecord.ITEMS.First(x => x.Name == itemName));
            Quantity.Value = quantity;
        }

        public void ReloadScripts()
        {
            ItemRecord itemRecord = Models.ItemRecord.ITEMS.FirstOrDefault(x => x.Name == ItemRecord.Name);
            if (itemRecord.Conditions != null) ItemRecord.Conditions = (string)itemRecord.Conditions.Clone();
            if (itemRecord.FieldScript != null) ItemRecord.FieldScript = (string[])itemRecord.FieldScript.Clone();
            if (itemRecord.BattleScript != null) ItemRecord.BattleScript = (string[])itemRecord.BattleScript.Clone();
        }

        public CommandRecord ItemRecord { get; set; }
        public ModelProperty<int> Quantity { get; set; } = new ModelProperty<int>(1);
        public ModelProperty<int> CastingCost { get; set; } = new ModelProperty<int>(-1);

        public bool FieldUsable { get => ItemRecord.FieldScript != null; }
        public bool BattleUsable
        {
            get
            {
                if (ThrowMode) return Throwable;
                if (ItemRecord.BattleScript == null) return false;
                if (Quantity.Value >= 0) return true;

                var abilityRecord = ItemRecord as AbilityRecord;
                return abilityRecord != null && CurrentUser != null && abilityRecord.Cost <= CurrentUser.MP.Value;
            }
        }

        public static HeroModel CurrentUser;

        public bool Consumable { get => Quantity.Value > 0; }

        public bool Castable { get => CastingCost.Value >= 0; }

        public bool Throwable { get => ItemRecord.ThrowScript != null; }

        public static bool ThrowMode;
    }
}

