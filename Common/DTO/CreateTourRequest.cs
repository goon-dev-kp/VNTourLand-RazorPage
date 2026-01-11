using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.DTO
{
    public class CreateTourRequest
    {
        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public string Type { get; set; }
        public IFormFile Image { get; set; }
        public List<LocationRequest> Locations { get; set; } // <-- THÊM NÀY


        public List<string> IncludedItems { get; set; }
        public List<string> NotIncludedItems { get; set; }
        public List<ItineraryRequest> Itineraries { get; set; }
    }

    public class CreateTourSellerRequest
    {
        public Guid CustomerId { get; set; }
        public Guid RequestId { get; set; }

        public string TourName { get; set; }
        public string TourDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PriceOfAdult { get; set; }
        public decimal PriceOfChild { get; set; }

        public string Type { get; set; }
        public IFormFile Image { get; set; }
        public List<LocationRequest> Locations { get; set; } // <-- THÊM NÀY

        public string? CustomAddOnNote { get; set; }
        public decimal? CustomAddOnFee { get; set; }
        public List<string> IncludedItems { get; set; }
        public List<string> NotIncludedItems { get; set; }
        public List<ItineraryRequest> Itineraries { get; set; }
    }

    public class ItineraryRequest
    {
        public string Name { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ActivityRequest> Activities { get; set; }
    }

    public class ActivityRequest
    {
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class LocationRequest
    {
    
        public string LocationName { get; set; }

       
        public double Latitude { get; set; }

       
        public double Longitude { get; set; }
    }
}
