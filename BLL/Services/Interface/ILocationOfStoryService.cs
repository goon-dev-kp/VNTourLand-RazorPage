using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.DTO;

namespace BLL.Services.Interface
{
    public interface ILocationOfStoryService
    {
        Task<ResponseDTO> GetAllLocationsWithStoriesAsync();
        Task<ResponseDTO> CreateLocationAsync(LocationOfStoryCreateDTO dto);
        Task<ResponseDTO> DeleteLocationAsync(Guid locationId);
        Task<ResponseDTO> UpdateLocationAsync (LocationOfStoryUpdateDTO dto);
    }
}
