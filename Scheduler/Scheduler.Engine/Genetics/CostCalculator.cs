using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scheduler.Engine.EventArgs;
using Scheduler.Shared.Abstract;
using Scheduler.Shared.Classes;

namespace Scheduler.Engine.Genetics
{
    public class CostCalculator
    {
        private float _highestFitness = 0.0f;

        private SpinLock spinLock = new SpinLock();

        public List<Requirement> Requirements;

        public event EventHandler<BestSolutionEventArg> BestSolutionFound;


        #region Constructor


        public CostCalculator(List<Requirement> requirements)
        {
            Requirements = requirements;
        }


        #endregion

        #region public CalculateGeneralCost

        public void CalculateGeneralCost(List<Individual> individuals, int numberOfGenerations)
        {
            Parallel.ForEach(individuals, (individual) =>
            {
                individual.GeneralCost = 0;


                foreach (var requirement in Requirements)
                {
                    individual.GeneralCost += requirement.CalculateGeneralCost(individual);
                }


                #region RaiseEventIfCurrentSolutionIsBetterThanPrevious

                if (numberOfGenerations <= 0)
                    return;


                bool lockTaken = false;

                try
                {
                    spinLock.Enter(ref lockTaken);

                    if (individual.Fitness >= _highestFitness)
                    {
                        _highestFitness = individual.Fitness;
                        BestSolutionEventArg args = new BestSolutionEventArg { BestSolution = individual };
                        OnBestSolutionFound(args);
                    }
                }
                finally
                {
                    spinLock.Exit();
                }

                #endregion


            });
        }

        #endregion

        #region protected virtual OnBestSolutionFound

        protected virtual void OnBestSolutionFound(BestSolutionEventArg args)
        {
            EventHandler<BestSolutionEventArg> handler = BestSolutionFound;
            handler?.Invoke(this, args);
        }

        #endregion


    }
}
