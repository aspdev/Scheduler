using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Torun.Settings.Enumeration;
using Newtonsoft.Json;
using Scheduler.Shared.Abstract;
using Scheduler.Shared.Classes;
using Scheduler.Shared.Interfaces;

namespace Client.Torun.Settings.Optimization
{
    public class DutyRequirements : Requirement
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly DateTime _date;
        private List<Guid> _doctorIds;
        private List<KeyValuePair<Guid, Tuple<int, int, int>>> _dutyRequirementsForMonth;

        public DutyRequirements(DateTime date) : base()
        {
            _date = date;
        }

        public override int CalculateGeneralCost(Individual individual)
        {
            int numberOfMatrixDutyRequirements = MatrixAssignedDutyRequirements(individual.Dna).Sum();
            int numberOfRequiredDuties = _dutyRequirementsForMonth.Select(d => d.Value.Item1).Sum();

            return (numberOfRequiredDuties - numberOfMatrixDutyRequirements);
        }

        public override int CalculateLocalCost(IAllel[] rowOfAlleles, int index)
        {
            Guid doctor = _doctorIds[index];
            KeyValuePair<Guid, Tuple<int, int, int>> dutyRequirement = _dutyRequirementsForMonth.FirstOrDefault(d => d.Key == doctor);

            if (dutyRequirement.Equals(default(KeyValuePair<Guid, Tuple<int, int, int>>)))
            {
                return 0;
            }

            int numberOfRequiredDuties = dutyRequirement.Value.Item1;
            int numberOfAssignedDuties = rowOfAlleles.Count(a => a != null && a.Value == Allel.OnDuty.Value);

            return (numberOfRequiredDuties - numberOfAssignedDuties);


        }

        public override async Task SetDataFromRemoteApi()
        {
            var one = Task.Run(async () => _doctorIds = await GetDoctorIds());
            var two = Task.Run(async () => _dutyRequirementsForMonth = await GetDutyRequirementsForMonth());
            
            await Task.WhenAll(one, two);
        }

        private IEnumerable<int> MatrixAssignedDutyRequirements(IAllel[,] dna)
        {
            for (int i = 0; i < dna.GetLength(0); i++)
            {
                Guid doctor = _doctorIds[i];
                KeyValuePair<Guid, Tuple<int, int, int>> dutyRequirement = _dutyRequirementsForMonth
                    .FirstOrDefault(d => d.Key == doctor);

                if (dutyRequirement.Equals(default(KeyValuePair<Guid, Tuple<int, int, int>>)))
                {
                    continue;
                }

                for (int j = 0; j < dna.GetLength(1); j++)
                {
                    if (dna[i, j] != null && dna[i, j].Value == Allel.OnDuty.Value)
                    {
                        yield return 1;
                    }

                }

            }
        }

        private async Task<List<Guid>> GetDoctorIds()
        {
            string url = @"http://localhost:50451/originator/doctor-ids";

            var response = await HttpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            List<Guid> doctorIds = JsonConvert.DeserializeObject<List<Guid>>(content);

            return doctorIds;
        }

        private async Task<List<KeyValuePair<Guid, Tuple<int, int, int>>>> GetDutyRequirementsForMonth()
        {
            string url = @"http://localhost:50451/originator/duty-requirements-for-month?year=" + _date.Year +
                         "&month=" + _date.Month;

            var response = await HttpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            List<KeyValuePair<Guid, Tuple<int, int, int>>> dutyRequirements =
                JsonConvert.DeserializeObject<List<KeyValuePair<Guid, Tuple<int, int, int>>>>(content);

            return dutyRequirements;
        }
    }
}
