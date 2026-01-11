using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface INotIncludeService
    {
        Task<ResponseDTO> GetAllByTourId(Guid tourId);
    }
}
