using Raven.Client.Documents;

namespace IdentityServer.DataStore
{
    public class DocumentStoreHolder : IDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public DocumentStoreHolder()
        {
            var store = new DocumentStore
            {
                Urls = new[] { "http://127.0.0.1:8080", "http://127.0.0.2:8080",
                    "http://127.0.0.3:8080" },
                Database = "IdentityServerUsers"
            };

            Store = store.Initialize();
            
        }
       
        

        
    }
}
