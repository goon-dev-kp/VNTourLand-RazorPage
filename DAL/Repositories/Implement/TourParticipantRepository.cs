using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;

namespace DAL.Repositories.Implement
{
    public class TourParticipantRepository : GenericRepository<TourParticipant>, ITourParticipantRepository
    {
        private readonly ApplicationDbContext _context;
        public TourParticipantRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
