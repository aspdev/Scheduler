using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.Entities;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.Helpers
{
    public class ColorRetriever
    {
        public static async Task<List<ColorForUser>> RetrieveColorsForUsers(IDocumentStore clientStore)
        {
            using (var session = clientStore.OpenAsyncSession())
            {
                var colorsForUsers = await session.Query<ClientColor>()
                    .Where(c => c.UserId != null)
                    .Select(c => new ColorForUser {ColorName = c.Name, UserId = c.UserId})
                    .ToListAsync();

                return colorsForUsers;
            }
        }
    }

    public class ColorForUser
    {
        public string ColorName { get; set; }
        public string UserId { get; set; }
    }
}