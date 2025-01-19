using System.Linq;

namespace Sayohime.Models
{
    public class ItemModel
    {
        public ItemModel(ItemRecord itemRecord)
        {
            ItemRecord = itemRecord;
            Quantity.Value = -1;
        }

        public ItemModel(string itemName, int quantity)
        {
            ItemRecord = new ItemRecord(ItemRecord.ITEMS.First(x => x.Name == itemName));
            Quantity.Value = quantity;
        }

        public ItemRecord ItemRecord { get; set; }
        public ModelProperty<int> Quantity { get; set; } = new ModelProperty<int>(1);

        public bool FieldUsable { get => ItemRecord.FieldScript != null; }
        public bool BattleUsable { get => ItemRecord.BattleScript != null; }

		public bool Consumable { get => Quantity.Value > 0; }
    }
}

