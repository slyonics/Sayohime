using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.PortableExecutable;

namespace Sayohime.Models
{
    [Serializable]
    public class SaveProfile
    {
        private static Dictionary<string, object> DEFAULT_SAVE_VALUES = new Dictionary<string, object>()
        {
            { "NewGame", true }
        };

        public SaveProfile()
        {
            SaveData = new Dictionary<string, object>(DEFAULT_SAVE_VALUES);
        }

        public SaveProfile(BinaryReader reader)
        {
            reader.ReadString();
            reader.ReadInt32();
            reader.ReadString();
            reader.ReadString();

            int partyCount = reader.ReadInt32();
            for (int i = 0; i < partyCount; i++) Party.Add(new HeroModel(reader));

            int inventoryCount = reader.ReadInt32();
            for (int i = 0; i < inventoryCount; i++)
            {
                string itemName = reader.ReadString();
                var itemRecord = ItemRecord.ITEMS.First(x => x.Name == itemName);
                int itemQuantity = reader.ReadInt32();
                AddInventory(itemName, itemQuantity);
            }
            Money.Value = reader.ReadInt64();

            Steps.Value = reader.ReadInt64();

            LocationName.Value = reader.ReadString();
            LastMap = reader.ReadString();
            LastSpawn = reader.ReadString();

            SaveData = new Dictionary<string, object>();
            int saveCount = reader.ReadInt32();
            for (int i = 0; i < saveCount; i++)
            {
                string saveName = reader.ReadString();
                byte typeByte = reader.ReadByte();
                switch (typeByte)
                {
                    case 0: SaveData.Add(saveName, reader.ReadBoolean()); break;
                    case 1: SaveData.Add(saveName, reader.ReadByte()); break;
                    case 2: SaveData.Add(saveName, reader.ReadInt32()); break;
                    case 3: SaveData.Add(saveName, reader.ReadInt64()); break;
                    case 4: SaveData.Add(saveName, reader.ReadSingle()); break;
                    case 5: SaveData.Add(saveName, reader.ReadString()); break;
                }
            }
        }

        public void AddInventory(string itemName, int quantity)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value.ItemRecord.Name == itemName);
            if (itemEntry == null || quantity < 0)
            {
                Inventory.Add(new ItemModel(itemName, quantity));
            }
            else
            {
                itemEntry.Value.Quantity.Value = itemEntry.Value.Quantity.Value + quantity;
                if (itemEntry.Value.Quantity.Value < 1) Inventory.Remove(itemEntry);
            }
        }

        public void RemoveInventory(ItemModel itemModel, int quantity)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value == itemModel);

            itemEntry.Value.Quantity.Value = itemEntry.Value.Quantity.Value + quantity;
            if (itemEntry.Value.Quantity.Value < 1)
            {
                Inventory.Remove(itemEntry);
            }
        }

        public int GetInventoryCount(string itemName)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value.ItemRecord.Name == itemName);
            if (itemEntry == null) return 0;
            else return itemEntry.Value.Quantity.Value;
        }

        public void WriteToFile(BinaryWriter writer)
        {
            writer.Write(LocationName.Value);
            writer.Write(GameProfile.SaveSlot);
            writer.Write(Party[0].PortraitSprite.Value);
            writer.Write(Party.Count < 2 ? Main.GameSprite.Actors_Blank.ToString() : Party[1].PortraitSprite.Value);

            writer.Write(Party.Count);
            foreach (var hero in Party) hero.Value.WriteToFile(writer);

            writer.Write(Inventory.Count);
            foreach (var item in Inventory)
            {
                writer.Write(item.Value.ItemRecord.Name);
                writer.Write(item.Value.Quantity.Value);
            }
            writer.Write(Money.Value);

            writer.Write(Steps.Value);

            writer.Write(LocationName.Value);
            writer.Write(LastMap);
            writer.Write(LastSpawn);

            writer.Write(SaveData.Count);
            foreach (var item in SaveData)
            {
                writer.Write(item.Key);
                switch (item.Value)
                {
                    case bool: writer.Write((byte)0); writer.Write((bool)item.Value); break;
                    case byte: writer.Write((byte)1); writer.Write((byte)item.Value); break;
                    case int: writer.Write((byte)2); writer.Write((int)item.Value); break;
                    case long: writer.Write((byte)3); writer.Write((long)item.Value); break;
                    case float: writer.Write((byte)4); writer.Write((float)item.Value); break;
                    case string: writer.Write((byte)5); writer.Write((string)item.Value); break;
                }
            }
        }

		[field: NonSerialized]
		public HeroModel StoredHero { get; set; }

		public ModelCollection<HeroModel> Party { get; set; } = new ModelCollection<HeroModel>();
        public ModelCollection<ItemModel> Inventory { get; set; } = new ModelCollection<ItemModel>();
		public ModelProperty<long> Money { get; set; } = new ModelProperty<long>(13);

		public ModelProperty<long> Steps { get; set; } = new ModelProperty<long>(0);

        public ModelProperty<string> LocationName { get; set; } = new ModelProperty<string>("Your House");                
        public string LastMap { get; set; } = "Interior";
		public string LastSpawn { get; set; } = "Intro";


        public Dictionary<string, object> SaveData;
	}
}
