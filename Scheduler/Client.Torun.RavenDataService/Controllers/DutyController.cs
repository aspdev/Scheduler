using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Exceptions.Documents.Indexes;

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
    }
}
