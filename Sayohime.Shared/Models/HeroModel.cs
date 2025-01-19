using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sayohime.Models
{
	public class HeroModel : BattlerModel
	{
		public HeroModel()
		{

		}

		public HeroModel(BinaryReader binaryReader)
        {

        }

		public void WriteToFile(BinaryWriter binaryWriter)
		{

		}

		public ModelProperty<int> Attack { get; set; } = new ModelProperty<int>(0);
		public ModelProperty<int> Hit { get; set; } = new ModelProperty<int>(100);
	}
}
