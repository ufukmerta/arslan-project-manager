using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    /// <summary>
    /// Seeds initial lookup / system data into the database (board tags, log categories, task categories, roles).
    /// Safe to run multiple times; it only inserts data when the corresponding tables are empty.
    /// </summary>
    public interface IDatabaseSeederService
    {
        Task SeedAsync();
    }
}

