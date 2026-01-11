using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Transaction> GetFirstOrDefaultAsync(Expression<Func<Transaction, bool>> predicate)
        {
            return await _context.Transactions.FirstOrDefaultAsync(predicate);
        }
        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                        .Include(t => t.Booking)
                            .ThenInclude(b => b.Tour)
                        .ToListAsync();
        }

    }

}

