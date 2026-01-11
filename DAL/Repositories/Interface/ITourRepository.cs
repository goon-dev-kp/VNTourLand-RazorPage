using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ITourRepository : IGenericRepository<Tour>
    {
        Task<IEnumerable<Tour>> GetAllByType(TourType type);
        Task<Tour> GetTourDetailByIdAsync(Guid tourId);
        Task<IEnumerable<Tour>> GetAllWithLocationAsync();
        Task<IEnumerable<Tour>> GetAllWithLocationAndParticipantsAsync();
    }
}
