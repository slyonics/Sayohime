using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Models
{
    public enum ItemType
    {
        Weapon,
        Shield,
        Armor,
        Accessory,
        Medicine,
        Consumable,
        Plot
    }

    [Serializable]
    public class ItemRecord
    {
        public ItemRecord()
        {

        }

        public string Name { get; set; }
		public string Description { get; set; }
		public ItemType ItemType { get; set; }
        public HeroType[] UsableBy { get; set; }
		public string Icon { get; set; }
		public long SellPrice { get; set; } = 13;

        public int StrengthModifier { get; set; }
        public int AgilityModifier { get; set; }
        public int EnduranceModifier { get; set; }
        public int MagicModifier { get; set; }
		public int LuckModifier { get; set; }
		public int Attack { get; set; }
        public int Hit { get; set; }
        public int Critical { get; set; }
        public int Defense { get; set; }
        public int MagicDefense { get; set; }
        public int Evade { get; set; }
        public int MagicEvade { get; set; }

        public ElementType AttackElement { get; set; }
        public ElementType[] ElementsWeak { get; set; }
        public ElementType[] ElementsStrong { get; set; }
        public ElementType[] ElementsAbsorb { get; set; }
        public ElementType[] ElementsImmune { get; set; }
        public AilmentType[] AilmentsImmune { get; set; }

        public ElementType[] ElementsBoost { get; set; }
        public BuffType[] AutoBuffs { get; set; }

		public TargetType Targetting { get; set; }
		public bool TargetDead { get; set; } // true if this can target dead allies

		public string Conditions { get; set; }
		public string[] BattleScript { get; set; }
		public string[] FieldScript { get; set; }

		public static List<ItemRecord> ITEMS { get; set; }
    }
}
