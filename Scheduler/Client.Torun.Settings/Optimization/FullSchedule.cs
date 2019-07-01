using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Client.Torun.Settings.Enumeration;
using Newtonsoft.Json;
using Scheduler.Shared.Abstract;
using Scheduler.Shared.Classes;
using Scheduler.Shared.Interfaces;

namespace Client.Torun.Settings.Optimization
{
    public class FullSchedule : Requirement
    {
        private readonly DateTime _date;
        private static readonly HttpClient HttpClient = new HttpClient();
        private int _numberOfDoctorsOnDuty;
        


        public FullSchedule(DateTime date) : base()
        {
            _date = date;
           
        }

        public override int CalculateGeneralCost(Individual individual)
        {
            var numberOfDutiesInMatrix = TestMatrixFullSchedule(individual.Dna).Sum();

            var targetNumberOfDuties = individual.Dna.GetLength(1) * _numberOfDoctorsOnDuty;

            return (targetNumberOfDuties - numberOfDutiesInMatrix);


        }

        public override int CalculateLocalCost(IAllel[] rowOfAlleles, int index)
        {
            return 0;
        }

        public override async Task SetDataFromRemoteApi()
        {
            _numberOfDoctorsOnDuty = await GetNumberOfDoctorsOnDuty();
        }

        private IEnumerable<int> TestMatrixFullSchedule(IAllel[,] dna)
        {

            for (int i = 0; i < dna.GetLength(0); i++)
            {
                for (int j = 0; j < dna.GetLength(1); j++)
                {
                    if (dna[i, j] != null && dna[i, j].Value == Allel.OnDuty.Value)
                    {
                        yield return 1;
                    }
                }
            }

        }

        private async Task<int> GetNumberOfDoctorsOnDuty()
        {
            string url = @"http://localhost:50451/originator/number-of-doctors-on-duty";

            var response = await HttpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            int numberOfDocs = JsonConvert.DeserializeObject<int>(content);

            return numberOfDocs;


        }
    }
}
