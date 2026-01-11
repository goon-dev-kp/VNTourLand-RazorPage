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
    public class ReviewerRepository : GenericRepository<Reviewer>, IReviewerRepository
    {
        public readonly ApplicationDbContext _context;
        public ReviewerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<Reviewer>> GetAllByTourIdAsync(Guid tourId)
        {
            return await _context.Reviewers
                .Include(r => r.User)
                .Where(r => r.TourId == tourId)
                .ToListAsync();
        }

        public async Task<List<Reviewer>> GetAllWithBlogId(Guid blogId)
        {
            return await _context.Reviewers
                .Where(r => r.BlogId == blogId)
                .Include (r => r.User)
                .ToListAsync();
        }
        public async Task<List<Reviewer>> GetAllWithStoryId(Guid storyId)
        {
            return await _context.Reviewers
                .Where(r => r.StoryId == storyId)
                .Include(r => r.User)
                .ToListAsync();
        }

    }

}
