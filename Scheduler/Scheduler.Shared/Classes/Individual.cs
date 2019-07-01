using System.Collections.Generic;
using Scheduler.Shared.Interfaces;

namespace Scheduler.Shared.Classes
{
    public class Individual
    {
        public IAllel[,] Dna { get; set; } 

        public Dictionary<int, IAllel[]> IndexedRows { get; set; } = new Dictionary<int, IAllel[]>();

        public List<KeyValuePair<int, IAllel[]>> SortedRows { get; set; } = new List<KeyValuePair<int, IAllel[]>>();

        public int GeneralCost { get; set; }

        public float Fitness => 1 / (float)(1 + GeneralCost);
    }
}
