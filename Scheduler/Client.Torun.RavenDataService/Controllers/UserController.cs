using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Client.Torun.RavenDataService.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IDocumentStore _clientStore;
        private readonly IMapper _mapper;

        public UserController(ClientDocumentStoreHolder clientDocumentStoreHolder,
            IMapper mapper)
        {
            _clientStore = clientDocumentStoreHolder.Store;
            _mapper = mapper;
        }

        [Route("requirements/set-new-requirement")]
        [HttpPost]
        public async Task<IActionResult> SetNewRequirement([FromBody] RequirementToSetDto requirementToSet)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                var dutyRequirement = _mapper.Map<DutyRequirement>(requirementToSet);
                await clientSession.StoreAsync(dutyRequirement);
                await clientSession.SaveChangesAsync();
                var requirementToReturn = _mapper.Map<PostCreationRequirementToReturnDto>(dutyRequirement);

                return Ok(requirementToReturn);
            }
        }

        [Route("requirements/update-requirement")]
        [HttpPut]
        public async Task<IActionResult> UpdateRequirement([FromBody] RequirementToUpdateDto requirementToUpdate)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                var requirement = await clientSession.LoadAsync<DutyRequirement>(requirementToUpdate.Id);
                requirement.RequiredTotalDutiesInMonth = requirementToUpdate.RequiredTotalDutiesInMonth;
                requirement.TotalHolidayDuties = requirementToUpdate.TotalHolidayDuties;
                requirement.TotalWeekdayDuties = requirementToUpdate.TotalWeekdayDuties;
                await clientSession.StoreAsync(requirement);
                await clientSession.SaveChangesAsync();

                return NoContent();
            }
        }
        

        [Route("requirements/is-requirement-set")]
        [HttpPost]
        public async Task<IActionResult> IsRequirementSet([FromBody] IsRequirementSetParamsDto isRequirementSetParams)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<DutyRequirement>().AnyAsync())
                {
                    var requirement = await Queryable.Where(clientSession.Query<DutyRequirement>(), userRequirement => userRequirement.UserId == isRequirementSetParams.UserId
                                              && userRequirement.Date.Year == isRequirementSetParams.Date.Value.Year
                                              && userRequirement.Date.Month == isRequirementSetParams.Date.Value.Month)
                        .FirstOrDefaultAsync();
                    

                    return requirement != null 
                        ? Ok(new {isRequirementSet = true, requirement.Id}) 
                        : Ok(new {isRequirementSet = false});
                }

                return Ok(new {isRequirementSet = false});
            }
        }

        [Route("day-off")]
        [HttpPost]
        public async Task<IActionResult> SetDayOff([FromBody] DayOffToSetDto dayOffToSet)
        {
            if (dayOffToSet is null)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
               var dayOffToSave = _mapper.Map<DayOff>(dayOffToSet);
               await clientSession.StoreAsync(dayOffToSave);
               await clientSession.SaveChangesAsync();

               var dayOffToReturn = _mapper.Map<DayOffToReturnDto>(dayOffToSave);

               return Ok(dayOffToReturn);
            }
        }

        [Route("days-off")]
        [HttpGet]
        public async Task<IActionResult> GetDaysOffForUser([FromQuery] DateTime currentViewDate, string userId)
        {
            if (currentViewDate == default || userId is null)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                var daysOff = await clientSession.Query<DayOff>()
                    .Where(dayOff => dayOff.Date.Year == currentViewDate.Date.Year
                                     && dayOff.Date.Month == currentViewDate.Date.Month
                                     && dayOff.UserId == userId)
                    .ToListAsync();

                var daysOffToReturn = _mapper.Map<IEnumerable<DayOffToReturnDto>>(daysOff);

                return Ok(daysOffToReturn);
            }
        }

        [Route("day-off")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDayOff([FromQuery] string dayOffId)
        {
            if (dayOffId is null)
            {
                return BadRequest();
            }

            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                clientSession.Delete(dayOffId);
                await clientSession.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
