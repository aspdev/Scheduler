using System.Collections.Generic;
using System.Linq;
using Scheduler.Engine.EventArgs;
using Scheduler.Engine.Utility;
using Scheduler.Shared.Classes;
using Scheduler.Shared.Interfaces;

namespace Scheduler.Engine.Genetics
{
    public class Population
    {
        private CostCalculator _costCalculator { get; }

        private readonly ISpeciesOriginator _speciesOriginator;

        public List<Individual> Individuals { get; set; } = new List<Individual>();

        public int NumberOfGenerations { get; set; } = 0;

        public Individual BestSolution { get; set; }


        #region Constructor

        public Population(CostCalculator costCalculator, ISpeciesOriginator speciesOriginatorOriginator)
        {
            this._costCalculator = costCalculator;
            this._speciesOriginator = speciesOriginatorOriginator;
            _costCalculator.BestSolutionFound += OnBestSolutionFound;

        }

        

        #endregion

        #region public Initialize

        public void Initialize()
        {
            Individuals = _speciesOriginator.Initialize().ToList();
        }

        #endregion

        #region public CalculateGeneralCost

        public void CalculateGeneralCost()
        {
            _costCalculator.CalculateGeneralCost(Individuals, NumberOfGenerations);

        }

        #endregion

        #region public PerformNaturalSelection

        public void PerformNaturalSelection()
        {
            List<Individual> nextGeneration = new List<Individual>();

            while (nextGeneration.Count < Individuals.Count)
            {

                Individual parentA = null;
                Individual parentB = null;

                for (int i = 0; i < 1000; i++)
                {
                    parentA = SelectMatingParent();
                    parentB = SelectMatingParent();

                    if (parentA != parentB)
                    {
                        break;
                    }

                }

                if (parentA == parentB)
                {
                    parentA = Individuals[0];
                    parentB = Individuals[Individuals.Count - 1];
                }


                Individual childA = new Individual();
                Individual childB = new Individual();

                CreateOffspring(parentA, parentB, out var firstChild, out var secondChild);

                childA.Dna = firstChild;
                childB.Dna = secondChild;

                nextGeneration.Add(childA);
                nextGeneration.Add(childB);

            }

            Individuals = nextGeneration;
            NumberOfGenerations++;
        }

        #endregion

        #region private SelectMatingParent

        private Individual SelectMatingParent()
        {

            for (int i = 0; i < Individuals.Count; i++)
            {
                int random = RandomGenerator.IntBetween(0, Individuals.Count);
                var parent = Individuals[random];
                double randomDouble = RandomGenerator.GetDoubleFromZeroToOne();

                if (parent.Fitness > randomDouble)
                {
                    return parent;
                }
            }

            return Individuals[RandomGenerator.IntBetween(0, Individuals.Count)];

        }

        #endregion

        #region private CreateOffspring

        private void CreateOffspring(Individual parentA, Individual parentB, out IAllel[,] firstChild, out IAllel[,] secondChild)
        {

            #region GetRowsFromParents

            parentA.IndexedRows.Clear();
            parentB.IndexedRows.Clear();

            int numberOfRows = parentA.Dna.GetLength(0);
            int numberOfColumns = parentA.Dna.GetLength(1);

            // czytamy poszczególne rzędy z rodzica A i rodzica B

            
            for (int i = 0; i < numberOfRows; i++)
            {

                IAllel[] rowOfAllelesA = new IAllel[numberOfColumns];
                IAllel[] rowOfAllelesB = new IAllel[numberOfColumns];

                for (int j = 0; j < parentA.Dna.GetLength(1); j++)
                {
                    rowOfAllelesA[j] = (IAllel)parentA.Dna.GetValue(i, j);
                    rowOfAllelesB[j] = (IAllel)parentB.Dna.GetValue(i, j);

                }

                // zapisujemy rząd z indeksem w słowniku (np. lekarz 0, dużury rowsOfAlleles A)

                parentA.IndexedRows.Add(i, rowOfAllelesA);
                parentB.IndexedRows.Add(i, rowOfAllelesB);


            }

            #endregion

            #region SortRowsAscendingAccordingToLocalCost

            // sortujemy według wartości lokalnego kosztu od najmniejszego do największego (domyślenie w .NET)

            parentA.SortedRows =
                parentA.IndexedRows.Select(kv => kv).OrderBy(kv =>
                {
                    int localCost = 0;
                    foreach (var requirement in _costCalculator.Requirements)
                    {
                        localCost += requirement.CalculateLocalCost(kv.Value, kv.Key);
                    }

                    return localCost;
                }).ToList();

            // sortujemy rzędy z drugiego rodzica

            parentB.SortedRows = parentB.IndexedRows.Select(kv => kv).OrderBy(kv =>
            {
                int localCost = 0;
                foreach (var requirement in _costCalculator.Requirements)
                {
                    localCost += requirement.CalculateLocalCost(kv.Value, kv.Key);
                }

                return localCost;
            }).ToList();

            #endregion

            #region Crossover

            int random = RandomGenerator.IntBetween(1, parentA.SortedRows.Count + 1);

            List<KeyValuePair<int, IAllel[]>> crossedGenesForChildA = parentA.SortedRows
                .Take(random)
                .ToList();

            List<KeyValuePair<int, IAllel[]>> crossedGenesForChildB = null;

            if (random < parentA.SortedRows.Count)
            {
                crossedGenesForChildB = parentA.SortedRows
                    .Skip(random).Take(parentA.SortedRows.Count - random).ToList();

                var crossedGenesAKeys = crossedGenesForChildA.Select(kv => kv.Key);
                crossedGenesForChildA.AddRange(parentB.SortedRows.FindAll(kv => !crossedGenesAKeys.Contains(kv.Key)));

                var crossedGenesBKeys = crossedGenesForChildB.Select(kv => kv.Key);
                crossedGenesForChildB.AddRange(parentB.SortedRows.FindAll(kv => !crossedGenesBKeys.Contains(kv.Key)));

            }
            else
            {
                crossedGenesForChildB =
                    parentB.SortedRows.Take(parentB.SortedRows.Count).ToList();
            }

            // po skrzyżowaniu genów układamy je ponownie i porządkujemy według indeksów rosnąco

            var orderedCrossedGenesForChildA = crossedGenesForChildA.OrderBy(kv => kv.Key).ToList();
            var orderedCrossedGenesForChildB = crossedGenesForChildB.OrderBy(kv => kv.Key).ToList();

            IAllel[,] childA = new IAllel[numberOfRows, numberOfColumns];
            IAllel[,] childB = new IAllel[numberOfRows, numberOfColumns];

            // wpisujemy geny z powrotem do tablicy 

            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfColumns; j++)
                {
                    IAllel[] arrayA = orderedCrossedGenesForChildA[i].Value;
                    IAllel[] arrayB = orderedCrossedGenesForChildB[i].Value;
                    childA.SetValue(arrayA[j], i, j);
                    childB.SetValue(arrayB[j], i, j);
                }
            }

            // przed zwróceniem naprawiamy ograniczenia pionowe 

            firstChild = _speciesOriginator.Repair(childA);
            secondChild = _speciesOriginator.Repair(childB);

            #endregion



        }

        #endregion

        #region OnBestSolutionFound

        private void OnBestSolutionFound(object sender, BestSolutionEventArg arg)
        {

            BestSolution = arg.BestSolution;
            //if (_speciesOriginator.TestForNumberOfDoctorsRequired(arg.BestSolution))
            //{
            //    BestSolution = arg.BestSolution;
            //}
        }

        #endregion

    }
}
