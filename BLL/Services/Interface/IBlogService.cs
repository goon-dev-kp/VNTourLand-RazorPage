using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IBlogService
    {
        Task<ResponseDTO> GetAllBlog();
        Task<ResponseDTO> CreateBlog(CreateBlogDTO createBlogDTO);
        Task<ResponseDTO> GetBlogByIdAsync(Guid blogId);

    }
}
