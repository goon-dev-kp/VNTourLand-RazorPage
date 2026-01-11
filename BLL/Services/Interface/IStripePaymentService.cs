using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutSessionAsync(GetBookingDTO booking, string successUrl, string cancelUrl);
    }
}
