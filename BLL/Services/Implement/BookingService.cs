using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Enums;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.Implement
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserUtility _userUtility;
        private readonly IPayPalService _paypalService;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookingService(IUnitOfWork unitOfWork, UserUtility userUtility, IPayPalService payPalService, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _userUtility = userUtility;
            _paypalService = payPalService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseDTO<List<BookingDTO>>> GetAllBookingAsync()
        {
            var bookings = await (from b in _unitOfWork.BookingRepo.GetAll()
                                  join u in _unitOfWork.UserRepo.GetAll() on b.UserId equals u.UserId
                                  select new BookingDTO
                                  {
                                      BookingId = b.BookingId,
                                      UserId = b.UserId,
                                      // Lấy thêm tên người dùng
                                      // Giả sử bạn thêm thuộc tính UserName vào BookingDTO
                                      UserName = u.UserName,
                                      TourName = b.Tour.TourName, // Lấy tên tour từ TourId
                                      TourId = b.TourId,
                                      NumberOfAdults = b.NumberOfAdults,
                                      NumberOfChildren = b.NumberOfChildren,
                                      BookingDate = b.BookingDate,
                                      Code = b.Code,
                                      Status = b.Status
                                  }).ToListAsync();

            return new ResponseDTO<List<BookingDTO>>
            {
                Success = true,
                Message = "Lấy danh sách booking thành công",

            };
        }

        public async Task<ResponseDTO> CreateBooking(CreateBookingDTO dto)
        {
            try
            {
                var userId = _userUtility.GetUserIDFromToken();

                // Lấy thông tin Tour
                var tour = await _unitOfWork.TourRepo.GetByIdAsync(dto.TourId);
                if (tour == null)
                {
                    return new ResponseDTO("Tour not found", 404, false);
                }

                // Lấy customerAddFee từ tour (hoặc từ dto nếu bạn truyền lên)
                decimal customerAddFee = tour.CustomAddOnFee ?? 0m;

                // Tính tiền vé người lớn + trẻ em
                decimal basePrice =
                    (tour.PriceOfAdult * dto.NumberOfAdults) +
                    (tour.PriceOfChild * dto.NumberOfChildren);

                // Tính tiền add-on nếu có
                //decimal addOnPrice = 0;
                //if (dto.SelectedAddOns?.Any() == true)
                //{
                //    var addOns = await _unitOfWork.AddOnOptionRepo
                //        .GetAsync(o => dto.SelectedAddOns.Contains(o.AddOnOptionId));

                //    addOnPrice = addOns.Sum(o => o.Price);
                //}

                // Tổng tiền
                decimal totalPrice = basePrice + customerAddFee;

                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    TourId = dto.TourId,
                    UserId = userId,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    Notes = string.IsNullOrWhiteSpace(dto.Note) ? "N/A" : dto.Note,
                    NumberOfAdults = dto.NumberOfAdults,
                    NumberOfChildren = dto.NumberOfChildren,
                    BookingDate = DateTime.UtcNow,
                    IsActive = false,
                    Code = BookingCodeGenerator.GenerateUniqueBookingCode(),
                    Status = Common.Enums.BookingStatus.WAITING_FOR_PAYMENT,
                    TotalPrice = totalPrice // 👈 GÁN GIÁ TRỊ TÍNH ĐƯỢC
                };

                await _unitOfWork.BookingRepo.AddAsync(booking);

                //// Thêm OptionOnTour nếu có chọn Add-ons
                //if (dto.SelectedAddOns?.Any() == true)
                //{
                //    var optionList = dto.SelectedAddOns.Select(id => new OptionOnTour
                //    {
                //        OptionOnTourId = Guid.NewGuid(),
                //        BookingId = booking.BookingId,
                //        AddOnOptionId = id
                //    }).ToList();

                //    await _unitOfWork.OptionOnTourRepo.AddRangeAsync(optionList);
                //}

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO("Create tour successfully", 200, true, booking.BookingId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error", 500, false, ex.Message);
            }
        }


        public async Task<ResponseDTO> GetBookingById(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.BookingRepo.GetBookingDetailByIdAsync(bookingId);

                if (booking == null)
                {
                    return new ResponseDTO("Not found booking", 404, false);
                }

                var bookingDto = new GetBookingDTO
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    UserName = booking.User?.UserName,
                    TourId = booking.TourId,
                    TourName = booking.Tour?.TourName,
                    NumberOfAdults = booking.NumberOfAdults,
                    NumberOfChildren = booking.NumberOfChildren,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    FullName = booking.FullName,
                    Email = booking.Email,
                    PhoneNumber = booking.PhoneNumber,
                    Address = booking.Address,
                    Note = booking.Notes,
                    TotalPrice = booking.TotalPrice,
                    Code = booking.Code,
                    //AddOnOptions = booking.OptionOnTours?.Select(o => new AddOptionDTO
                    //{
                    //    AddOnOptionId = o.AddOnOptionId,
                    //    Name = o.AddOnOption?.Name,
                    //    Price = o.AddOnOption?.Price ?? 0
                    //}).ToList()
                };

                return new ResponseDTO("Create booking successfullt", 200, true, bookingDto);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error in booking", 500, false, ex.Message);
            }
        }

        public async Task<(Guid bookingId, Guid? requestId)?> MarkAsPaidAsync(string paypalOrderId)
        {
            try
            {
                // 1. Lấy access token từ PayPal
                var token = await _paypalService.GetAccessTokenAsync();
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 2. Gọi API để lấy custom_id (tức là bookingId)
                var response = await client.GetAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{paypalOrderId}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Không lấy được thông tin order PayPal: " + json);
                    return null;
                }

                var result = JsonSerializer.Deserialize<JsonElement>(json);
                string bookingIdString = result.GetProperty("purchase_units")[0].GetProperty("custom_id").GetString();

                if (!Guid.TryParse(bookingIdString, out Guid bookingId))
                {
                    Console.WriteLine("custom_id không phải là GUID hợp lệ.");
                    return null;
                }

                // 3. Cập nhật trạng thái Booking
                var booking = await _unitOfWork.BookingRepo.GetByIdWithTourAsync(bookingId);
                if (booking == null)
                {
                    Console.WriteLine($"Không tìm thấy booking với ID {bookingId}");
                    return null;
                }

                booking.Status = Common.Enums.BookingStatus.CONFIRMED;
                booking.IsActive = true;

                await _unitOfWork.SaveChangeAsync();
                Console.WriteLine($"✅ Booking {bookingId} đã cập nhật trạng thái thành CONFIRMED");

                return (booking.BookingId, booking.Tour?.RequestId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong MarkAsPaidAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<ResponseDTO> GetMyBookingAsync(Guid userId)
        {
            var bookings = await _unitOfWork.BookingRepo.GetBookingWithTourByUserId(userId);

            var result = bookings.Select(b => new MyBookingDTO
            {
                BookingId = b.BookingId,
                TourTitle = b.Tour.TourName,
                TourImage = b.Tour.TourImages.FirstOrDefault() ?? "/img/packages-default.jpg",
                Rating = 4.0f, // Nếu có chấm sao thực tế thì bind sau
                Description = b.Tour.TourDescription,

                StartDate = b.Tour.StartDate,
                EndDate = b.Tour.EndDate,
                Destination = string.Join(", ", b.Tour.TourLocations.Select(tl => tl.Location.LocationName)),

                DurationDays = (b.Tour.EndDate - b.Tour.StartDate).Days,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                Code = b.Code,
                ShowPayButton = b.Status == BookingStatus.WAITING_FOR_PAYMENT,
                ShowCancelButton = b.Status == BookingStatus.CANCELLED,
                ShowSuccessMessage = b.Status == BookingStatus.CONFIRMED
            }).ToList();

            return new ResponseDTO("Lấy danh sách booking thành công", 200, true, result);
        }



        public static class BookingCodeGenerator
        {
            private static readonly Random _random = new Random();

            public static string GenerateUniqueBookingCode()
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // Đảm bảo từng giây là khác nhau
                var randomPart = GenerateRandomString(6); // Tạo chuỗi ngẫu nhiên

                return $"VN{timestamp}-{randomPart}";
            }

            private static string GenerateRandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
        }
        public async Task<ResponseDTO> GetAllBookingsAsync()
        {
            var bookings = await _unitOfWork.BookingRepo.GetAllAsync();

            var result = bookings.Select(b => new BookingManage
            {
                BookingId = b.BookingId,
                FullName = b.FullName,
                BookingCode = b.Code,
                Email = b.Email,
                TourName = b.Tour?.TourName ?? "Unknown",
                BookingDate = b.BookingDate,
                NumberOfAdults = b.NumberOfAdults,
                NumberOfChildren = b.NumberOfChildren,
                Status = b.Status
            }).ToList();

            return new ResponseDTO("Get all bookings successfully", 200, true, result);
        }
        public async Task<ResponseDTO> SearchBookingsByCodeAsync(string code)
        {
            var bookings = await _unitOfWork.BookingRepo.SearchByCodeAsync(code);

            var result = bookings.Select(b => new BookingManage
            {
                BookingId = b.BookingId,
                FullName = b.FullName,
                Email = b.Email,
                TourName = b.Tour?.TourName ?? "Unknown",
                BookingDate = b.BookingDate,
                NumberOfAdults = b.NumberOfAdults,
                NumberOfChildren = b.NumberOfChildren,
                Status = b.Status
            }).ToList();

            return new ResponseDTO("Search bookings successfully", 200, true, result);
        }

        public async Task<ResponseDTO> GetBookingForSeller()
        {
            try
            {
                var userId = _userUtility.GetUserIDFromToken(); // Lấy SellerId hiện tại

                var bookings = await _unitOfWork.BookingRepo.GetBookingsWithCustomerToursAsync();

                var filteredBookings = bookings
                    .Where(b => b.Tour != null
                                && b.Tour.Participants.Any(tp => tp.UserId == userId))  // Lọc theo participant
                    .Select(b => new BookingForCustomerDTO
                    {
                        BookingId = b.BookingId,
                        Code = b.Code,
                        FullName = b.FullName,
                        TourName = b.Tour.TourName,
                        StartDate = b.Tour.StartDate,
                        EndDate = b.Tour.EndDate,
                        NumberOfAdults = b.NumberOfAdults,
                        NumberOfChildren = b.NumberOfChildren,
                        Status = b.Status.ToString()
                    })
                    .ToList();

                return new ResponseDTO("Fetched bookings successfully", 200, true, filteredBookings);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error while fetching seller bookings", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> GetBookingDetailsForSellerAsync(Guid bookingId)
        {
            var booking = await _unitOfWork.BookingRepo.GetBookingWithDetailsForSellerAsync(bookingId);
            if (booking == null)
                return new ResponseDTO("Booking not found", 404, false, null);

            var dto = new BookingDetailsDTO
            {
                FullName = booking.FullName,
                Email = booking.Email,
                Phone = booking.PhoneNumber,
                Status = booking.Status.ToString(),
                BookingDate = booking.BookingDate,
                NumberOfGuests = booking.NumberOfAdults + booking.NumberOfChildren, // Số khách = Người lớn + Trẻ em
                TotalPrice = booking.TotalPrice,

                TourDetails = new TourDetailDTO
                {
                    TourId = booking.Tour.TourId,
                    TourName = booking.Tour.TourName,
                    TourDescription = booking.Tour.TourDescription,
                    StartDate = booking.Tour.StartDate,
                    EndDate = booking.Tour.EndDate,
                    ImageUrl = booking.Tour.TourImages.FirstOrDefault(),
                    // ✅ Danh sách Location chi tiết
                    Location = booking.Tour.TourLocations.Select(tl => new LocationRequestJsonDTO
                    {
                        LocationName = tl.Location.LocationName,
                        Latitude = tl.Location.Latitude,
                        Longitude = tl.Location.Longitude
                    }).ToList(),
                    PriceOfAdult = booking.Tour.PriceOfAdult,
                    PriceOfChild = booking.Tour.PriceOfChild,

                    Type = booking.Tour.Type.ToString()
                },

                Itineraries = booking.Tour.Itineraries.Select(itinerary => new ItineraryDTO
                {
                    Name = itinerary.Name,

                    StartTime = itinerary.StartTime,
                    EndTime = itinerary.EndTime,
                    Activities = itinerary.Activities.Select(act => new ActivityDTO
                    {
                        Description = act.Description,
                        StartTime = act.StartTime,
                        EndTime = act.EndTime
                    }).ToList()
                }).ToList(),

                IncludedServices = booking.Tour.Included.Select(i => new IncludedDTO
                {
                    Description = i.Description
                }).ToList(),

                NotIncludedServices = booking.Tour.NotIncluded.Select(n => new NotIncludedDTO
                {
                    Description = n.Description
                }).ToList()
            };

            return new ResponseDTO("Get booking details successfully", 200, true, dto);
        }

        public async Task<ResponseDTO> ChangeStatusForManager(Guid bookingId, BookingStatus bookingStatus)
        {
            var booking = await _unitOfWork.BookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return new ResponseDTO("Booking not found", 404, false);
            }
            booking.Status = bookingStatus;


            await _unitOfWork.SaveChangeAsync();
            return new ResponseDTO("Booking status updated successfully", 200, true, booking.BookingId);
        }
    }
}
