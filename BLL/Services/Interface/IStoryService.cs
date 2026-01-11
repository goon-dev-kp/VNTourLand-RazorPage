using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface IStoryService
    {
        Task<ResponseDTO> CreateStoryAsync(StoryCreateDTO dto);
        Task<ResponseDTO> DeleteStoryAsync(Guid storyId);
        Task<ResponseDTO> GetStoryByIdAsync(Guid storyId);
        Task<ResponseDTO> GetStoriesByLocationNameAsync(string locationName);

        Task<ResponseDTO> UpdateStoryAsync(StoryEditDTO storyEditDTO);

    }
}
