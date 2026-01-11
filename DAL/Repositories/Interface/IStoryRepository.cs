using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IStoryRepository : IGenericRepository<Story>
    {
        Task<bool> DeleteStoryAsync(Guid storyId);
        Task<List<Story>> GetStoriesByLocationNameAsync(string locationName);

    }
}
