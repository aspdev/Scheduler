using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Client.Torun.RavenDataService.Helpers;
using Common;
using Microsoft.AspNetCore.Authorization;

namespace Client.Torun.RavenDataService.Controllers
{
    [Authorize]
    [Route("originator")]
    public class OriginatorController : Controller
    {
        private readonly DataServiceConfiguration _configuration;
        private readonly IDocumentStore _clientStore;
        private readonly IDocumentStore _identityStore;
        private readonly IMapper _mapper;

        public OriginatorController(DataServiceConfiguration configuration, 
            ClientDocumentStoreHolder clientStoreHolder,
            IdentityServerDocumentStoreHolder identityServerStoreHolder,
            IMapper mapper)
        {
            _configuration = configuration;
            _clientStore = clientStoreHolder.Store;
            _identityStore = identityServerStoreHolder.Store;
            _mapper = mapper;
        }

        [HttpGet("number-of-doctors-on-duty")]
        public async Task<IActionResult> GetNumberOfDoctorsOnDuty()
        {
            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                AdminSettings adminSettings = await clientSession.Query<AdminSettings>().FirstAsync();

                int numberOfDocs = adminSettings.NumberOfDoctorsOnDuty;

                return Ok(numberOfDocs);
            }
        }

        [HttpGet("population-size")]
        public IActionResult GetPopulationSize()
        {
            int populationSize = _configuration.PopulationSize;

            return Ok(populationSize);
        }

        [HttpGet("total-number-of-doctors")]
        public async Task<IActionResult> GetTotalNumberOfDoctors()
        {
            using (var identitySession = _identityStore.OpenAsyncSession())
            {
                int totalNumberOfDoctors = await identitySession.Query<User>()
                    .Where(u => u.Clients.Contains(ConstNames.TorunClientName))
                    .CountAsync();

                return Ok(totalNumberOfDoctors);
            }
        }

        [HttpGet("doctor-ids")]
        public async Task<IActionResult> GetDoctorIds()
        {
            using (var identitySession = _identityStore.OpenAsyncSession())
            {
                var doctorIds = await identitySession.Query<User>()
                    .Where(u => u.Clients.Contains(ConstNames.TorunClientName))
                    .Select(u => u.Id).ToListAsync();

                return Ok(doctorIds);
            }
        }

        [HttpGet("days-off")]
        public async Task<IActionResult> GetDaysOffForMonth([FromQuery] DateTime? dateFilter)
        {
            if (dateFilter == null)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<DayOff>().AnyAsync())
                {
                    List<DayOffDto> daysOff = await clientSession.Query<DayOff>()
                        .Where(d => d.Date.Year == dateFilter.Value.Year && d.Date.Month == dateFilter.Value.Month)
                        .Select(d => new DayOffDto { UserId = d.UserId, Date = d.Date }).ToListAsync();

                    return Ok(daysOff);
                }

                return Ok(new List<DayOffDto>());
            }
        }

        [HttpGet("public-holidays")]
        public IActionResult GetPublicHolidaysForMonth([FromQuery] int? year, [FromQuery] int? month)
        {
            if (year == null || month == null)
            {
                return BadRequest(new { error = "Query string parameters are required." });
            }

            if (year < 0 || (month > 12 || month < 1))
            {
                return BadRequest();
            }

            int currentYear = year.Value;
            int currentMonth = month.Value;

            List<DateTime> publicHolidays = new List<DateTime>()
            {
                new DateTime(currentYear, 1, 1), // Nowy Rok
                new DateTime(currentYear, 1, 6), // Trzech Króli
                new DateTime(currentYear, 5, 1), // Święto Pracy
                new DateTime(currentYear, 5, 3), // Święto Konstytucji
                new DateTime(currentYear, 8, 15), // Święto Wojska Polskiego
                new DateTime(currentYear, 11, 1), // Dzień Wszystkich Świętych
                new DateTime(currentYear, 11, 11), // Dzień Niepodległości
                new DateTime(currentYear, 12, 25), // Boże Narodzenie
                new DateTime(currentYear, 12, 26) // 2 dzień Świąt Bożego Narodzenia
            };

            DateTime easterMondayDate = CalculateEasterDate(currentYear).AddDays(1);
            DateTime corpusChristiDate = CalculateEasterDate(currentYear).AddDays(60);

            publicHolidays.Add(easterMondayDate);
            publicHolidays.Add(corpusChristiDate);

            var result = publicHolidays.Where(h => h.Year == currentYear && h.Month == currentMonth)
                .Select(h => h.ToShortDateString()).ToList();

            return Ok(result);
        }

        [HttpGet("duty-requirements-for-month")]
        public async Task<IActionResult> GetDutyRequirementsForMonth([FromQuery] int? year, [FromQuery] int? month)
        {
            if (year == null || month == null)
            {
                return BadRequest(new { error = "Query string parameters are required." });
            }

            if (year < 0 || (month > 12 || month < 1))
            {
                return BadRequest();
            }

            int currentYear = year.Value;
            int currentMonth = month.Value;

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<DutyRequirement>().AnyAsync())
                {
                    List<DutyRequirementForMonthDto> dutyRequirements = await clientSession.Query<DutyRequirement>()
                        .Where(dr => dr.Date.Year == currentYear && dr.Date.Month == currentMonth)
                        .Select(dr => new DutyRequirementForMonthDto
                        {
                            UserId = dr.UserId,
                            RequiredTotalDutiesInMonth = dr.RequiredTotalDutiesInMonth,
                            RequiredTotalHolidayDuties = dr.TotalHolidayDuties,
                            RequiredTotalWeekdayDuties = dr.TotalWeekdayDuties
                        }).ToListAsync();

                    return Ok(dutyRequirements);
                }

                return Ok(new List<DutyRequirementForMonthDto>());
            }
        }

        [HttpGet("doctors-on-duty-on-last-day-previous-month")]
        public async Task<IActionResult> GetDoctorsOnDutyLastDayOfPreviousMonth([FromQuery] int? year,
            [FromQuery] int? month)
        {
            if (year == null || month == null)
            {
                return BadRequest(new { error = "Query string parameters are required." });
            }

            if (year < 0 || (month > 12 || month < 1))
            {
                return BadRequest();
            }

            DateTime previousMonth = new DateTime(year.Value, month.Value, 1).AddMonths(-1);

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<Duty>().AnyAsync())
                {
                    var doctors = await clientSession.Query<Duty>()
                        .Where(d => d.Date.Year == previousMonth.Year &&
                                    d.Date.Month == previousMonth.Month &&
                                    d.Date.Day == DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month))
                        .Select(d => d.UserId).ToListAsync();

                    return Ok(doctors);
                }

                return Ok(new List<string>());
            }
        }

        [HttpGet("days-off-on-last-day-previous-month")]
        public async Task<IActionResult> GetDaysOffOnLastDayOfPreviousMonth([FromQuery] int? year,
            [FromQuery] int? month)
        {
            if (year == null || month == null)
            {
                return BadRequest(new { error = "Query string parameters are required." });
            }

            if (year < 0 || (month > 12 || month < 1))
            {
                return BadRequest();
            }

            DateTime previousMonthDate = new DateTime(year.Value, month.Value, 1).AddMonths(-1);
            int lastDay = DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month);

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<DayOff>().AnyAsync())
                {
                    List<DayOffOnLastDayPreviousMonthDto> daysOff = await clientSession.Query<DayOff>()
                        .Where(d => d.Date.Year == previousMonthDate.Year && d.Date.Month == previousMonthDate.Month &&
                                    d.Date.Day == lastDay)
                        .Select(d => new DayOffOnLastDayPreviousMonthDto { UserId = d.UserId, Date = d.Date })
                        .ToListAsync();

                    return Ok(daysOff);
                }

                return Ok(new List<DayOffOnLastDayPreviousMonthDto>());
            }
        }

        [HttpGet("doctordtos")]
        public async Task<IActionResult> GetDoctorDtos()
        {
            using (var identitySession = _identityStore.OpenAsyncSession())
            {
                if (await identitySession.Query<User>().AnyAsync())
                {
                    var users = await identitySession.Query<User>().ToListAsync();

                    var doctorDtos = _mapper.Map<List<DoctorDto>>(users);
                    
                    

                    return Ok(doctorDtos);
                }
                
                return Ok(new List<DoctorDto>());
            }
        }
        
        private DateTime CalculateEasterDate(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }
    }
}
