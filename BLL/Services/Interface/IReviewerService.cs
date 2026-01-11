using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface IReviewerService
    {
        Task<ResponseDTO> GetAllByTourId(Guid tourId);
        Task<ResponseDTO> CreateReviewerByBlogIdAsync(CreateReviewerDTO reviewerDto);
        Task<ResponseDTO> GetReviewerByBlogId(Guid blogId);
        Task<ResponseDTO> CreateReviewerByTourIdAsync(CreateReviewerDTO reviewerDto);
        Task<ResponseDTO> CreateReviewerByStoryIdAsync(CreateReviewerDTO reviewerDTO);
        Task<ResponseDTO> GetAllByStoryId(Guid storyId);

    }
}
