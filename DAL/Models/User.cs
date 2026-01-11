using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace DAL.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public RoleType Role { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
        public virtual ICollection<Reviewer> Reviewers { get; set; } = new HashSet<Reviewer>();
        public bool IsActive { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new HashSet<Message>();

        public virtual ICollection<RequestOfCustomer> RequestsOfCustomer { get; set; }

        public ICollection<TourParticipant> TourParticipants { get; set; } = new List<TourParticipant>();

    }
}
