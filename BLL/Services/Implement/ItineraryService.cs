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
    public class ItineraryService : IItineraryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItineraryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> GetAllItinerariesByTourId(Guid tourId)
        {
            try
            {
                var itineraries = await _unitOfWork.ItineraryRepo.GetItinerariesByTourId(tourId);

                var result = itineraries.Select(i => new ItineraryDTO
                {
                    Name = i.Name,
                    StartTime = i.StartTime,
                    EndTime = i.EndTime,
                    Activities = i.Activities.Select(a => new ActivityDTO
                    {
                        Description = a.Description,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    }).ToList()
                }).ToList();

                return new ResponseDTO("Successfully retrieved the itinerary list", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving the itinerary", 500, false, ex.Message);
            }
        }

    }
}
