using BLL.Services.Implement;
using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IIUserService
    {
        Task<List<UserDTO>> GetAllUserAsync();
        Task<bool> ChangeUserStatusAsync(Guid userId, bool isActive);
        Task<ResponseDTO<UserDTO>> UpdateUserAsync(UserDTO userDto);
        Task<ResponseDTO> GetAllManagerAndSeller();
        Task<ResponseDTO> GetAllSeller();
        Task<ResponseDTO> DeleteUserAsync(Guid userId);
        Task<ResponseDTO> GetUserById();


    }
}
