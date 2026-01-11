using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IReviewerRepository : IGenericRepository<Reviewer>
    {
        Task<List<Reviewer>> GetAllByTourIdAsync(Guid tourId);
        Task<List<Reviewer>> GetAllWithStoryId(Guid storyId);
       

        Task<List<Reviewer>> GetAllWithBlogId(Guid blogId);
    }
}
