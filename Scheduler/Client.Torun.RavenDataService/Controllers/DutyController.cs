using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

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
