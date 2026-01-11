using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ITourLocationRepository : IGenericRepository<TourLocation>
    {
        Task<List<TourLocation>> GetAllAsync(Expression<Func<TourLocation, bool>> predicate, string includeProperties = "");


    }
}
