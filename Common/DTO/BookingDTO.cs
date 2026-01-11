using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class BookingDTO
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid TourId { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public string Code { get; set; }
        public String UserName { get; set; } // Thêm thuộc tính UserName vào BookingDTO
        public string TourName { get; set; } // Thêm thuộc tính TourName vào BookingDTO

    }

    public class CreateBookingDTO
    {
        public Guid TourId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Note { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        //public List<Guid> SelectedAddOns { get; set; } = new(); 
    }

    public class GetBookingDTO
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid TourId { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }

        public string UserName { get; set; } // Tên người dùng
        public string TourName { get; set; } // Tên tour

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Note { get; set; }
        public string Code { get; set; } // Mã booking
        public decimal TotalPrice { get; set; } // Giá tour

        //public List<AddOptionDTO> AddOnOptions { get; set; } = new List<AddOptionDTO>();
    }

    public class MyBookingDTO
    {
        public Guid BookingId { get; set; }
        public string TourTitle { get; set; }
        public string TourImage { get; set; }
        public float Rating { get; set; }
        public string Description { get; set; }
 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Destination { get; set; }

        public int DurationDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string Code { get; set; } // Mã booking

        // Nút điều khiển UI
        public bool ShowPayButton { get; set; }
        public bool ShowCancelButton { get; set; }
        public bool ShowSuccessMessage { get; set; }
    }

    public class BookingManage
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string TourName { get; set; }
        public DateTime BookingDate { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public BookingStatus Status { get; set; }
    }

    public class BookingForCustomerDTO
    {
        public Guid BookingId { get; set; }
        public string Code { get; set; }

        // Customer info
        public string FullName { get; set; }

        // Tour info
        public string TourName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Booking details
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public int TotalPeople => NumberOfAdults + NumberOfChildren;

        public string Status { get; set; }
    }

    public class BookingDetailsDTO
    {
        // Customer info
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public int NumberOfGuests { get; set; }  // Số khách
        public decimal TotalPrice { get; set; }  // Tổng tiền

        public TourDetailDTO TourDetails { get; set; } = new TourDetailDTO(); // Thông tin tour

        public List<ItineraryDTO> Itineraries { get; set; } = new List<ItineraryDTO>();

        public List<IncludedDTO> IncludedServices { get; set; } = new List<IncludedDTO>();

        public List<NotIncludedDTO> NotIncludedServices { get; set; } = new List<NotIncludedDTO>();
    }

}
