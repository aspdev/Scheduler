using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Scheduler.Shared.Classes;
using Scheduler.Shared.Interfaces;

namespace Scheduler.Shared.Abstract
{
    public abstract class Requirement : IRequirement
    {
        public abstract int CalculateGeneralCost(Individual individual);

        public abstract int CalculateLocalCost(IAllel[] rowOfAlleles, int index);

        public abstract Task SetDataFromRemoteApi();
    }
}
