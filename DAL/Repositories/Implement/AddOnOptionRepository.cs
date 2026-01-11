//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using DAL.Data;
//using DAL.Models;
//using DAL.Repositories.Interface;
//using Microsoft.EntityFrameworkCore;

//namespace DAL.Repositories.Implement
//{
//    public class AddOnOptionRepository : GenericRepository<AddOnOption>, IAddOnOptionRepository
//    {
//        private readonly ApplicationDbContext _context;
//        public AddOnOptionRepository(ApplicationDbContext context) : base(context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<AddOnOption>> GetAsync(Expression<Func<AddOnOption, bool>> predicate)
//        {
//            return await _context.AddOnOptions.Where(predicate).ToListAsync();
//        }

//    }
//}
