using Common.DTO;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Implement
{

   public class RequestOfCustomerRepository : IRequestOfCustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public RequestOfCustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RequestOfCustomer> CreateAsync(RequestOfCustomer request)
        {
            _context.RequestOfCustomer.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }
        public async Task<List<RequestOfCustomerWithNameDTO>> GetAllRequestsWithNameAsync()
        {
            return await _context.RequestOfCustomer
                .Include(r => r.Customer)
                .Select(r => new RequestOfCustomerWithNameDTO
                {
                    RequestId = r.RequestId,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer != null ? r.Customer.UserName : "",

                    Destination = r.Destination,
                    DepartureLocation = r.DepartureLocation,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,

                    NumberOfPeople = r.NumberOfPeople,
                    Requirements = r.Requirements,
                    BudgetRange = r.BudgetRange,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,

                })
                .ToListAsync();
        }
    }
}
