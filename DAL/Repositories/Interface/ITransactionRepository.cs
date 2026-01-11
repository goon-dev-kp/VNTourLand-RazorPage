using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<Transaction> GetFirstOrDefaultAsync(Expression<Func<Transaction, bool>> predicate);
        Task<List<Transaction>> GetAllAsync();
    }
}
