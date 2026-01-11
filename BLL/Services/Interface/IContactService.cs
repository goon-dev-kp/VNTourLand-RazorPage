using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface IContactService
    {
        public Task<ResponseDTO> CreateContact(CreateContactDTO contactDTO);
        public Task<ResponseDTO> GetAllContacts();
    }
}
