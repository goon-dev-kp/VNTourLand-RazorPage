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
    public class StoryRepository : GenericRepository<Story>, IStoryRepository
    {
        private readonly ApplicationDbContext _context;
        public StoryRepository (ApplicationDbContext context) : base (context)
        {
            _context = context;
        }
        public async Task<bool> DeleteStoryAsync(Guid storyId)
        {
            var entity = await _context.Stories.FindAsync(storyId);
            if (entity == null)
                return false;

            _context.Stories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Story>> GetStoriesByLocationNameAsync(string locationName)
        {
            return await _context.Stories
                .Include(s => s.LocationOfStory)
                .Where(s =>
                    !string.IsNullOrEmpty(locationName) &&
                    s.LocationOfStory.LocationOfStoryName.ToLower().Contains(locationName.ToLower())
                )
                .ToListAsync();
        }


    }
}
