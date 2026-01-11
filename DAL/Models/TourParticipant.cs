using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace DAL.Models
{
    public class TourParticipant
    {
        public Guid TourParticipantId { get; set; }
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public ParticipantRole Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
