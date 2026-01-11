using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Activity
    {
        public Guid ActivityId { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid ItineraryId { get; set; }
        public virtual Itinerary Itinerary { get; set; }
    }
}
