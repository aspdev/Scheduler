using Scheduler.Engine.Genetics;
using Scheduler.Shared.Classes;

namespace Scheduler.Engine.EventArgs
{
    public class BestSolutionEventArg
    {
        public Individual BestSolution { get; set; }
    }
}
