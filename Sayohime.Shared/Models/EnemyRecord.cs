using System;
using System.Collections.Generic;

using Sayohime.SceneObjects;

namespace Sayohime.Models
{
    public class AttackData
    {
        public string[] Script;
        public int Weight;
        public TargetType Targetting = TargetType.OneEnemy;
        public int Power = -1;
        public int Hit = 100;
    }

    [Serializable]
    public class EnemyRecord
    {
        public EnemyRecord()
        {

        }

        public EnemyRecord(EnemyRecord clone)
        {
            Name = clone.Name;
            Level = clone.Level;
            Class = clone.Class;
            Sprite = clone.Sprite;
            ShadowOffset = clone.ShadowOffset;
            Exp = clone.Exp;
            HP = clone.HP;
            MP = clone.MP;
            Agility = clone.Agility;
            Magic = clone.Magic;
            Attack = clone.Attack;
            AttackMultiplier = clone.AttackMultiplier;
            Defense = clone.Defense;
            Evade = clone.Evade;
            MagicMultiplier = clone.MagicMultiplier;
            MagicDefense = clone.MagicDefense;
            MagicEvade = clone.MagicEvade;
            Analysis = clone.Analysis;
            Attacks = (AttackData[])clone.Attacks.Clone();
            if (clone.ElementWeak != null) ElementWeak = (ElementType[])clone.ElementWeak.Clone();
            if (clone.ElementStrong != null) ElementStrong = (ElementType[])clone.ElementStrong.Clone();
            if (clone.ElementImmune != null) ElementImmune = (ElementType[])clone.ElementImmune.Clone();
            if (clone.ElementAbsorb != null) ElementAbsorb = (ElementType[])clone.ElementAbsorb.Clone();
            if (clone.AilmentImmune != null) AilmentImmune = (AilmentType[])clone.AilmentImmune.Clone();
            BattleOffsetX = clone.BattleOffsetX;
            BattleOffsetY = clone.BattleOffsetY;
            BattleAlignment = clone.BattleAlignment;
        }

        public string Name { get; set; }
        public int Level { get; set; }
        public ClassType Class { get; set; }
        public string Sprite { get; set; }
        public int ShadowOffset { get; set; }
        public int Exp { get; set; }

        public long HP { get; set; }
        public int MP { get; set; }
        public int Agility { get; set; }
        public int Magic { get; set; }
        public int Attack { get; set; }
        public int AttackMultiplier { get; set; }
        public int Defense { get; set; }
        public int Evade { get; set; }
        public int MagicMultiplier { get; set; }
        public int MagicDefense { get; set; }
        public int MagicEvade { get; set; }
        public string Analysis { get; set; }

        public AttackData[] Attacks { get; set; }

        public ElementType[] ElementWeak { get; set; }
        public ElementType[] ElementStrong { get; set; }
        public ElementType[] ElementImmune { get; set; }
        public ElementType[] ElementAbsorb { get; set; }
        public AilmentType[] AilmentImmune { get; set; }

        public int BattleOffsetX { get; set; }
        public int BattleOffsetY { get; set; }
        public Alignment BattleAlignment { get; set; } = Alignment.Cascading;

        public static List<EnemyRecord> ENEMIES { get; set; }
    }
}
