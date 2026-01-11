using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace DAL.Models
{
    public class TourSchedule
    {
        public Guid TourScheduleId { get; set; }

        // Liên kết với Tour gốc
        public Guid TourId { get; set; }
        public virtual Tour Tour { get; set; }

        // Thời gian và số chỗ
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableSlot { get; set; }

        // Trạng thái của lịch khởi hành: Đang mở bán, Hết chỗ, Đã huỷ...
        public TourStatus Status { get; set; }

        // Booking thuộc về lịch này
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
    }

}
