using BLL.Services.Interface;
using Common.DTO;
using DAL.Data;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;
        private readonly IFileUploadService _fileUploadService;

        public ChatService(IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task<ResponseDTO> GetConversationAsync(Guid userId, Guid targetId)
        {
            var messages = await _unitOfWork.MessageRepo.GetConversationAsync(userId, targetId);

            var result = messages.Select(m => new MessageDTO
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                ImageUrl = m.ImageUrl,
                IsRead = m.IsRead,
                SenderName = m.Sender?.UserName ?? "Unknown"
            }).ToList();

            return new ResponseDTO("Conversation retrieved successfully", 200, true, result);
        }

        //public async Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content)
        //{
        //    var message = new Message
        //    {
        //        MessageId = Guid.NewGuid(),
        //        SenderId = senderId,
        //        ReceiverId = receiverId,
        //        Content = content,
        //        SentAt = DateTime.UtcNow,
        //        IsRead = false
        //    };

        //    await _unitOfWork.MessageRepo.CreateAsync(message);

        //    return new ResponseDTO("Message sent", 200, true, null);
        //}
        public async Task<ResponseDTO> SendMessageAsync(Guid senderId, Guid receiverId, string content, string? imageUrl = null)
        {
            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ImageUrl = imageUrl
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return new ResponseDTO("Success", 200, true, message);
        }

        public async Task<ResponseDTO> GetChatUsersAsync(Guid userId)
        {
            var users = await _unitOfWork.MessageRepo.GetChatUsersAsync(userId);

            if (users == null || !users.Any())
                return new ResponseDTO("No chat history found", 200, true, new List<UserChatDTO>());

            var result = users.Select(u => new UserChatDTO
            {
                UserId = u.UserId,
                UserName = u.UserName
            }).ToList();

            return new ResponseDTO("Successfully retrieved the list of chat users", 200, true, result);
        }



    }
}
