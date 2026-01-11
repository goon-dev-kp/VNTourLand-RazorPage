using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IPayPalService
    {
        Task<string> GetAccessTokenAsync();
        Task<string> CreateOrderAndGetRedirectUrlAsync(decimal amount, Guid bookingId);
        Task<bool> CaptureOrderAsync(string orderId);
    }
}
