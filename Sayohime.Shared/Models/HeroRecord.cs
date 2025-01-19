using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayohime.Models
{
    public enum HeroType
    {
        Sayo
    }

    public class HeroRecord
    {
        public HeroType Name { get; set; }
        public Dictionary<string, string> Sprites { get; set; }

		public int BaseHP { get; set; }
		public int BaseMP { get; set; }
		public int BaseStrength { get; set; }
        public int BaseAgility { get; set; }
        public int BaseEndurance { get; set; }
        public int BaseMagic { get; set; }
		public int BaseLuck { get; set; }

		public static List<HeroRecord> HEROES { get; set; }
    }
}
