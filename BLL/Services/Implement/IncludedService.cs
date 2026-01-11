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
    public class IncludedService : IIncludeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public IncludedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> GetAllByTourId(Guid tourId)
        {
            try
            {
                var items = await _unitOfWork.IncludedRepo.GetAllByTourIdAsync(tourId);

                var result = items.Select(i => new IncludedDTO
                {
                    Description = i.Description
                }).ToList();

                return new ResponseDTO("Successfully retrieved the Included list", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving Included", 500, false, ex.Message);
            }
        }

    }
}
