using Common.DTO;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IRequestOfCustomerService
    {
        Task<ResponseDTO> CreateRequestOfCustomerAsync(RequestOfCustomerDTO dto, Guid userId);
        Task<ResponseDTO> GetAllRequestsAsync();
        Task<ResponseDTO> ChangeStatusAsync(Guid requestId, RequestStatus newStatus);
    }
}
