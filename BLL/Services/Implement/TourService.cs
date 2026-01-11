using BLL.Hubs;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Enums;
using DAL.Models;
using DAL.Repositories.Interface;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implement
{
    public class TourService : ITourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly UserUtility _userUtility;
        private readonly IHubContext<ReloadHub> _reloadHub;

        public TourService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService, UserUtility userUtility, IHubContext<ReloadHub> reloadHub)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _userUtility = userUtility;
            _reloadHub = reloadHub;
        }

        public async Task<ResponseDTO> GetAllTourByType(string type)
        {
            try
            {
                if (!Enum.TryParse<TourType>(type, true, out var parsedType))
                {
                    return new ResponseDTO("Invalid tour type", 400, false);
                }

                var tours = await _unitOfWork.TourRepo.GetAllByType(parsedType);

                // Map sang DTO
                var tourDTOs = tours.Select(t => new TourDTO
                {
                    TourId = t.TourId,
                    TourName = t.TourName,
                    TourDescription = t.TourDescription,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    PriceOfAdult = t.PriceOfAdult,
                    PriceOfChild = t.PriceOfChild,
      
                    Type = t.Type.ToString(),
                    ImageUrl = t.TourImages.FirstOrDefault(),
                    Location = t.TourLocations
                        .Where(tl => tl.Location != null)
                        .Select(tl => tl.Location.LocationName)
                        .ToList(),

                }).ToList();

                return new ResponseDTO("Get all tours by type successfully", 200, true, tourDTOs);

            }
            catch (Exception ex)
            {
                return new ResponseDTO("An error occurred while getting tours by type", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> GetTourDetailByIdAsync(Guid tourId)
        {
            try
            {
                var tour = await _unitOfWork.TourRepo.GetTourDetailByIdAsync(tourId);

                if (tour == null)
                {
                    return new ResponseDTO("Tour not found", 404, false);
                }

                var dto = new TourDetailDTO
                {
                    TourId = tour.TourId,
                    TourName = tour.TourName,
                    TourDescription = tour.TourDescription,
                    StartDate = tour.StartDate,
                    EndDate = tour.EndDate,
                    PriceOfAdult = tour.PriceOfAdult,
                    PriceOfChild = tour.PriceOfChild,
                    CustomAddOnFee = tour.CustomAddOnFee ?? 0,
                    CustomAddOnNote = tour.CustomAddOnNote,
                    Type = tour.Type.ToString(),
                    ImageUrl = tour.TourImages.FirstOrDefault(),
                    // ✅ Thay vì 1 location, trả về danh sách đầy đủ
                    Location = tour.TourLocations.Select(tl => new LocationRequestJsonDTO
                    {
                        LocationName = tl.Location.LocationName,
                        Latitude = tl.Location.Latitude,
                        Longitude = tl.Location.Longitude
                    }).ToList()

                };

                return new ResponseDTO("Get tour detail successfully", 200, true, dto);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("An error occurred while getting tour detail", 500, false, ex.Message);
            }
        }
        public async Task<ResponseDTO> GetAllTour()
        {
            try
            {
                var tours = await _unitOfWork.TourRepo.GetAllWithLocationAsync();
                var tourDTOs = tours.Select(t => new TourDTO
                {
                    TourId = t.TourId,
                    TourName = t.TourName,
                    TourDescription = t.TourDescription,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    PriceOfAdult = t.PriceOfAdult,
                    PriceOfChild = t.PriceOfChild,

                    Type = t.Type.ToString(),
                    ImageUrl = t.TourImages.FirstOrDefault(),
                    Status = t.Status.ToString(),
                    Location = t.TourLocations.Where(tl => tl.Location != null).Select(tl => tl.Location.LocationName).ToList(),
                    IsActive = t.IsActive
                }).ToList();

                return new ResponseDTO("Get all tours successfully", 200, true, tourDTOs);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("An error occurred while getting all tours", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> CreateTourSellerAsync(CreateTourSellerRequest request)
        {
            try
            {
                var userId = _userUtility.GetUserIDFromToken();
                if (userId == Guid.Empty)
                {
                    return new ResponseDTO("User is not logged in", 401, false);
                }

                // Upload image
                var imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(request.Image);

                // Create Tour entity
                var tourId = Guid.NewGuid();
                var tour = new Tour
                {
                    TourId = tourId,
                    TourName = request.TourName,
                    RequestId = request.RequestId,
                    TourDescription = request.TourDescription,
                    StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                    PriceOfAdult = request.PriceOfAdult,
                    PriceOfChild = request.PriceOfChild,
                    CustomAddOnNote = request.CustomAddOnNote,
                    CustomAddOnFee = request.CustomAddOnFee ?? 0,
                    Type = Enum.Parse<TourType>(request.Type),
                    TourImages = new List<string> { imageUrl },
                    Status = TourStatus.ACTIVE
                };

                await _unitOfWork.TourRepo.AddAsync(tour);

                foreach (var locRequest in request.Locations)
                {
                    var normalized = locRequest.LocationName.Trim().ToLower();
                    var similarLocations = await _unitOfWork.LocationRepo.GetAllAsync(
                        l => l.LocationName.ToLower().Contains(normalized) || normalized.Contains(l.LocationName.ToLower()));

                    Location location = similarLocations.FirstOrDefault();
                    if (location == null)
                    {
                        location = new Location
                        {
                            LocationId = Guid.NewGuid(),
                            LocationName = locRequest.LocationName.Trim(),
                            Latitude = locRequest.Latitude,
                            Longitude = locRequest.Longitude
                        };
                        await _unitOfWork.LocationRepo.AddAsync(location);
                    }

                    var tourLocation = new TourLocation
                    {
                        TourLocationId = Guid.NewGuid(),
                        TourId = tourId,
                        LocationId = location.LocationId,
                        Location = location
                    };
                    await _unitOfWork.TourLocationRepo.AddAsync(tourLocation);
                }


                foreach (var i in request.IncludedItems)
                {
                    await _unitOfWork.IncludedRepo.AddAsync(new Included
                    {
                        IncludedId = Guid.NewGuid(),
                        TourId = tourId,
                        Description = i
                    });
                }

                foreach (var n in request.NotIncludedItems)
                {
                    await _unitOfWork.NotIncludedRepo.AddAsync(new NotIncluded
                    {
                        NotIncludedId = Guid.NewGuid(),
                        TourId = tourId,
                        Description = n
                    });
                }

                foreach (var it in request.Itineraries)
                {
                    var itineraryId = Guid.NewGuid();
                    var itinerary = new Itinerary
                    {
                        ItineraryId = itineraryId,
                        TourId = tourId,
                        Name = it.Name,
                        StartTime = DateTime.SpecifyKind(it.StartTime, DateTimeKind.Utc),
                        EndTime = DateTime.SpecifyKind(it.EndTime, DateTimeKind.Utc)
                    };
                    await _unitOfWork.ItineraryRepo.AddAsync(itinerary);

                    foreach (var a in it.Activities)
                    {
                        await _unitOfWork.ActivityRepo.AddAsync(new Activity
                        {
                            ActivityId = Guid.NewGuid(),
                            ItineraryId = itineraryId,
                            Description = a.Description,
                            StartTime = DateTime.SpecifyKind(a.StartTime, DateTimeKind.Utc),
                            EndTime = DateTime.SpecifyKind(a.EndTime, DateTimeKind.Utc)
                        });
                    }
                }

                var tourParticipant = new TourParticipant
                {
                    TourParticipantId = Guid.NewGuid(),
                    TourId = tourId,
                    UserId = request.CustomerId,
                    Role = ParticipantRole.CUSTOMER,
                    AssignedAt = DateTime.UtcNow
                };
                await _unitOfWork.TourParticipantRepo.AddAsync(tourParticipant);

                var tourSeller = new TourParticipant
                {
                    TourParticipantId = Guid.NewGuid(),
                    TourId = tourId,
                    UserId = userId,
                    Role = ParticipantRole.SELLER,
                    AssignedAt = DateTime.UtcNow
                };
                await _unitOfWork.TourParticipantRepo.AddAsync(tourSeller);

                await _unitOfWork.SaveChangeAsync();
                await _reloadHub.Clients.All.SendAsync("ReloadPage");
                return new ResponseDTO("Tour created successfully", 201, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Failed to create tour", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> CreateTourAsync(CreateTourRequest request)
        {
            try
            {
                var imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(request.Image);

                var tourId = Guid.NewGuid();
                var tour = new Tour
                {
                    TourId = tourId,
                    TourName = request.TourName,
                    TourDescription = request.TourDescription,
                    StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                    PriceOfAdult = 1,
                    PriceOfChild = 1,
                    Type = Enum.Parse<TourType>(request.Type),
                    TourImages = new List<string> { imageUrl },
                    Status = TourStatus.ACTIVE
                };

                await _unitOfWork.TourRepo.AddAsync(tour);

                foreach (var locRequest in request.Locations)
                {
                    var normalized = locRequest.LocationName.Trim().ToLower();
                    var similarLocations = await _unitOfWork.LocationRepo.GetAllAsync(
                        l => l.LocationName.ToLower().Contains(normalized) || normalized.Contains(l.LocationName.ToLower()));

                    Location location = similarLocations.FirstOrDefault();
                    if (location == null)
                    {
                        location = new Location
                        {
                            LocationId = Guid.NewGuid(),
                            LocationName = locRequest.LocationName.Trim(),
                            Latitude = locRequest.Latitude,
                            Longitude = locRequest.Longitude
                        };
                        await _unitOfWork.LocationRepo.AddAsync(location);
                    }

                    var tourLocation = new TourLocation
                    {
                        TourLocationId = Guid.NewGuid(),
                        TourId = tourId,
                        LocationId = location.LocationId,
                        Location = location
                    };
                    await _unitOfWork.TourLocationRepo.AddAsync(tourLocation);
                }


                foreach (var i in request.IncludedItems)
                {
                    await _unitOfWork.IncludedRepo.AddAsync(new Included
                    {
                        IncludedId = Guid.NewGuid(),
                        TourId = tourId,
                        Description = i
                    });
                }

                foreach (var n in request.NotIncludedItems)
                {
                    await _unitOfWork.NotIncludedRepo.AddAsync(new NotIncluded
                    {
                        NotIncludedId = Guid.NewGuid(),
                        TourId = tourId,
                        Description = n
                    });
                }

                foreach (var it in request.Itineraries)
                {
                    var itineraryId = Guid.NewGuid();
                    var itinerary = new Itinerary
                    {
                        ItineraryId = itineraryId,
                        TourId = tourId,
                        Name = it.Name,
                        StartTime = DateTime.SpecifyKind(it.StartTime, DateTimeKind.Utc),
                        EndTime = DateTime.SpecifyKind(it.EndTime, DateTimeKind.Utc)
                    };
                    await _unitOfWork.ItineraryRepo.AddAsync(itinerary);

                    foreach (var a in it.Activities)
                    {
                        await _unitOfWork.ActivityRepo.AddAsync(new Activity
                        {
                            ActivityId = Guid.NewGuid(),
                            ItineraryId = itineraryId,
                            Description = a.Description,
                            StartTime = DateTime.SpecifyKind(a.StartTime, DateTimeKind.Utc),
                            EndTime = DateTime.SpecifyKind(a.EndTime, DateTimeKind.Utc)
                        });
                    }
                }

                await _unitOfWork.SaveChangeAsync();
                await _reloadHub.Clients.All.SendAsync("ReloadPage");
                return new ResponseDTO("Tour created successfully", 201, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Failed to create tour", 500, false, ex.Message);
            }
        }

        public async Task<List<TourLocationDTO>> GetToursNearAsync(double lat, double lng, double radiusKm)
        {
            // Lấy toàn bộ các tour location, include Location và Tour
            var tourLocations = await _unitOfWork.TourLocationRepo
                .GetAllAsync(tl => true, includeProperties: "Location,Tour");

            var results = tourLocations
                .Where(tl =>
                {
                    var distance = Haversine(lat, lng, tl.Location.Latitude, tl.Location.Longitude);
                    return distance <= radiusKm;
                })
                .Select(tl => new TourLocationDTO
                {
                    TourId = tl.TourId,
                    TourName = tl.Tour.TourName,
                    Latitude = tl.Location.Latitude,
                    Longitude = tl.Location.Longitude,
                    ImageURL = tl.Tour.TourImages.FirstOrDefault() ?? "",
                })
                .ToList();

            return results;
        }

        private double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371; // Earth radius in km
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public async Task<ResponseDTO> EnableTourAsync(Guid tourId)
        {
            var tour = await _unitOfWork.TourRepo.GetByIdAsync(tourId);
            if (tour == null)
                return new ResponseDTO("Tour not found", 404, false);

            tour.IsActive = true;
            await _unitOfWork.SaveChangeAsync();
            await _reloadHub.Clients.All.SendAsync("ReloadPage");

            return new ResponseDTO("Tour enabled successfully", 200, true);
        }

        public async Task<ResponseDTO> DisableTourAsync(Guid tourId)
        {
            var tour = await _unitOfWork.TourRepo.GetByIdAsync(tourId);
            if (tour == null)
                return new ResponseDTO("Tour not found", 404, false);

            tour.IsActive = false;
            await _unitOfWork.SaveChangeAsync();
            await _reloadHub.Clients.All.SendAsync("ReloadPage");

            return new ResponseDTO("Tour disabled successfully", 200, true);
        }
        public async Task<ResponseDTO> UpdateTourByIdAsync(UpdateTourRequest request)
        {
            try
            {
                var tour = await _unitOfWork.TourRepo.GetTourDetailByIdAsync(request.TourId);
                if (tour == null)
                    return new ResponseDTO("Tour not found", 404, false);

                // Xử lý ảnh
                string imageUrl = tour.TourImages?.FirstOrDefault() ?? "";
                if (request.NewImage != null)
                {
                    imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(request.NewImage);
                }
                tour.TourImages = new List<string> { imageUrl };

                // Cập nhật các trường còn lại
                tour.TourName = request.TourName;
                tour.TourDescription = request.TourDescription;
                tour.StartDate = request.StartDate;
                tour.EndDate = request.EndDate;
                tour.PriceOfAdult = request.PriceOfAdult;
                tour.PriceOfChild = request.PriceOfChild;
        
                tour.IsActive = request.IsActive;
                tour.Type = Enum.Parse<TourType>(request.Type);
                tour.Status = Enum.Parse<TourStatus>(request.Status);

                // Xoá hết các TourLocation cũ
                var currentTourLocations = tour.TourLocations.ToList();
                foreach (var tl in currentTourLocations)
                {
                    _unitOfWork.TourLocationRepo.Delete(tl);
                }

                // Xử lý từng Location trong danh sách mới
                foreach (var locReq in request.Locations)
                {
                    var locationName = locReq.LocationName.Trim().ToLower();

                    var existingLocation = await _unitOfWork.LocationRepo
                        .FirstOrDefaultAsync(l => l.LocationName.ToLower() == locationName);

                    Location location;
                    if (existingLocation != null)
                    {
                        location = existingLocation;
                    }
                    else
                    {
                        location = new Location
                        {
                            LocationId = Guid.NewGuid(),
                            LocationName = locReq.LocationName.Trim()
                        };
                        await _unitOfWork.LocationRepo.AddAsync(location);
                    }

                    var newTourLocation = new TourLocation
                    {
                        TourId = tour.TourId,
                        LocationId = location.LocationId
                    };
                    await _unitOfWork.TourLocationRepo.AddAsync(newTourLocation);
                }


                await _unitOfWork.SaveChangeAsync();
                await _reloadHub.Clients.All.SendAsync("ReloadPage");
                return new ResponseDTO("Update Tour Successfully", 200, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new ResponseDTO("Error :", 500, false, ex.Message);
              
            }
        }

        public async Task<ResponseDTO> GetTourByUserId()
        {
            try
            {
                var userId = _userUtility.GetUserIDFromToken();
                if (userId == Guid.Empty)
                {
                    return new ResponseDTO("User is not logged in", 401, false);
                }

                // Lấy tất cả tour kèm location và participants
                var tours = await _unitOfWork.TourRepo.GetAllWithLocationAndParticipantsAsync();

                // Lọc ra những tour có user tham gia
                var userTours = tours
                    .Where(t => t.Participants.Any(p => p.UserId == userId))
                    .ToList();

                var tourDTOs = userTours.Select(t => new TourDTO
                {
                    TourId = t.TourId,
                    TourName = t.TourName,
                    TourDescription = t.TourDescription,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    PriceOfAdult = t.PriceOfAdult,
                    PriceOfChild = t.PriceOfChild,
                    CustomAddOnFee = t.CustomAddOnFee ?? 0,
                    CustomAddOnNote = t.CustomAddOnNote,
                    Type = t.Type.ToString(),
                    ImageUrl = t.TourImages.FirstOrDefault(),
                    Status = t.Status.ToString(),
                    Location = t.TourLocations
                        .Where(tl => tl.Location != null)
                        .Select(tl => tl.Location.LocationName)
                        .ToList(),

                    IsActive = t.IsActive
                }).ToList();

                return new ResponseDTO("Get tours by user successfully", 200, true, tourDTOs);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("An error occurred while getting user tours", 500, false, ex.Message);
            }
        }

        public async Task<ResponseDTO> DeleteTourAsync(Guid tourId)
        {
            if (tourId == Guid.Empty)
            {
                return new ResponseDTO("Error", 400, false);
            }
            try
            {
                await _unitOfWork.TourRepo.DeleteAsync(tourId);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                return new ResponseDTO("An error occurred while getting user tours", 500, false, ex.Message);
            }
            await _reloadHub.Clients.All.SendAsync("ReloadPage");

            return new ResponseDTO("Delete successfully", 200, true);

        }
    }

}



