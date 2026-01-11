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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<User> FindByEmailAsync(string email)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        // 1. Lấy tất cả Seller
        public async Task<List<User>> GetAllSeller()
        {
            return await _context.User
                .Where(u => u.Role == RoleType.SELLER || u.Role == RoleType.USER)
                .ToListAsync();
        }

        // 2. Lấy tất cả Manager hoặc Seller
        public async Task<List<User>> GetAllManagerAndSeller()
        {
            return await _context.User
                .Where(u => u.Role == RoleType.MANAGER|| u.Role == RoleType.SELLER || u.Role == RoleType.USER)
                .ToListAsync();
        }
    }

    
}
