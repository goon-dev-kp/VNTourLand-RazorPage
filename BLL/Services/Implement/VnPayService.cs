using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using DAL.UnitOfWork;
using Microsoft.Extensions.Configuration;
using VNPAY.NET.Models;
using VNPAY.NET;

namespace BLL.Services.Implement
{
    public class VnPayService : IVnPayService
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public VnPayService(IVnpay vnpay, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _vnpay = vnpay; // ✅ Không cần gọi _vnpay.Initialize() nữa vì đã làm trong Program.cs
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePaymentUrlAsync(PaymentRequest request)
        {
            return _vnpay.GetPaymentUrl(request);
        }

        //public async Task<ResponseDTO> CallBackVnPay(Guid transactionId)
        //{
        //    // Lấy giao dịch theo ID
        //    var transaction = await _unitOfWork.TransactionRepo.GetByIdAsync(transactionId);


        //    if (transaction == null)
        //    {
        //        return new ResponseDTO("Không tìm thấy giao dịch!", 400, false);
        //    }



        //    // Lấy Enrollment dựa trên Transaction (giả sử có EnrollmentId trong Transaction)
        //    var booking = await _unitOfWork.BookingRepo.GetByIdAsync(transaction.BookingId);

        //    if (booking == null)
        //    {
        //        return new ResponseDTO("Không tìm thấy khóa học!", 400, false);
        //    }

        //    enrollment.Status = Common.Enum.EnrollmentStatus.Enrolled;


        //    transaction.status = Transaction.TransactionStatus.SUCCESS;

        //    await _unitOfWork.SaveChangeAsync(); // ✅ Cập nhật trạng thái giao dịch trong DB
        //    return new ResponseDTO("thanh toan thanh cong", 200, true);

        //    //}
        //}
    }
}
