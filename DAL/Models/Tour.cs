using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DAL.Models
{
    public class Tour
    {
        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        // Price: Giá của tùy chọn bổ sung
        [Precision(18, 2)]
        public decimal PriceOfChild { get; set; }

        [Precision(18, 2)]
        public decimal PriceOfAdult { get; set; }
        //public int AvailableSlot { get; set; }
        public TourType Type { get; set; }
        public TourStatus Status { get; set; }

        // Locations: Danh sách các địa điểm sẽ ghé qua trong tour, bao gồm cả điểm bắt đầu và kết thúc
        // Sửa ở đây: Tour có nhiều TourLocation
        public virtual ICollection<TourLocation> TourLocations { get; set; } = new HashSet<TourLocation>();
        // Itineraries: Danh sách lịch trình cho từng ngày trong tour

        public virtual ICollection<Itinerary> Itineraries { get; set; } = new HashSet<Itinerary>();
        // TourImages: Danh sách hình ảnh của tour
        public  List<string> TourImages { get; set; } = new List<string>();
        public virtual ICollection<NotIncluded> NotIncluded { get; set; } = new HashSet<NotIncluded>();
        public virtual ICollection<Included> Included { get; set; } = new HashSet<Included>();
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
        public virtual ICollection<Reviewer> Reviewers { get; set; } = new HashSet<Reviewer>();
        public virtual ICollection<TourParticipant> Participants { get; set; } = new List<TourParticipant>();

        public bool IsActive { get; set; }
        public Guid? RequestId { get; set; }
        public string? CustomAddOnNote { get; set; }

        [Precision(18, 2)]
        public decimal? CustomAddOnFee { get; set; }
    }
}
