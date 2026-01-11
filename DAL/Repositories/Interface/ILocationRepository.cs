using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<List<Location>> GetAllAsync(Expression<Func<Location, bool>> predicate);


    }
}
