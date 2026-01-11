using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;

namespace DAL.Repositories.Implement
{
    public class BlogCategoryRepository : GenericRepository<BlogCategory>, IBlogCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogCategoryRepository (ApplicationDbContext context) : base (context)
        {
            _context = context;
        }
    }
}
