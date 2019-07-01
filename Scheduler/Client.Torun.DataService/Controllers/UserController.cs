using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.DataService.Config;
using Client.Torun.DataService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Client.Torun.DataService.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository _repository;
        private readonly DataServiceConfiguration _configuration;

        public UserController(IRepository repository, DataServiceConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        { 
        
            var connString = _configuration.ConnectionString;
            var popCount = _configuration.PopulationSize;
            var numOfDoctorsOnDuty = await _repository.GetNumberOfDoctorsOnDuty();
            
            
            return Ok(new {connectionString = connString,
                populationCount = popCount, numDocsOnDuty = numOfDoctorsOnDuty});
        }

    }
}
