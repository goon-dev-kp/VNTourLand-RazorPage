using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class RequestOfCustomer
    {
        public Guid RequestId { get; set; }           // Mã yêu cầu
        public Guid CustomerId { get; set; }          // Mã người dùng (User) gửi yêu cầu

        public string Destination { get; set; }       // Điểm đến chính
        public string Email { get; set; }              // Email liên hệ
        public string PhoneNumber { get; set; }       // Số điện thoại liên hệ
        public string Address { get; set; }
        public string DepartureLocation { get; set; } // Nơi khởi hành
        public DateTime? StartDate { get; set; }      // Ngày bắt đầu mong muốn
        public DateTime? EndDate { get; set; }        // Ngày kết thúc mong muốn

        public int NumberOfPeople { get; set; }       // Số người tham gia
        public string Requirements { get; set; } // Yêu cầu đặc biệt (ăn chay, nghỉ dưỡng, v.v.)
        public string BudgetRange { get; set; }       // Khoảng ngân sách dự kiến (ví dụ: "5-10 triệu")
        public RequestStatus Status { get; set; }     // Trạng thái xử lý yêu cầu (Pending, Approved, Rejected...)
        public DateTime CreatedAt { get; set; }       // Ngày gửi yêu cầu


        // Quan hệ với User (Khách hàng)
        public virtual User Customer { get; set; }
    }
}
