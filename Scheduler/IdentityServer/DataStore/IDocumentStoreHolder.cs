using Raven.Client.Documents;

namespace IdentityServer.DataStore
{
    public interface IDocumentStoreHolder
    {
        IDocumentStore Store { get; }
    }
}
