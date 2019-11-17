using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Scheduler.Api.Models;
using Scheduler.Api.Utility;
using Scheduler.Engine.Genetics;
using Scheduler.Shared.Abstract;
using Scheduler.Shared.Interfaces;


namespace Scheduler.Api.Controllers
{
    [Authorize]
    public class SchedulerController : Controller
    {
        private readonly ILogger _logger;
        
        public SchedulerController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SchedulerController>();
        }

        [HttpPost("scheduler")]
        public async Task<IActionResult> GetSchedule([FromBody] ScheduleCreatorDto scheduleCreator)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                                      .Substring(6) + Path.DirectorySeparatorChar + scheduleCreator.AssemblyDirName;


            if (!scheduleCreator.Date.HasValue)
            {
                return BadRequest();
            }

            ISpeciesOriginator speciesOriginator = DynamicFactory.GetSpeciesOriginator(assemblyPath, scheduleCreator.Date.Value, scheduleCreator.Args);

            List<Requirement> requirements = DynamicFactory.GetRequirements(assemblyPath, scheduleCreator.Date.Value, scheduleCreator.Args);

            await speciesOriginator.SetDataFromCustomApi();

            foreach (var requirement in requirements)
            {
                await requirement.SetDataFromRemoteApi();
            }

            Population population = new Population(new CostCalculator(requirements), speciesOriginator);

            population.Initialize();

            population.CalculateGeneralCost();

            do
            {
                population.PerformNaturalSelection();

                population.CalculateGeneralCost();

              
            } while (population.NumberOfGenerations < 100);

            IAllel[,] solution = population.BestSolution.Dna;

            Dictionary<DateTime, object> data = speciesOriginator.CustomFormatSolution(solution, scheduleCreator.Date.Value);

            Dictionary<string, object>  dataToReturn = Converter.ConvertDateTimeToString(data);

            return Ok(dataToReturn);
                     
        }

        [HttpPost("pre-generation-checks")]
        public async Task<IActionResult> PerformPreGenerationChecks([FromBody] ScheduleCreatorDto scheduleCreator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                                      .Substring(6) + Path.DirectorySeparatorChar + scheduleCreator.AssemblyDirName;


            if (!scheduleCreator.Date.HasValue)
            {
                return BadRequest();
            }

            ISpeciesOriginator speciesOriginator = DynamicFactory.GetSpeciesOriginator(assemblyPath, scheduleCreator.Date.Value, scheduleCreator.Args);

            List<Requirement> requirements = DynamicFactory.GetRequirements(assemblyPath, scheduleCreator.Date.Value, scheduleCreator.Args);

            await speciesOriginator.SetDataFromCustomApi();

            var messenger = speciesOriginator.PerformPreGenerationChecks();

            return Ok(messenger.Messages);
        }


    }
}
