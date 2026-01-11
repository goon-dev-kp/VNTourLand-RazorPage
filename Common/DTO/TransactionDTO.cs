using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.DTO
{
    public class TransactionDTO
    {
        public Guid TransactionId { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
    }


    public class CreateTransactionDTO
    {
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }


    }
    public class TransactionManage
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public Common.Enums.PaymentStatus Status { get; set; }
        public string TransactionReference { get; set; }

        // Booking info for display
        public Guid BookingId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string TourName { get; set; }
        // Nếu cần thông tin tour, khách hàng thì có thể thêm như sau:
        public BookingManage Booking { get; set; }
    }
}

