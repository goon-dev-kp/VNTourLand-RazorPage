using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace DAL.Models
{
    public class Reviewer
    {
        public Guid ReviewerId { get; set; }

        public Guid? BlogId { get; set; }
        public virtual Blog Blog { get; set; }
        public Guid? TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public Guid? StoryId { get; set; }
        public virtual Story Story { get; set; }
        public Guid UserId { get; set; } // Khóa ngoại đến User (người dùng đã đăng nhập)
        public virtual User User { get; set; } // Navigation property đến User (người dùng đã đăng nhập)
        public DateTime DateTime { get; set; }
        public int Rating { get; set; } // ⭐ Đánh giá từ 1 đến 5 sao

        public string Comment { get; set; } // (tuỳ chọn) lời nhận xét
    }
}
