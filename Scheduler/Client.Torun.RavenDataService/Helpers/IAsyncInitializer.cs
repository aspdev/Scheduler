using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Helpers
{
    public interface IAsyncInitializer
    {
        string Name { get; }
        Task InitializeAsync();
    }
}