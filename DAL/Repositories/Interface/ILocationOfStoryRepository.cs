using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ILocationOfStoryRepository : IGenericRepository<LocationOfStory>
    {
        Task<List<LocationOfStory>> GetAllLocationsWithStoriesAsync();
        Task<bool> DeleteLocationAsync(Guid locationId);
    }
}
