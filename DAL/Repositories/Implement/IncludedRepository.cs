using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class IncludedRepository : GenericRepository<Included>, IIncludedRepository
    {
        private readonly ApplicationDbContext _context;
        public IncludedRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Included>> GetAllByTourIdAsync(Guid tourId)
        {
            return await _context.IncludedItems
                .Where(i => i.TourId == tourId)
                .ToListAsync();
        }
    }
}
