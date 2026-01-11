using BLL.Services.Implement;
using BLL.Services.Interface;
using Common.DTO;
using DAL.Models;
using DAL.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;

    public BlogService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
    }

    public async Task<ResponseDTO> GetAllBlog()
    {
        var posts = _unitOfWork.BlogRepo.GetAllIncludeReviewers();

        var result = posts.Select(p => new BlogDTO
        {
            BlogId = p.BlogId,
            Title = p.Title,
            Summary = !string.IsNullOrEmpty(p.Content) && p.Content.Length > 150
                        ? p.Content.Substring(0, 100) + "..."
                        : p.Content,
            CreatedDate = p.CreatedDate,
            CountComment = p.Reviewers != null ? p.Reviewers.Count : 0,
            ImageUrl = p.Image
        }).ToList();

        return new ResponseDTO("Lấy danh sách bài viết thành công.", 200, true, result);
    }

    public async Task<ResponseDTO> GetAllBlogManager()
    {
        var posts = _unitOfWork.BlogRepo.GetAllIncludeReviewers();

        var result = posts.Select(p => new BlogDTO
        {
            BlogId = p.BlogId,
            Title = p.Title,
            CreatedDate = p.CreatedDate,
            CountComment = p.Reviewers != null ? p.Reviewers.Count : 0,
            ImageUrl = p.Image
        }).ToList();

        return new ResponseDTO("Lấy danh sách bài viết thành công.", 200, true, result);
    }


    public async Task<ResponseDTO> CreateBlog(CreateBlogDTO createBlogDTO)
    {
        try
        {
            // ✅ Upload ảnh lên Firebase
            string imageUrl = null;
            if (createBlogDTO.ImageFile != null && createBlogDTO.ImageFile.Length > 0)
            {
                imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(createBlogDTO.ImageFile);
            }

            // ✅ Tạo Blog entity
            var blog = new Blog
            {
                BlogId = Guid.NewGuid(),
                Title = createBlogDTO.Title.Trim(),
                Content = createBlogDTO.Content?.Trim(),
                CreatedDate = DateTime.UtcNow,
                Image = imageUrl,
                BlogCategoryId = createBlogDTO.BlogCategoryId
            };

            await _unitOfWork.BlogRepo.AddAsync(blog);

            // ✅ Lưu thay đổi
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO("Tạo bài viết thành công", 201, true);
        }
        catch (Exception ex)
        {
            return new ResponseDTO("Lỗi khi tạo bài viết", 500, false, ex.Message);
        }
    }
    public async Task<ResponseDTO> GetBlogByIdAsync(Guid blogId)
    {
        // Lấy blog kèm danh sách reviewer (nếu cần)
        var blog = await _unitOfWork.BlogRepo.GetByIdAsync(blogId);

        if (blog == null)
        {
            return new ResponseDTO("Không tìm thấy bài viết.", 404, false);
        }

        var dto = new BlogDetailDTO
        {
            BlogId = blog.BlogId,
            Title = blog.Title,
            Content = blog.Content,
            CreatedDate = blog.CreatedDate,
            ImageUrl = blog.Image,

            BlogCategoryId = blog.BlogCategoryId
        };

        return new ResponseDTO("Lấy bài viết thành công.", 200, true, dto);
    }


}