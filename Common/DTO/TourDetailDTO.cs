using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class TourDetailDTO
    {
        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; }
        public List<LocationRequestJsonDTO> Location { get; set; }
        public decimal PriceOfAdult { get; set; }
        public decimal PriceOfChild { get; set; }
        public string? CustomAddOnNote { get; set; }
        public decimal? CustomAddOnFee { get; set; }

        public string Type { get; set; }

    }

    public class ItineraryDTO
    {
        public string Name { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ActivityDTO> Activities { get; set; } = new();
    }

    public class ActivityDTO
    {
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class IncludedDTO
    {
        public string Description { get; set; }
    }

    public class NotIncludedDTO
    {
        public string Description { get; set; }
    }
    public class LocationRequestJsonDTO
    {

        [JsonPropertyName("locationName")]
        public string LocationName { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
