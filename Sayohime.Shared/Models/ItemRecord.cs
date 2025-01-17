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

        public ItemRecord(ItemRecord clone)
        {
            ItemType = clone.ItemType;
            if (clone.UsableBy != null) UsableBy = (ClassType[])clone.UsableBy.Clone();

            StrengthModifier = clone.StrengthModifier;
            AgilityModifier = clone.AgilityModifier;
            VitalityModifier = clone.VitalityModifier;
            MagicModifier = clone.MagicModifier;
            Attack = clone.Attack;
            Hit = clone.Hit;
            Critical = clone.Critical;
            Defense = clone.Defense;
            MagicDefense = clone.MagicDefense;
            Evade = clone.Evade;
            MagicEvade = clone.MagicEvade;
            Weight = clone.Weight;

            AttackElement = clone.AttackElement;
            if (clone.ElementsWeak != null) ElementsWeak = (ElementType[])clone.ElementsWeak.Clone();
            if (clone.ElementsStrong != null) ElementsStrong = (ElementType[])clone.ElementsStrong.Clone();
            if (clone.ElementsAbsorb != null) ElementsAbsorb = (ElementType[])clone.ElementsAbsorb.Clone();
            if (clone.ElementsImmune != null) ElementsImmune = (ElementType[])clone.ElementsImmune.Clone();
            if (clone.AilmentsImmune != null) AilmentsImmune = (AilmentType[])clone.AilmentsImmune.Clone();

            if (clone.ElementsBoost != null) ElementsBoost = (ElementType[])clone.ElementsBoost.Clone();
            if (clone.AutoBuffs != null) AutoBuffs = (BuffType[])clone.AutoBuffs.Clone();

            Conditions = clone.Conditions;
        }

        public ItemType ItemType { get; set; }
        public ClassType[] UsableBy { get; set; }
        public long SellPrice { get; set; } = 13;

        public int StrengthModifier { get; set; }
        public int AgilityModifier { get; set; }
        public int VitalityModifier { get; set; }
        public int MagicModifier { get; set; }
        public int Attack { get; set; }
        public int Hit { get; set; }
        public int Critical { get; set; }
        public int Defense { get; set; }
        public int MagicDefense { get; set; }
        public int Evade { get; set; }
        public int MagicEvade { get; set; }
        public int Weight { get; set; }

        public ElementType AttackElement { get; set; }
        public ElementType[] ElementsWeak { get; set; }
        public ElementType[] ElementsStrong { get; set; }
        public ElementType[] ElementsAbsorb { get; set; }
        public ElementType[] ElementsImmune { get; set; }
        public AilmentType[] AilmentsImmune { get; set; }

        public ElementType[] ElementsBoost { get; set; }
        public BuffType[] AutoBuffs { get; set; }

        public static List<ItemRecord> ITEMS { get; set; }
    }
}
