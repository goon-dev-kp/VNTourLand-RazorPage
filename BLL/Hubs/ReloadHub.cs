using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BLL.Hubs
{
    public class ReloadHub : Hub
    {
        // Gửi lệnh reload cho tất cả client kết nối
        public async Task ReloadAllClients()
        {
            await Clients.All.SendAsync("ReloadPage");
        }

        // Gửi lệnh reload cho 1 client cụ thể (nếu cần)
        public async Task ReloadClient(string connectionId)
        {
            await Clients.Client(connectionId).SendAsync("ReloadPage");
        }
    }
}
