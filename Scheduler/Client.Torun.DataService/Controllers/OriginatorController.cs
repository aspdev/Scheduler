using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.DataService.Config;
using Client.Torun.DataService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Client.Torun.DataService.Controllers
{
    [Route("originator")]
    public class OriginatorController : Controller
    {
        private readonly IRepository _repository;
        private readonly DataServiceConfiguration _configuration;

        public OriginatorController(IRepository repository, DataServiceConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpGet("number-of-doctors-on-duty")]
        public async Task<IActionResult> GetNumberOfDoctorsOnDuty()
        {
            int numberOfDocs = await _repository.GetNumberOfDoctorsOnDuty();

            return Ok(numberOfDocs);
        }

        [HttpGet("population-size")]
        public IActionResult GetPopulationSize()
        {
            int populationSize =  _configuration.PopulationSize;

            return Ok(populationSize);
        }

        [HttpGet("total-number-of-doctors")]
        public async Task<IActionResult> GetTotalNumberOfDoctors()
        {
            int totalNumberOfDoctors = await _repository.GetTotalNumberOfDoctors();

            return Ok(totalNumberOfDoctors);
        }

        [HttpGet("doctor-ids")]
        public async Task<IActionResult> GetDoctorIds()
        {
            List<Guid> doctorIds = await _repository.GetDoctorIds();

            return Ok(doctorIds);
        }

        [HttpGet("days-off")]
        public async Task<IActionResult> GetDaysOffForMonth([FromQuery] DateTime? dateFilter)
        {
            if (dateFilter == null)
            {
                return BadRequest();
            }

            List<Tuple<Guid, DateTime>> daysOff = await _repository.GetDaysOffForMonth(dateFilter.Value);

            return Ok(daysOff);

        }

        [HttpGet("public-holidays")]
        public IActionResult GetPublicHolidaysForMonth([FromQuery] int? year, [FromQuery] int? month)
        {
            if (year == null || month == null)
            {
                return BadRequest(new { error = "Query string parameters are required."});
            }

            if(year < 0 || (month > 12 || month < 1))
            {
                return BadRequest();
            }

            var publicHolidays = _repository.GetPublicHolidays(year.Value, month.Value);

            return Ok(publicHolidays);
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

            var dutyRequirements = await _repository.GetDutyRequirementsForMonth(year.Value, month.Value);

            return Ok(dutyRequirements);

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

            var doctors = await _repository.GetDoctorsOnDutyLastDayOfPreviousMonth(year.Value, month.Value);

            return Ok(doctors);


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

            var daysOff = await _repository.GetDaysOffOnLastDayOfPreviousMonth(year.Value, month.Value);

            return Ok(daysOff);
        }

        [HttpGet("doctordtos")]
        public async Task<IActionResult> GetDoctorDtos()
        {
            var doctors = await _repository.GetDoctorDtos();

            return Ok(doctors);
        }


    }
}

