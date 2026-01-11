
using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface ITourService
    {
        Task<ResponseDTO> GetAllTour();
        Task<ResponseDTO> GetAllTourByType(string type);
        Task<ResponseDTO> CreateTourAsync(CreateTourRequest request);
        Task<ResponseDTO> CreateTourSellerAsync(CreateTourSellerRequest request);

        
        Task<ResponseDTO> GetTourDetailByIdAsync(Guid tourId);

        Task<List<TourLocationDTO>> GetToursNearAsync(double lat, double lng, double radiusKm);

        Task<ResponseDTO> UpdateTourByIdAsync(UpdateTourRequest request);
        Task<ResponseDTO> EnableTourAsync(Guid tourId);
        Task<ResponseDTO> DisableTourAsync(Guid tourId);
        Task<ResponseDTO> GetTourByUserId();

        Task<ResponseDTO> DeleteTourAsync(Guid tourId);

    }
    
}

