using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using VNPAY.NET.Models;

namespace BLL.Services.Interface
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrlAsync(PaymentRequest request);
        //Task<ResponseDTO> CallBackVnPay(Guid transactionId);
    }
}
