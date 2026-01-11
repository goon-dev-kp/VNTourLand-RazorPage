using Common.DTO;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interface
{
    public interface IRequestOfCustomerRepository
    {
        Task<RequestOfCustomer> CreateAsync(RequestOfCustomer request);
        Task<List<RequestOfCustomerWithNameDTO>> GetAllRequestsWithNameAsync();
    }
}
