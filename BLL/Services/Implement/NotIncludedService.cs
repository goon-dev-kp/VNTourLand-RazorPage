using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using DAL.UnitOfWork;

namespace BLL.Services.Implement
{
    public class NotIncludedService : INotIncludeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotIncludedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> GetAllByTourId(Guid tourId)
        {
            try
            {
                var items = await _unitOfWork.NotIncludedRepo.GetAllByTourIdAsync(tourId);

                var result = items.Select(i => new NotIncludedDTO
                {
                    Description = i.Description
                }).ToList();

                return new ResponseDTO("Successfully retrieved the NotIncluded list", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving NotIncluded", 500, false, ex.Message);
            }
        }

    }
}
