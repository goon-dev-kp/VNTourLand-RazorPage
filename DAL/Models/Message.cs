using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
        public string? ImageUrl { get; set; } // Thêm dòng này
    }

}
