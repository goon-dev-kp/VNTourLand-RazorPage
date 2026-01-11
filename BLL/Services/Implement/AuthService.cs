using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.Constants;
using Common.DTO;
using Common.Enums;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using static Common.DTO.AuthDTO;

namespace BLL.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserUtility _userUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUnitOfWork unitOfWork, UserUtility userUtility, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userUtility = userUtility;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ResponseDTO> Login(LoginDTO loginDTO)
        {
            var user = await _unitOfWork.UserRepo.FindByEmailAsync(loginDTO.Email);
            if (user == null )
            {
                return new ResponseDTO("User not found", 404, false);
            }
            if (!user.IsActive)
            {
                return new ResponseDTO("The account has been deactivated.", 403, false);
            }

            // kiểm tra mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password);
            if (!isPasswordValid)
            {
                return new ResponseDTO("Wrong password", 400, false);
            }

            //kiểm tra refreshToken
            var exitsRefreshToken = await _unitOfWork.TokenRepo.GetRefreshTokenByUserID(user.UserId);
            if (exitsRefreshToken != null)
            {
                exitsRefreshToken.IsRevoked = true;
                await _unitOfWork.TokenRepo.UpdateAsync(exitsRefreshToken);
            }

            //khởi tạo claim
            var claims = new List<Claim>
            {
                new Claim(JwtConstant.KeyClaim.Email, user.Email),
                new Claim(JwtConstant.KeyClaim.UserId, user.UserId.ToString()),
                new Claim(JwtConstant.KeyClaim.UserName, user.UserName),
                new Claim(JwtConstant.KeyClaim.Role, user.Role.ToString())
            };

            //tạo refesh token
            var refreshTokenKey = JwtProvider.GenerateRefreshToken(claims);
            var accessTokenKey = JwtProvider.GenerateAccessToken(claims);

            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                RefreshTokenKey = refreshTokenKey,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.TokenRepo.Add(refreshToken);
            try
            {
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                return new ResponseDTO($"Error saving refresh token: {ex.Message}", 500, false);
            }

            

            return new ResponseDTO("Login successfully", 200, true, new TokenDTO
            {
                AccessToken = accessTokenKey,
                RefreshToken = refreshTokenKey,
                Role = user.Role.ToString(),
            });


        }
        public async Task<ResponseDTO> LoginWithGoogle(string googleEmail)
        {
            var user = await _unitOfWork.UserRepo.FindByEmailAsync(googleEmail);
            if (user == null)
            {
                // Tạo mới user nếu cần hoặc return lỗi
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = googleEmail,
                    UserName = googleEmail.Split('@')[0],
                    Password = "GOOGLE_LOGIN", // tránh trùng lặp
                    Role = RoleType.USER,
                    PhoneNumber = "null", // hoặc gán giá trị mặc định nếu cần
                    IsActive = true
                    // các trường khác gán mặc định
                };
                _unitOfWork.UserRepo.Add(user);
                await _unitOfWork.SaveChangeAsync();
            }

            if (!user.IsActive)
            {
                return new ResponseDTO("The account has been deactivated.", 403, false);
            }

            // Kiểm tra refresh token hiện có và revoke nếu có
            var existsRefreshToken = await _unitOfWork.TokenRepo.GetRefreshTokenByUserID(user.UserId);
            if (existsRefreshToken != null)
            {
                existsRefreshToken.IsRevoked = true;
                await _unitOfWork.TokenRepo.UpdateAsync(existsRefreshToken);
            }

            var claims = new List<Claim>
    {
        new Claim(JwtConstant.KeyClaim.Email, user.Email),
        new Claim(JwtConstant.KeyClaim.UserId, user.UserId.ToString()),
        new Claim(JwtConstant.KeyClaim.UserName, user.UserName),
        new Claim(JwtConstant.KeyClaim.Role, user.Role.ToString())
    };

            var refreshTokenKey = JwtProvider.GenerateRefreshToken(claims);
            var accessTokenKey = JwtProvider.GenerateAccessToken(claims);

            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,

                RefreshTokenKey = refreshTokenKey,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.TokenRepo.Add(refreshToken);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO("Login by Google successfully", 200, true, new TokenDTO
            {
                AccessToken = accessTokenKey,
                RefreshToken = refreshTokenKey,
                Role = user.Role.ToString(),
            });
        }

        public async Task<ResponseDTO> Register(RegisterDTO registerDTO)
        {
            if (string.IsNullOrWhiteSpace(registerDTO.UserName) ||
                string.IsNullOrWhiteSpace(registerDTO.Email) ||
                string.IsNullOrWhiteSpace(registerDTO.Password))

            {
                return new ResponseDTO("Please fill in all required fields.", 400, false);
            }

            if (registerDTO.Password != registerDTO.ConfirmPassword)
            {
                return new ResponseDTO("Passwords do not match.", 400, false);

            }

            try
            {
                var existingUser = await _unitOfWork.UserRepo.FindByEmailAsync(registerDTO.Email);
                if (existingUser != null)
                {
                    return new ResponseDTO("Email already exists.", 200, false);

                }

                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password, salt);

                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = registerDTO.UserName,
                    Email = registerDTO.Email,
                    Password = hashedPassword,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Role = RoleType.USER,
                    IsActive = true,
                };
                await _unitOfWork.UserRepo.AddAsync(newUser);
                await _unitOfWork.SaveAsync();


                return new ResponseDTO("Register successfully.", 200, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO($"Error: {ex.Message}", 500, false);
            }
        }
        public async Task<ResponseDTO> Logout()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                // Lấy AccessToken từ cookie để lấy userId (hoặc token bạn đang dùng)
                var accessToken = context.Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(accessToken))
                {
                    return new ResponseDTO("Access token not found", 400, false);
                }

                // Lấy userId từ accessToken bằng _userUtility
                var userId = _userUtility.GetUserIDFromToken();
                if (userId == Guid.Empty)
                {
                    return new ResponseDTO("Invalid access token", 401, false);
                }

                // Lấy refreshToken từ DB dựa trên userId
                var refreshTokenEntity = await _unitOfWork.TokenRepo.GetRefreshTokenByUserId(userId);
                if (refreshTokenEntity != null)
                {
                    // Đánh dấu refreshToken bị thu hồi
                    refreshTokenEntity.IsRevoked = true;
                    await _unitOfWork.TokenRepo.UpdateAsync(refreshTokenEntity);
                    await _unitOfWork.SaveChangeAsync();

                    Console.WriteLine($"[AuthService] RefreshToken của user {userId} đã bị thu hồi!");
                }

                // Xóa cookies AccessToken & RefreshToken
                context.Response.Cookies.Delete("AccessToken");
                

                Console.WriteLine("[AuthService] Đăng xuất thành công!");
            }
            else
            {
                return new ResponseDTO("HttpContext is null", 500, false);
            }

            return new ResponseDTO("Logout successful", 200, true);
        }


        public async Task<ResponseDTO> RegisterSeller(RegisterForBoss registerDTO)
        {
            if (string.IsNullOrWhiteSpace(registerDTO.UserName) ||
                string.IsNullOrWhiteSpace(registerDTO.Email) ||
                string.IsNullOrWhiteSpace(registerDTO.Password))
            {
                return new ResponseDTO("Vui lòng nhập đầy đủ thông tin", 200, false);
            }

           

            try
            {
                var existingUser = await _unitOfWork.UserRepo.FindByEmailAsync(registerDTO.Email);
                if (existingUser != null)
                {
                    return new ResponseDTO("Email đã tồn tại", 200, false);
                }

                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password, salt);

                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = registerDTO.UserName,
                    Email = registerDTO.Email,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Password = hashedPassword,
                    Role = RoleType.SELLER,
                    IsActive = true
                };

                await _unitOfWork.UserRepo.AddAsync(newUser);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO("Tạo tài khoản Seller thành công", 200, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO($"Lỗi khi tạo Seller: {ex.Message}", 500, false);
            }
        }

        public async Task<ResponseDTO> RegisterManager(RegisterForBoss registerDTO)
        {
            if (string.IsNullOrWhiteSpace(registerDTO.UserName) ||
                string.IsNullOrWhiteSpace(registerDTO.Email) ||
                string.IsNullOrWhiteSpace(registerDTO.Password))
            {
                return new ResponseDTO("Vui lòng nhập đầy đủ thông tin", 200, false);
            }

            

            try
            {
                var existingUser = await _unitOfWork.UserRepo.FindByEmailAsync(registerDTO.Email);
                if (existingUser != null)
                {
                    return new ResponseDTO("Email đã tồn tại", 200, false);
                }

                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password, salt);

                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = registerDTO.UserName,
                    Email = registerDTO.Email,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Password = hashedPassword,
                    Role = RoleType.MANAGER,
                    IsActive = true
                };

                await _unitOfWork.UserRepo.AddAsync(newUser);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO("Tạo tài khoản Manager thành công", 200, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO($"Lỗi khi tạo Manager: {ex.Message}", 500, false);
            }
        }


    }
}
