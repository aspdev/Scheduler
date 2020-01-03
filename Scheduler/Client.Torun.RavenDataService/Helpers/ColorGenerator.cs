using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.Helpers
{
    public class ColorGenerator : IAsyncInitializer
    {
        private readonly IDocumentStore _clientStore;

        public ColorGenerator(ClientDocumentStoreHolder clientDocumentStoreHolder)
        {
            _clientStore = clientDocumentStoreHolder.Store;
        }

        public string Name => nameof(ColorGenerator);

        public async Task InitializeAsync()
        {
            await InitColors();
        }

        private async Task InitColors()
        {
            using (var clientSession = _clientStore.OpenAsyncSession())
            {
                if (await clientSession.Query<ClientColor>().AnyAsync())
                {
                    return;
                }
                
                List<string> knownColors = new List<string>();
                var lightnessLimit = 220 * 3;
                var darknessLimit = 40 * 3;
            
                for (int i = 28; i < 168; i++)
                {
                    var knownColor = Color.FromKnownColor((KnownColor) i);

                    if (knownColor.R + knownColor.G + knownColor.B <= lightnessLimit
                        && knownColor.R + knownColor.G + knownColor.B >= darknessLimit)
                    {
                        knownColors.Add(knownColor.Name);
                    }
                }

                foreach (var knownColor in knownColors)
                {
                    var clientColor = new ClientColor
                    {
                        Name = knownColor
                    };
                    
                    await clientSession.StoreAsync(clientColor);
                }
                
                await clientSession.SaveChangesAsync();  
            }
        }
    }
}