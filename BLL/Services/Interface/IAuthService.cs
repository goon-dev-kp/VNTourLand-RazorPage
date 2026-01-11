using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using static Common.DTO.AuthDTO;

namespace BLL.Services.Interface
{
    public interface IAuthService
    {
        Task<ResponseDTO> Login(LoginDTO loginDTO);
        Task<ResponseDTO> Register(RegisterDTO registerDTO);
        Task<ResponseDTO> Logout();
        Task<ResponseDTO> RegisterSeller(RegisterForBoss registerDTO);
        Task<ResponseDTO> RegisterManager(RegisterForBoss registerDTO);
        Task<ResponseDTO> LoginWithGoogle(string googleEmail);
    }
}
