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
    public class StoryService : IStoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        public StoryService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
        }
        public async Task<ResponseDTO> CreateStoryAsync(StoryCreateDTO dto)
        {
            try
            {
                string imageUrl = null;
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(dto.ImageFile);
                }

                var entity = new Story
                {
                    StoryId = Guid.NewGuid(),
                    LocationOfStoryId = dto.LocationOfStoryId,
                    Title = dto.Title,
                    Content = dto.Content,
                    ImageUrl = imageUrl,
                    AuthorName = dto.AuthorName,
                    StoryDate = DateTime.SpecifyKind(dto.StoryDate, DateTimeKind.Utc)
                };

                var result = await _unitOfWork.StoryRepo.AddAsync(entity);
                await _unitOfWork.SaveChangeAsync();
                return new ResponseDTO("Story created successfully", 201, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Failed to create story", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> DeleteStoryAsync(Guid storyId)
        {
            try
            {
                bool deleted = await _unitOfWork.StoryRepo.DeleteStoryAsync(storyId);
                if (deleted)
                {
                    await _unitOfWork.SaveChangeAsync();
                    return new ResponseDTO("Story deleted successfully.", 200, true, null);
                }
                else
                {
                    return new ResponseDTO("Story not found.", 404, false, null);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error deleting Story: " + ex.Message, 500, false, null);
            }
        }
        public async Task<ResponseDTO> GetStoryByIdAsync(Guid storyId)
        {
            try
            {
                var story = await _unitOfWork.StoryRepo.GetByIdAsync(storyId);

                if (story == null)
                    return new ResponseDTO("Story not found", 404, false, null);

                // Map từ DAL.Model sang DTO
                var storyDto = new StoryDTO
                {
                    StoryId = story.StoryId,

                    Title = story.Title,
                    Content = story.Content,
                    ImageUrl = story.ImageUrl,
                    AuthorName = story.AuthorName,
                    StoryDate = story.StoryDate
                };

                return new ResponseDTO("Get story successfully", 200, true, storyDto);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error when getting story", 500, false, ex.Message);
            }
        }
        public async Task<ResponseDTO> GetStoriesByLocationNameAsync(string locationName)
        {
            var stories = await _unitOfWork.StoryRepo.GetStoriesByLocationNameAsync(locationName);

            if (stories == null || !stories.Any())
            {
                return new ResponseDTO("No stories found for the specified location.", 404, false, null);
            }
            var storyDto = stories.Select(s => new StoryDTO
            {
                StoryId = s.StoryId,
                Title = s.Title,
                Content = s.Content,
                ImageUrl = s.ImageUrl,
                AuthorName = s.AuthorName,
                StoryDate = s.StoryDate
            }).ToList();

            return new ResponseDTO("Stories retrieved successfully.", 200, true, storyDto);
        }

        public async Task<ResponseDTO> UpdateStoryAsync(StoryEditDTO dto)
        {
            try
            {
                var existingStory = await _unitOfWork.StoryRepo.GetByIdAsync(dto.StoryId);
                if (existingStory == null)
                {
                    return new ResponseDTO("Story not found", 404, false, null);
                }
                // Cập nhật thông tin story
                existingStory.LocationOfStoryId = dto.LocationOfStoryId;
                existingStory.Title = dto.Title;
                existingStory.Content = dto.Content;
                existingStory.AuthorName = dto.AuthorName;
                existingStory.StoryDate = DateTime.SpecifyKind(dto.StoryDate, DateTimeKind.Utc);
                // Xử lý ảnh nếu có
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    existingStory.ImageUrl = await _fileUploadService.UploadImageToFirebaseAsync(dto.ImageFile);
                }
                await _unitOfWork.SaveChangeAsync();
                return new ResponseDTO("Story updated successfully", 200, true, existingStory);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Failed to update story", 500, false, ex.Message);
            }

        }
    }
}
