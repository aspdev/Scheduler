using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scheduler.Shared.Classes;

namespace Scheduler.Shared.Interfaces
{
    public interface ISpeciesOriginator
    {
        PreGenerationCheckMessenger PerformPreGenerationChecks();
        Task SetDataFromCustomApi();
        List<Individual> Initialize();
        IAllel[,] Repair(IAllel[,] dna);
        Dictionary<DateTime, Object> CustomFormatSolution(IAllel[,] dna, DateTime date);
        

    }
}
