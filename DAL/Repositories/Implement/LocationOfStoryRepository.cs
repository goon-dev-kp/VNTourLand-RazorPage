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
    public class LocationOfStoryRepository : GenericRepository<LocationOfStory>, ILocationOfStoryRepository
    {
        private readonly ApplicationDbContext _context;
        public LocationOfStoryRepository (ApplicationDbContext context) : base (context)
        {
            _context = context;
        }
        public async Task<List<LocationOfStory>> GetAllLocationsWithStoriesAsync()
        {
            return await _context.locationOfStories
                .Include(l => l.Stories)
                .ToListAsync();
        }

        public async Task<bool> DeleteLocationAsync(Guid locationId)
        {
            var entity = await _context.locationOfStories.FindAsync(locationId);
            if (entity == null)
                return false;

            // Nếu cần xóa luôn các Story liên quan (có cascade delete thì không cần)
            var relatedStories = _context.Stories.Where(s => s.LocationOfStoryId == locationId);
            _context.Stories.RemoveRange(relatedStories);

            _context.locationOfStories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
