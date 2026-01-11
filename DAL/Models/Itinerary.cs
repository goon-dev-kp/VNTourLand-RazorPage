using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DAL.Models
{
    public class Itinerary
    {
        public Guid ItineraryId { get; set; }
        public string Name { get; set; }
        // Session: Buổi trong ngày, ví dụ: Morning, Afternoon, Evening

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        // Activities: Danh sách các hoạt động trong một lịch trình
        public virtual ICollection<Activity> Activities { get; set; } = new HashSet<Activity>();
        public Guid TourId { get; set; }
        public virtual Tour Tour { get; set; }
    }
}
