using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sayohime.Models
{
    public static partial class GameProfile
    {
        public static int SaveSlot { get; set; }

        public static SaveProfile CurrentSave;

        private static ManualResetEvent autosaveEvent = new ManualResetEvent(true);

        public static void NewState()
        {
            //SaveSlot = -1;
            CurrentSave = new SaveProfile();

            CurrentSave.AddInventory("Tonic", 3);
            CurrentSave.AddInventory("Ether", 1);
            CurrentSave.Money.Value = 800;

            var sparr = new HeroModel(HeroType.Sparr, ClassType.Scholar, 1);
            sparr.Equip("Mahogany");
            sparr.Equip("Talisman");
            sparr.Equip("Fancy Robes");
            CurrentSave.Party.Add(sparr);
        }

        public static void Autosave()
        {
            if (autosaveEvent.WaitOne(1000))
            {
                autosaveEvent.Reset();

                Task.Run(SaveState).ContinueWith(t => autosaveEvent.Set());
            }
        }

        public static void SetSaveData<T>(string name, T value)
        {
            if (CurrentSave.SaveData.ContainsKey(name)) CurrentSave.SaveData[name] = value;
            else CurrentSave.SaveData.Add(name, value);
        }

        public static T GetSaveData<T>(string name)
        {
            if (CurrentSave.SaveData.ContainsKey(name)) return (T)CurrentSave.SaveData[name];

            T newValue = default(T);
            CurrentSave.SaveData.Add(name, newValue);
            return newValue;
        }
    }
}
