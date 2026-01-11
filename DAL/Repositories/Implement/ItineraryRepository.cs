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
    public class ItineraryRepository : GenericRepository<Itinerary>, IItineraryRepository 
    {
        private readonly ApplicationDbContext _context;
        public ItineraryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Itinerary>> GetItinerariesByTourId(Guid tourId)
        {
            return await _context.Itineraries
                .Where(i => i.TourId == tourId)
                .Include(i => i.Activities)
                .ToListAsync();
        }
    }
}
