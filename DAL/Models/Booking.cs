using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models
{
    public class Booking
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid TourId { get; set; }
        
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public virtual Tour Tour { get; set; }
        public virtual User User { get; set; }
        public Guid? CreateById { get; set; } // Người tạo booking
        public bool IsActive { get; set; }

        // ✅ Thông tin người đặt tour
        public string FullName { get; set; }           // Họ và tên
        public string Email { get; set; }              // Email
        public string PhoneNumber { get; set; }        // Số điện thoại
        public string Address { get; set; }            // Địa chỉ
        public string Notes { get; set; }              // Ghi chú thêm (nếu có)

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }        // Tổng giá tour (bao gồm các tùy chọn thêm)
        public string Code { get; set; }

        //public virtual ICollection<OptionOnTour> OptionOnTours { get; set; }
        public ICollection<Transaction> Transactions { get; set; }


    }
}
