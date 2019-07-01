using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.DataStore
{
    public interface IDocumentStoreHolder
    {
        IDocumentStore Store { get; } 
    }
}
