using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Services.Implement
{
    public class ReviewerService : IReviewerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserUtility _userUtility;
        public ReviewerService(IUnitOfWork unitOfWork, UserUtility userUtility)
        {
            _unitOfWork = unitOfWork;
            _userUtility = userUtility;
        }
        public async Task<ResponseDTO> GetAllByTourId(Guid tourId)
        {
            try
            {
                var reviewers = await _unitOfWork.ReviewerRepo.GetAllByTourIdAsync(tourId);

                var result = reviewers.Select(r => new ReviewerDTO
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Name = r.User.UserName,
                    Email = r.User.Email,
                }).ToList();

                return new ResponseDTO("Successfully retrieved the tour reviews", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving the reviews", 500, false, ex.Message);
            }
        }


        public async Task<ResponseDTO> GetAllByStoryId(Guid storyId)
        {
            try
            {
                var reviewers = await _unitOfWork.ReviewerRepo.GetAllWithStoryId(storyId);

                var result = reviewers.Select(r => new ReviewerDTO
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Name = r.User.UserName,
                    Email = r.User.Email,
                }).ToList();

                return new ResponseDTO("Successfully retrieved the story reviews", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error occurred while retrieving the reviews", 500, false, ex.Message);
            }
        }


        public async Task<ResponseDTO> CreateReviewerByStoryIdAsync(CreateReviewerDTO reviewerDTO)
        {
            var userId = _userUtility.GetUserIDFromToken();
            if (userId == Guid.Empty)
            {
                return new ResponseDTO("User is not logged in.", 401, false, null);
            }

            if (!reviewerDTO.StoryId.HasValue)
                return new ResponseDTO("StoryId is required.", 400, false, null);

            var story = await _unitOfWork.StoryRepo.GetByIdAsync(reviewerDTO.StoryId.Value);
            if (story == null)
                return new ResponseDTO("Story not found.", 404, false, null);

            var reviewer = new Reviewer
            {
                ReviewerId = Guid.NewGuid(),
                BlogId = reviewerDTO.StoryId,
                UserId = userId,
                Rating = reviewerDTO.Rating,
                Comment = reviewerDTO.Comment,
                DateTime = DateTime.UtcNow
            };

            await _unitOfWork.ReviewerRepo.AddAsync(reviewer);
            await _unitOfWork.SaveAsync();

            return new ResponseDTO("Review added successfully.", 200, true, null);
        }



        public async Task<ResponseDTO> CreateReviewerByBlogIdAsync(CreateReviewerDTO reviewerDto)
        {
            var userId = _userUtility.GetUserIDFromToken();
            if (userId == Guid.Empty)
            {
                return new ResponseDTO("User not logged in.", 401, false, null);
            }

            // Check if Blog exists
            if (!reviewerDto.BlogId.HasValue)
                return new ResponseDTO("Missing BlogId.", 400, false, null);

            var blog = await _unitOfWork.BlogRepo.GetByIdAsync(reviewerDto.BlogId.Value);
            if (blog == null)
                return new ResponseDTO("Blog does not exist.", 404, false, null);

            // Map from DTO to Reviewer entity
            var reviewer = new Reviewer
            {
                ReviewerId = Guid.NewGuid(),
                BlogId = reviewerDto.BlogId,
                UserId = userId,
                Rating = reviewerDto.Rating,
                Comment = reviewerDto.Comment,
                DateTime = DateTime.UtcNow
            };

            await _unitOfWork.ReviewerRepo.AddAsync(reviewer);
            await _unitOfWork.SaveAsync();

            return new ResponseDTO("Successfully created the comment.", 200, true, null);
        }


        public async Task<ResponseDTO> GetReviewerByBlogId(Guid blogId)
        {
            var reviewers = await _unitOfWork.ReviewerRepo.GetAllWithBlogId(blogId);

            var reviewerDtos = reviewers.Select(r => new ReviewerDTO
            {
                Rating = r.Rating,
                Comment = r.Comment,
                Name = r.User.UserName,
                Email = r.User.Email,
            }).ToList();

            return new ResponseDTO("Lấy danh sách bình luận thành công.", 200, true, reviewerDtos);
        }
        // Tạo bình luận cho tour
        public async Task<ResponseDTO> CreateReviewerByTourIdAsync(CreateReviewerDTO reviewerDto)
        {
            var userId = _userUtility.GetUserIDFromToken();
            if (userId == Guid.Empty)
            {
                return new ResponseDTO("User is not logged in.", 401, false, null);
            }

            if (!reviewerDto.TourId.HasValue)
            {
                return new ResponseDTO("TourId is required.", 400, false, null);
            }

            var tour = await _unitOfWork.TourRepo.GetByIdAsync(reviewerDto.TourId.Value);
            if (tour == null)
            {
                return new ResponseDTO("Tour not found.", 404, false, null);
            }

            var reviewer = new Reviewer
            {
                ReviewerId = Guid.NewGuid(),
                TourId = reviewerDto.TourId,
                UserId = userId,
                Rating = reviewerDto.Rating,
                Comment = reviewerDto.Comment,
                DateTime = DateTime.UtcNow
            };

            await _unitOfWork.ReviewerRepo.AddAsync(reviewer);
            await _unitOfWork.SaveAsync();

            return new ResponseDTO("Review submitted successfully.", 200, true, reviewer.TourId);
        }


        // Nếu bạn cần, giữ nguyên hàm blog (hoặc chỉnh sửa tuỳ ý)
    }
}
