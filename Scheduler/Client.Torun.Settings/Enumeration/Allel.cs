using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler.Shared.Interfaces;

namespace Client.Torun.Settings.Enumeration
{
    public class Allel : IAllel
    {
        public static Allel OnDuty { get; } = new Allel(1, "On Duty");
        public static Allel OffDuty  { get; } = new Allel(2, "Off Duty");
        public static Allel DayOff { get; } = new Allel(3, "Day Off");

        public int Value { get; }
        public string Name { get; }

        private Allel(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static IEnumerable<IAllel> ListOfAllels()
        {
            return new[] {OnDuty, OffDuty, DayOff};
        }

        public static IAllel GetAllelByName(string allelName)
        {
            return ListOfAllels().FirstOrDefault(a => String.Equals(a.Name, allelName, StringComparison.OrdinalIgnoreCase));
        }

        public static IAllel GetAllelByValue(int allelValue)
        {
            return ListOfAllels().FirstOrDefault(a => a.Value == allelValue);
        }
    }
}
