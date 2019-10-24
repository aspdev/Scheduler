using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IDocumentStore _store;
        private readonly IMapper _mapper;

        public UserController(IDocumentStoreHolder documentStoreHolder, IMapper mapper)
        {
            _store = documentStoreHolder.Store;
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

            using (var session = _store.OpenAsyncSession())
            {
                var dutyRequirement = _mapper.Map<DutyRequirement>(requirementToSet);
                await session.StoreAsync(dutyRequirement);
                await session.SaveChangesAsync();
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

            using (var session = _store.OpenAsyncSession())
            {
                var requirement = await session.LoadAsync<DutyRequirement>(requirementToUpdate.Id);
                requirement.RequiredTotalDutiesInMonth = requirementToUpdate.RequiredTotalDutiesInMonth;
                requirement.TotalHolidayDuties = requirementToUpdate.TotalHolidayDuties;
                requirement.TotalWeekdayDuties = requirementToUpdate.TotalWeekdayDuties;
                await session.StoreAsync(requirement);
                await session.SaveChangesAsync();

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

            using (var session = _store.OpenAsyncSession())
            {
                if (await session.Query<DutyRequirement>().AnyAsync())
                {
                    var requirement = await session.Query<DutyRequirement>()
                        .Where(userRequirement => userRequirement.UserId == isRequirementSetParams.UserId
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
    }
}
