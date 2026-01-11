using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using BLL.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace BLL.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly UserUtility _userUtility;

        public ChatHub(IChatService chatService, UserUtility userUtility)
        {
            _chatService = chatService;
            _userUtility = userUtility;
        }


        public override Task OnConnectedAsync()
        {
            Console.WriteLine("🧠 SignalR Connected. UserIdentifier = " + Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = _userUtility.GetUserIDFromToken();
            if (senderId == Guid.Empty) return;

            await _chatService.SendMessageAsync(senderId, Guid.Parse(receiverId), message);
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId.ToString(), message);

            
        }

        public async Task SendReceiveChatUser(string targetUserId)
        {
            var senderId = _userUtility.GetUserIDFromToken();
            if (senderId == Guid.Empty) return;

            Console.WriteLine($"📤 SendReceiveChatUser from {senderId} to {targetUserId}");

            await Clients.User(targetUserId)
                .SendAsync("ReceiveChatUser", senderId.ToString());
        }

    }

}
