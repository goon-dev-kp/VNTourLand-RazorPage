using BLL.Services.Interface;
using Common.DTO;
using Common.Enums;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implement
{
    public class RequestOfCustomerService : IRequestOfCustomerService
    {
        private readonly IRequestOfCustomerRepository _repository;
        private readonly ApplicationDbContext _context;

        public RequestOfCustomerService(IRequestOfCustomerRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<ResponseDTO> CreateRequestOfCustomerAsync(RequestOfCustomerDTO dto, Guid userId)
        {
            // Validate start date must be today or in the future
            if (dto.StartDate.HasValue && dto.StartDate.Value.Date < DateTime.UtcNow.Date)
            {
                return new ResponseDTO("Start date must be today or later.", 400, false, null);
            }

            // Validate end date must be after start date and today
            if (dto.EndDate.HasValue)
            {
                if (!dto.StartDate.HasValue)
                {
                    return new ResponseDTO("Start date must be specified before the end date.", 400, false, null);
                }
                if (dto.EndDate.Value.Date <= dto.StartDate.Value.Date)
                {
                    return new ResponseDTO("End date must be after the start date.", 400, false, null);
                }
                if (dto.EndDate.Value.Date <= DateTime.UtcNow.Date)
                {
                    return new ResponseDTO("End date must be in the future.", 400, false, null);
                }
            }

            // Check if user exists
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return new ResponseDTO("User not found.", 404, false, null);
            }

            // Convert DateTime to UTC explicitly
            var startDateUtc = dto.StartDate.HasValue
                ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            var endDateUtc = dto.EndDate.HasValue
                ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            // Map DTO to entity
            var entity = new RequestOfCustomer
            {
                RequestId = Guid.NewGuid(),
                CustomerId = userId,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Destination = dto.Destination,
                DepartureLocation = dto.DepartureLocation,
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                NumberOfPeople = dto.NumberOfPeople,
                Requirements = dto.Requirements,
                BudgetRange = dto.BudgetRange,
                Status = RequestStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                Customer = user
            };

            var created = await _repository.CreateAsync(entity);

            return new ResponseDTO("Request created successfully.", 200, true, user.UserName);
        }



        public async Task<ResponseDTO> GetAllRequestsAsync()
        {
            var requests = await _context.RequestOfCustomer
                .Include(r => r.Customer)
                .Select(r => new RequestOfCustomerWithNameDTO
                {
                    RequestId = r.RequestId,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.UserName,
                    Email = r.Email,
                    PhoneNumber = r.PhoneNumber,
                    Address = r.Address,
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

            return new ResponseDTO("Requests retrieved successfully.", 200, true, requests);
        }

        public async Task<ResponseDTO> ChangeStatusAsync(Guid requestId, RequestStatus newStatus)
        {
            var request = await _context.RequestOfCustomer.FindAsync(requestId);

            if (request == null)
            {
                return new ResponseDTO("Request not found.", 404, false, null);
            }

            request.Status = newStatus;
            _context.RequestOfCustomer.Update(request);
            await _context.SaveChangesAsync();

            return new ResponseDTO("Request status updated successfully.", 200, true);
        }

    }
}

