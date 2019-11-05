using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;

namespace Client.Torun.RavenDataService.Controllers
{
    [Authorize]
    public class DutyController : Controller
    {
        private readonly IDocumentStore _store;

        public DutyController(IDocumentStoreHolder storeHolder)
        {
            _store = storeHolder.Store ;
        }

        [HttpPost("duties")]
        public async Task<IActionResult> SaveDuties([FromBody] List<Duty> duties)
        {
            if (duties == null)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                var date = duties.First().Date;
                var dutyFromDatabase = await session.Query<Duty>().FirstOrDefaultAsync(d => d.Date.Year == date.Year && d.Date.Month == date.Month);

                var message = "";

                if (dutyFromDatabase != null)
                {
                    message = $"Schedule for {date:Y} already exists. Do you want to overwrite it?";
                    
                    return Ok(new {message});

                }
                
                foreach(var duty in duties)
                {
                    await session.StoreAsync(duty);
                }

                await session.SaveChangesAsync();
                

                return Ok(new {message});
            }
        }

        [HttpPost("duties/overwrite")]
        public async Task<IActionResult> OverwriteSchedule([FromBody] List<Duty> duties)
        {
            if(duties == null)
            {
                return BadRequest();
            }

            var date = duties.First().Date;

            var startDate = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-ddT00:00:00.0000000");
            var endDate = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)).ToString("yyyy-MM-ddT00:00:00.0000000");

            var operation =  await _store.Operations
                .SendAsync(new DeleteByQueryOperation(new IndexQuery
                {
                    Query = $"from Duties where Date >= '{startDate}' and Date <= '{endDate}'"
                }));

            await operation.WaitForCompletionAsync(TimeSpan.FromSeconds(15));

            using (var session = _store.OpenAsyncSession())
            {
                foreach (var duty in duties)
                {
                    await session.StoreAsync(duty);
                }

                await session.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("duties/month")]
        public async Task<IActionResult> GetDuties([FromQuery] string currentDate)
        {
            if (currentDate is null)
            {
                return BadRequest();
            }

            if (DateTime.TryParse(currentDate, out var parsedCurrentDate) == false)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                var dutiesForCurrentDateLazy = session.Query<Duty>().Where(duty => duty.Date.Year == parsedCurrentDate.Year
                                                    && duty.Date.Month == parsedCurrentDate.Month).LazilyAsync();

                var usersLazy = session.Query<User>().LazilyAsync();

                var dutiesForCurrentDate = await dutiesForCurrentDateLazy.Value as List<Duty>;
                var users = await usersLazy.Value as List<User>;

                if (users is null)
                {
                    throw new ArgumentException("Cannot assign duties while User collection is empty");
                }

                if (dutiesForCurrentDate is null || dutiesForCurrentDate.Count == 0)
                {
                    return NoContent();
                }
                
                var listOfDutiesForMonth = new List<DutyForMonthDto>();

                foreach (var duty in dutiesForCurrentDate)
                {
                    var doctor =  users.FirstOrDefault(user => user.Id == duty.UserId);

                    var dutyForMonth = new DutyForMonthDto
                    {
                        Name = doctor is null ? "User deleted" : $"{doctor.FirstName} {doctor.LastName}",
                        Date = duty.Date.ToString("yyyy-MM-dd"),
                        DoctorId = duty.UserId,
                        DutyId = duty.Id
                    };

                    listOfDutiesForMonth.Add(dutyForMonth);
                }

                return Ok(listOfDutiesForMonth);
            }
        }

        [HttpDelete("duties")]
        public async Task<IActionResult> DeleteDuty([FromQuery] string dutyId)
        {
            if (dutyId is null)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                session.Delete(dutyId);
                await session.SaveChangesAsync();

                return Ok();
            }
        }

        [HttpPost("duties/dutytosave")]
        public async Task<IActionResult> SaveDuty([FromBody] DutyToSaveDto dutyToSave)
        {
            if (dutyToSave is null)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                var result = DateTime.TryParse(dutyToSave.Date, out var date);

                if (result == false)
                {
                    return BadRequest();
                }

                var duty = new Duty
                {
                    Date = date,
                    UserId = dutyToSave.DoctorId
                };

                await session.StoreAsync(duty);
                await session.SaveChangesAsync();

                var savedDutyToReturnDto = new SavedDutyToReturnDto
                {
                    Date = duty.Date.ToString("yyyy-MM-dd"),
                    DoctorId = duty.UserId,
                    DutyId = duty.Id
                };

                return Ok(savedDutyToReturnDto);
            }
        }

        [HttpPut("duties/dutytoupdate")]
        public async Task<IActionResult> UpdateDuty([FromBody] DutyToUpdate dutyToUpdate)
        {
            if (dutyToUpdate is null)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                var duty = await session.LoadAsync<Duty>(dutyToUpdate.DutyId);

                duty.Date = DateTime.Parse(dutyToUpdate.NewDate);

                await session.SaveChangesAsync();

                var updatedDutyToReturnDto = new UpdatedDutyToReturnDto()
                {
                    Date = duty.Date.ToString("yyyy-MM-dd"),
                    DoctorId = duty.UserId,
                    DutyId = duty.Id
                };

                return Ok(updatedDutyToReturnDto);
            }
        }

        [HttpPost("duties/doctor")]
        public async Task<IActionResult> GetDutiesForDoctor([FromBody]ParamsToGetDutiesForDoctorDto paramsToGetDutiesForDoctor)
        {
            if (paramsToGetDutiesForDoctor is null)
            {
                return BadRequest();
            }

            DateTime.TryParse(paramsToGetDutiesForDoctor.Date, out var dateFromParams);

            if (dateFromParams == default)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {

                var dutiesLazy = session.Query<Duty>()
                    .Where(duty => duty.UserId == paramsToGetDutiesForDoctor.DoctorId &&
                                   duty.Date.Year == dateFromParams.Year &&
                                   duty.Date.Month == dateFromParams.Month)
                    .LazilyAsync();

                var doctorLazy = session.Advanced.Lazily.LoadAsync<User>(paramsToGetDutiesForDoctor.DoctorId);

                var duties = (List<Duty>) await dutiesLazy.Value;
                var doctor = await doctorLazy.Value;

                var dutiesForDoctor = new List<DutyForDoctorDto>();

                if (duties.Any())
                {
                    dutiesForDoctor = duties.Select(duty => new DutyForDoctorDto
                    {
                        Name = $"{doctor.FirstName} {doctor.LastName}",
                        Date = duty.Date.ToString("yyyy-MM-dd")
                    }).ToList();
                }

                return Ok(dutiesForDoctor);
            }
        }
    }
}
