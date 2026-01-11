//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BLL.Services.Interface;
//using Common.DTO;
//using DAL.UnitOfWork;

//namespace BLL.Services.Implement
//{
//    public class AddOnOptionService : IAddOnOptionService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        public AddOnOptionService(IUnitOfWork unitOfWork)
//        {
//            _unitOfWork = unitOfWork;
//        }
//        public async Task<ResponseDTO> GetAllAsync()
//        {
//            var addOnOptions = _unitOfWork.AddOnOptionRepo.GetAll();
//            if (addOnOptions == null)
//            {
//                return new ResponseDTO ("No AddOnOptions found",400, false);
//            }
//            var addOnOptionDTOs = addOnOptions.Select(a => new AddOptionDTO
//            {
//                AddOnOptionId = a.AddOnOptionId,
//                Name = a.Name,
//                Description = a.Description,
//                Price = a.Price
//            }).ToList();

//            return new ResponseDTO("Get all AddOnOptions successfully", 200, true, addOnOptionDTOs);
//        }
//    }
//}

