using System.Threading.Tasks;
using Scheduler.Shared.Classes;

namespace Scheduler.Shared.Interfaces
{
    public interface IRequirement
    {
        int CalculateGeneralCost(Individual individual);
        int CalculateLocalCost(IAllel[] rowOfAlleles, int index);
        Task SetDataFromRemoteApi();
    }
}
