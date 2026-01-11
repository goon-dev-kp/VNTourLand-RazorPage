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
    public class NotIncludedRepository : GenericRepository<NotIncluded>, INotIncludedRepository
    {
        private readonly ApplicationDbContext _context;
        public NotIncludedRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<NotIncluded>> GetAllByTourIdAsync(Guid tourId)
        {
            return await _context.NotIncludedItems
                .Where(n => n.TourId == tourId)
                .ToListAsync();
        }

    }
}
