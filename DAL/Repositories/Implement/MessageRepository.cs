using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetConversationAsync(Guid userA, Guid userB)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == userA && m.ReceiverId == userB) ||
                            (m.SenderId == userB && m.ReceiverId == userA))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetChatUsersAsync(Guid userId)
        {
            // Lấy toàn bộ tin nhắn có liên quan đến userId
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();

            // Lấy ra những người đã chat với user (trừ chính mình)
            var users = messages
                .Select(m => m.SenderId == userId ? m.Receiver : m.Sender)
                .Where(u => u != null && u.UserId != userId)
                .DistinctBy(u => u.UserId)
                .ToList();

            return users;
        }



    }
}
