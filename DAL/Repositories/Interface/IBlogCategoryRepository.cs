using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Repositories.Implement;

namespace DAL.Repositories.Interface
{
    public interface IBlogCategoryRepository : IGenericRepository<BlogCategory>
    {
    }
}
