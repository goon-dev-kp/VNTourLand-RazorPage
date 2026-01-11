using BLL.Services.Implement;
using Common.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IChatService
    {
        Task<ResponseDTO> GetConversationAsync(Guid userId, Guid targetId);
        //Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content);
        Task<ResponseDTO> GetChatUsersAsync(Guid userId);
        //Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content, string? imageUrl = null);
        //Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content, IFormFile? imageFile = null);
        Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content, string? imageUrl = null);


    }
}
