using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Services.Implement
{
    public class LocationOfStoryService : ILocationOfStoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        public LocationOfStoryService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
        }

        public async Task<ResponseDTO> GetAllLocationsWithStoriesAsync()
        {
            try
            {
                var locations = await _unitOfWork.LocationOfStoryRepo.GetAllLocationsWithStoriesAsync();

                var result = locations.Select(l => new LocationOfStoryDTO
                {
                    LocationOfStoryId = l.LocationOfStoryId,
                    LocationOfStoryName = l.LocationOfStoryName,
                    Description = l.Description,
                    BannerImageUrl = l.BannerImageUrl,
                    Stories = l.Stories.Select(s => new StoryDTO
                    {
                        StoryId = s.StoryId,
                        Title = s.Title,
                        Content = s.Content,
                        ImageUrl = s.ImageUrl,
                        AuthorName = s.AuthorName,
                        StoryDate = s.StoryDate
                    }).ToList()
                }).ToList();

                return new ResponseDTO("Successfully retrieved Locations and Stories list", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving Locations and Stories data", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> CreateLocationAsync(LocationOfStoryCreateDTO dto)
        {
            try
            {
                var imageUrls = new List<string>();

                if (dto.BannerImageFile != null && dto.BannerImageFile.Any())
                {
                    foreach (var imageFile in dto.BannerImageFile)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(imageFile);
                            imageUrls.Add(imageUrl);
                        }
                    }
                }

                var entity = new LocationOfStory
                {
                    LocationOfStoryId = Guid.NewGuid(),
                    LocationOfStoryName = dto.LocationOfStoryName,
                    Description = dto.Description,
                    BannerImageUrl = imageUrls
                };

                var result = await _unitOfWork.LocationOfStoryRepo.AddAsync(entity);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO("Successfully created Location", 201, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while creating Location", 500, false, ex.Message);
            }
        }


        public async Task<ResponseDTO> DeleteLocationAsync(Guid locationId)
        {
            try
            {
                bool deleted =await  _unitOfWork.LocationOfStoryRepo.DeleteLocationAsync(locationId);
                if (deleted)
                {
                    await _unitOfWork.SaveChangeAsync();
                    return new ResponseDTO("Location deleted successfully.", 200, true, null);
                }
                else
                {
                    return new ResponseDTO("Location not found.", 404, false, null);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error deleting Location: " + ex.Message, 500, false, null);
            }
        }

        public async Task<ResponseDTO> UpdateLocationAsync(LocationOfStoryUpdateDTO dto)
        {
            try
            {
                var existing = await _unitOfWork.LocationOfStoryRepo.GetByIdAsync(dto.LocationOfStoryId);
                if (existing == null)
                {
                    return new ResponseDTO("Location not found.", 404, false, null);
                }

                // Cập nhật thông tin cơ bản
                existing.LocationOfStoryName = dto.LocationOfStoryName;
                existing.Description = dto.Description;

                // Nếu có ảnh mới => upload toàn bộ, thay thế danh sách ảnh cũ
                if (dto.BannerImageFile != null && dto.BannerImageFile.Any())
                {
                    var newImageUrls = new List<string>();

                    foreach (var imageFile in dto.BannerImageFile)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(imageFile);
                            newImageUrls.Add(imageUrl);
                        }
                    }

                    existing.BannerImageUrl = newImageUrls; // Ghi đè danh sách ảnh cũ
                }

                _unitOfWork.LocationOfStoryRepo.UpdateAsync(existing);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO("Location updated successfully.", 200, true, existing);
            }
            catch (Exception ex)
            {
                return new ResponseDTO($"Error updating Location: {ex.Message}", 500, false, null);
            }
        }



    }
}
