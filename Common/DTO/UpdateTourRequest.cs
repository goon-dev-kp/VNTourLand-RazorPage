using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class UpdateTourRequest
    {
        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PriceOfAdult { get; set; }
        public decimal PriceOfChild { get; set; }
  
        public string Type { get; set; }
        public string Status { get; set; } // "ACTIVE", "INACTIVE"...
        public bool IsActive { get; set; }

        public string ImageUrl { get; set; } // Optional: dùng khi không upload mới
        public IFormFile? NewImage { get; set; } // Optional: nếu người dùng upload hình mới
        public List<LocationRequest> Locations { get; set; }
    }
}
