using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Models
{
    public enum ClassType
    {
        None,

        Maho,
        Monster
    }

	public enum ElementType
	{
		None,

		Blunt,
		Sharp,
		Ranged,


		Ice,
		Fire,
		Thunder,
		Earth,
		Poison,
		Life,
		Dark,
		Holy
	}

	public enum AilmentType
	{
		Healthy,

		Fear,
		Death,
		Confusion,
		Poison,
		Stone
	}

	public enum BuffType
	{
		AutoRevive
	}

    public enum TargetType
    {
        OneEnemy,
        OneAlly,
        AllEnemies,
        AllAllies,
        Self,
        All,
        None
    }

	public class BattlerModel
    {
        public BattlerModel()
        {

        }

        public BattlerModel(EnemyRecord enemyRecord)
        {
            Name.Value = enemyRecord.Name;
            Class.Value = enemyRecord.Class;
            Level.Value = enemyRecord.Level;
			Description.Value = enemyRecord.Description;

			MaxHP.Value = enemyRecord.HP;
            HP.Value = MaxHP.Value;
            MaxMP.Value = enemyRecord.MP;
            MP.Value = MaxMP.Value;

            Strength.Value = enemyRecord.Strength;
            Agility.Value = enemyRecord.Agility;
            Endurance.Value = enemyRecord.Endurance;
            Magic.Value = enemyRecord.Magic;
            Luck.Value = enemyRecord.Luck;

            PhysicalDefense.Value = enemyRecord.PhysicalDefense;
            PhysicalEvade.Value = enemyRecord.PhysicalEvade;
            MagicDefense.Value = enemyRecord.MagicDefense;
            MagicEvade.Value = enemyRecord.MagicEvade;

            ElementWeak.ModelList = new List<ModelProperty<ElementType>>();
            if (enemyRecord.ElementWeak != null) foreach (var element in enemyRecord.ElementWeak) ElementWeak.Add(element);

            ElementStrong.ModelList = new List<ModelProperty<ElementType>>();
            if (enemyRecord.ElementStrong != null) foreach (var element in enemyRecord.ElementStrong) ElementStrong.Add(element);

            ElementImmune.ModelList = new List<ModelProperty<ElementType>>();
            if (enemyRecord.ElementImmune != null) foreach (var element in enemyRecord.ElementImmune) ElementImmune.Add(element);

            ElementAbsorb.ModelList = new List<ModelProperty<ElementType>>();
            if (enemyRecord.ElementAbsorb != null) foreach (var element in enemyRecord.ElementAbsorb) ElementAbsorb.Add(element);

            AilmentImmune.ModelList = new List<ModelProperty<AilmentType>>();
            if (enemyRecord.AilmentImmune != null) foreach (var ailment in enemyRecord.AilmentImmune) AilmentImmune.Add(ailment);
        }


        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>("Enemy");
        public ModelProperty<ClassType> Class { get; set; } = new ModelProperty<ClassType>(ClassType.Monster);
        public ModelProperty<int> Level { get; set; } = new ModelProperty<int>(1);
        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("This is your own party member, you fool.");

        public ModelCollection<AilmentType> StatusAilments { get; set; } = new ModelCollection<AilmentType>();
        public ModelProperty<long> HP { get; set; } = new ModelProperty<long>(10);
        public ModelProperty<long> MaxHP { get; set; } = new ModelProperty<long>(10);
        public ModelProperty<int> MP { get; set; } = new ModelProperty<int>(10);
        public ModelProperty<int> MaxMP { get; set; } = new ModelProperty<int>(10);

        public ModelProperty<int> Strength { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Agility { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Endurance { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Magic { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Luck { get; set; } = new ModelProperty<int>(3);

        public ModelProperty<int> PhysicalDefense { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> PhysicalEvade { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> MagicDefense { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> MagicEvade { get; set; } = new ModelProperty<int>(0);

		public ModelCollection<ElementType> ElementWeak { get; set; } = new ModelCollection<ElementType>();
        public ModelCollection<ElementType> ElementStrong { get; set; } = new ModelCollection<ElementType>();
        public ModelCollection<ElementType> ElementImmune { get; set; } = new ModelCollection<ElementType>();
        public ModelCollection<ElementType> ElementAbsorb { get; set; } = new ModelCollection<ElementType>();
        public ModelCollection<AilmentType> AilmentImmune { get; set; } = new ModelCollection<AilmentType>();
    }
}
