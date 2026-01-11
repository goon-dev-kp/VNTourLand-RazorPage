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
    public class BlogRepository : GenericRepository<Blog>, IBlogRepository
    {
        public readonly ApplicationDbContext _context;
        public BlogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Blog> GetAllIncludeReviewers()
        {
            return _context.Blogs.Include(b => b.Reviewers);
        }

    }
}
