using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Torun.Settings.Enumeration;
using Client.Torun.Shared.DTOs;
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
        private List<string> _doctorIds;
        private List<DutyRequirementForMonthDto> _dutyRequirementsForMonth;
        private static string _auth;

        public DutyRequirements(string authorizationToken, DateTime date) : base()
        {
            if (string.IsNullOrEmpty(_auth))
            {
                _auth = authorizationToken;
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_auth}");
            }
            else
            {
                if (string.Equals(_auth, authorizationToken, StringComparison.Ordinal) == false)
                {
                    _auth = authorizationToken;
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                    HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_auth}");
                }
            }

            _date = date;

        }

        public override int CalculateGeneralCost(Individual individual)
        {
            int numberOfMatrixDutyRequirements = MatrixAssignedDutyRequirements(individual.Dna).Sum();
            int numberOfRequiredDuties = _dutyRequirementsForMonth.Select(d => d.RequiredTotalDutiesInMonth).Sum();

            return (numberOfRequiredDuties - numberOfMatrixDutyRequirements);
        }

        public override int CalculateLocalCost(IAllel[] rowOfAlleles, int index)
        {
            string doctor = _doctorIds[index];
            DutyRequirementForMonthDto dutyRequirement = _dutyRequirementsForMonth.FirstOrDefault(d => d.UserId == doctor);

            if ( dutyRequirement == null)
            {
                return 0;
            }

            int numberOfRequiredDuties = dutyRequirement.RequiredTotalDutiesInMonth;
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
                string doctor = _doctorIds[i];
                DutyRequirementForMonthDto dutyRequirement = _dutyRequirementsForMonth
                    .FirstOrDefault(d => d.UserId == doctor);

                if (dutyRequirement == null)
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

        private async Task<List<string>> GetDoctorIds()
        {
            string url = @"http://localhost:51301/originator/doctor-ids";
            

            var response = await HttpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            List<string> doctorIds = JsonConvert.DeserializeObject<List<string>>(content);

            return doctorIds;
        }

        private async Task<List<DutyRequirementForMonthDto>> GetDutyRequirementsForMonth()
        {
            string url = @"http://localhost:51301/originator/duty-requirements-for-month?year=" + _date.Year +
                         "&month=" + _date.Month;
            

            var response = await HttpClient.GetAsync(url);
            
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            List<DutyRequirementForMonthDto> dutyRequirements =
                JsonConvert.DeserializeObject<List<DutyRequirementForMonthDto>>(content);

            return dutyRequirements;
        }
    }
}
