using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class TourRepository : GenericRepository<Tour>, ITourRepository
    {
        public readonly ApplicationDbContext _context;
        public TourRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tour>> GetAllByType(TourType type)
        {
            return await _context.Tours
                .Where(t => t.Type == type)
                .Include(t => t.TourLocations)
                    .ThenInclude(tl => tl.Location)
                .Where(t => !t.Participants.Any()) // 👈 lọc tour không có participant
                .ToListAsync();
        }

        public async Task<Tour> GetTourDetailByIdAsync(Guid tourId)
        {
            return await _context.Tours
                .Where(t => t.TourId == tourId)
                .Include(t => t.TourLocations).ThenInclude(tl => tl.Location)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Tour>> GetAllWithLocationAsync()
        {
            return await _context.Tours
                .Include(t => t.TourLocations)
                    .ThenInclude(tl => tl.Location)
                .Where(t => !t.Participants.Any()) // 👈 lọc tour không có participant
                .ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetAllWithLocationAndParticipantsAsync()
        {
            return await _context.Tours
                .Include(t => t.TourLocations)
                    .ThenInclude(tl => tl.Location)
                .Include(t => t.Participants)
                .ToListAsync();
        }


    }
}
