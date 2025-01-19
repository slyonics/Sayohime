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

    public class EnemyRecord
    {
        public EnemyRecord()
        {

        }

        public string Name { get; set; }
        public int Level { get; set; }
        public ClassType Class { get; set; }
        public string Sprite { get; set; }
        public int ShadowOffset { get; set; }
        public int Exp { get; set; }

        public long HP { get; set; }
        public int MP { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
		public int Endurance { get; set; }
		public int Magic { get; set; }
        public int Luck { get; set; }

		public int PhysicalDefense { get; set; }
		public int PhysicalEvade { get; set; }
        public int MagicDefense { get; set; }
        public int MagicEvade { get; set; }
        public string Description { get; set; }

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
