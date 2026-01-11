using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class TourDTO
    {
        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Location { get; set; }
        public decimal PriceOfAdult { get; set; }
        public decimal PriceOfChild { get; set; }
        public string? CustomAddOnNote { get; set; }
        public decimal? CustomAddOnFee { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } // "Active" or "Inactive"
        public bool IsActive { get; set; }
    }

    public class  TourLocationDTO
    {
        public Guid TourId { get; set; }
        public string ImageURL { get; set; }
        public string TourName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class TourWithStoryDTO
    {
        public TourDTO Tour { get; set; }
        public List<StoryDTO> Stories { get; set; }
    }

}
