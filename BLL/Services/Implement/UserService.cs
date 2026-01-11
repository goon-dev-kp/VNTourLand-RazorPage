using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Enums;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implement
{
    public class UserService : IIUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserUtility _userUtility;

        public UserService(IUnitOfWork unitOfWork, UserUtility userUtility)
        {
            _unitOfWork = unitOfWork;
            _userUtility = userUtility;
        }
        public async Task<List<UserDTO>> GetAllUserAsync()
        {
            return await _unitOfWork.UserRepo
                .GetAll()
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CreatedAt = u.CreatedAt,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }

        public async Task<ResponseDTO> GetUserById()
        {
            var userID = _userUtility.GetUserIDFromToken();
            if (userID == null)
            {
                return new ResponseDTO("User ID not found in token", 400, false, null);
            }
            var user = await _unitOfWork.UserRepo.GetByIdAsync(userID);
            if (user == null)
            {
                return new ResponseDTO("User not found", 404, false, null);
            }
            var userDto = new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
            return new ResponseDTO("User retrieved successfully", 200, true, userDto);
        }

        public async Task<ResponseDTO> GetAllSeller()
        {
            var sellers = await _unitOfWork.UserRepo.GetAllSeller();

            var sellerDtos = sellers.Select(s => new UserDTO
            {
                UserId = s.UserId,
                UserName = s.UserName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                CreatedAt = s.CreatedAt,
                Role = s.Role.ToString(),
                IsActive = s.IsActive
            }).ToList();

            if (!sellerDtos.Any())
            {
                return new ResponseDTO("No sellers found", 400, false, null);
            }

            return new ResponseDTO("Sellers retrieved successfully", 200, true, sellerDtos);
        }

        public async Task<ResponseDTO> GetAllManagerAndSeller()
        {
            var users = await _unitOfWork.UserRepo.GetAllManagerAndSeller();

            var userDtos = users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                CreatedAt = u.CreatedAt,
                Role = u.Role.ToString(),
                IsActive = u.IsActive
            }).ToList();

            if (!userDtos.Any())
            {
                return new ResponseDTO("No managers or sellers found", 400, false, null);
            }

            return new ResponseDTO("Managers and sellers retrieved successfully", 200, true, userDtos);
        }

        public async Task<bool> ChangeUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await _unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<ResponseDTO<UserDTO>> UpdateUserAsync(UserDTO userDto)
        {
            var user = await _unitOfWork.UserRepo.GetByIdAsync(userDto.UserId);
            if (user == null)
            {
                return new ResponseDTO<UserDTO>
                {
                    Success = false,
                    Message = "User not found",

                };
            }

            // Cập nhật các trường (bạn có thể chọn trường nào cho phép sửa)
            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Role = Enum.Parse<RoleType>(userDto.Role);
            user.IsActive = userDto.IsActive;

            await _unitOfWork.SaveAsync();

            // Map lại sang DTO để trả về
            var updatedUserDto = new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };

            return new ResponseDTO<UserDTO>
            {
                Success = true,
                Message = "User updated successfully",

            };
        }
        public async Task<ResponseDTO> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDTO("User not found", 404, false, null);
            }
            _unitOfWork.UserRepo.Delete(user);
            await _unitOfWork.SaveAsync();
            return new ResponseDTO("User deleted successfully", 200, true, null);
        }
    }

        

}
