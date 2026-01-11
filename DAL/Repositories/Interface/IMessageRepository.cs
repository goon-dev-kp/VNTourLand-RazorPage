using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<List<Message>> GetConversationAsync(Guid userA, Guid userB);
        Task CreateAsync(Message message);
        Task<List<User>> GetChatUsersAsync(Guid userId);


    }
}
