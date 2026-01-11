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
    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public readonly ApplicationDbContext _context;
        public LocationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetAllAsync(Expression<Func<Location, bool>> predicate)
        {
            return await _context.Locations.Where(predicate).ToListAsync();
        }


    }
}
