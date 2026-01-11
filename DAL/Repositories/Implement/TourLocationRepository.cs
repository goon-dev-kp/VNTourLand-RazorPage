using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class TourLocationRepository : GenericRepository<TourLocation>, ITourLocationRepository
    {
        private readonly ApplicationDbContext _context;
        public TourLocationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<TourLocation>> GetAllAsync(
    Expression<Func<TourLocation, bool>> predicate,
    string includeProperties = "")
        {
            IQueryable<TourLocation> query = _context.TourLocations;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.Where(predicate).ToListAsync();
        }


    }
}
