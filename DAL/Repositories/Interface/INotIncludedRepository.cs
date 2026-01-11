using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface INotIncludedRepository : IGenericRepository<NotIncluded>
    {
        Task<List<NotIncluded>> GetAllByTourIdAsync(Guid tourId);
    }
}
