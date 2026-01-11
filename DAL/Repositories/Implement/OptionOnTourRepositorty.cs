//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Common.DTO;
//using DAL.Data;
//using DAL.Models;
//using DAL.Repositories.Interface;
//using Microsoft.EntityFrameworkCore;

//namespace DAL.Repositories.Implement
//{
//    public class OptionOnTourRepositorty : GenericRepository<OptionOnTour>, IOptionOnTourRepositorty
//    {
//        private readonly ApplicationDbContext _context;

//        public OptionOnTourRepositorty (ApplicationDbContext context) : base (context)
//        {
//            _context = context;
//        }
//        public async Task<List<AddOptionDTO>> GetAddOnOptionsByBookingIdAsync(Guid bookingId)
//        {
//            return await _context.OptionOnTours
//                .Where(x => x.BookingId == bookingId)
//                .Include(x => x.AddOnOption)
//                .Select(x => new AddOptionDTO
//                {
//                    Name = x.AddOnOption.Name,
//                    Price = x.AddOnOption.Price
//                })
//                .ToListAsync();
//        }

//    }
//}
