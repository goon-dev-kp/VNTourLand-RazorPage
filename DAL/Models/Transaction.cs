using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models
{
    public class Transaction
    {
        public Guid TransactionId { get; set; } // Mã giao dịch thanh toán
        public Guid BookingId { get; set; } // Mã đặt tour (tham chiếu đến Booking)

        [Precision(18, 2)]
        public decimal Amount { get; set; } // Số tiền thanh toán
        public string PaymentMethod { get; set; } // Phương thức thanh toán (Credit Card, PayPal, v.v.)
        public DateTime PaymentDate { get; set; } // Ngày thanh toán
        public PaymentStatus Status { get; set; } // Trạng thái thanh toán (Pending, Completed, Failed)
        public string TransactionReference { get; set; } // Mã giao dịch tham chiếu từ cổng thanh toán
        public virtual Booking Booking { get; set; } // Tham chiếu đến Booking
    }
}
