using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implement
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BlogCategoryService (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> GetAllCategoryAsync()
        {
            var categories = _unitOfWork.BlogCategoryRepo.GetAll();



            var result = categories.Select(c => new CategoryDTO
            {
                CategoryId = c.BlogCategoryId,
                Name = c.CategoryName,
                
            }).ToList();

            return new ResponseDTO("Lấy danh sách danh mục thành công.", 200, true, result);
        }
    }
}
