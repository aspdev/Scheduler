using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.RavenDataService.Config;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.DataStore
{
    public class DocumentStoreHolder : IDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public DocumentStoreHolder(DataServiceConfiguration configuration)
        {
            var store = new DocumentStore
            {
                Urls = configuration.Urls,
                Database = configuration.Database
            };

            Store = store.Initialize();

        }
    }
}
