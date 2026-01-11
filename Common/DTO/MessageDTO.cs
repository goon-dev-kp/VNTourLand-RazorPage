using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class MessageDTO
    {
        public Guid MessageId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        // Optional: Thêm tên người gửi (nếu cần)
        public string SenderName { get; set; }
        public string? ImageUrl { get; set; } // Thêm dòng này
    }

}
