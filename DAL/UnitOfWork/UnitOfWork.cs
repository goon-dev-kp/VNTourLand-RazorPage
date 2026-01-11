using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Repositories.Implement;
using DAL.Repositories.Interface;

namespace DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool disposed = false;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            UserRepo = new UserRepository(_context);
            BlogRepo = new BlogRepository(_context);
            TransactionRepo = new TransactionRepository(_context);
            ReviewerRepo = new ReviewerRepository(_context);
            TourRepo = new TourRepository(_context);
            TokenRepo = new TokenRepository(_context);
            BookingRepo = new BookingRepository(_context);
            LocationRepo = new LocationRepository(_context);
            IncludedRepo = new IncludedRepository(_context);
            ItineraryRepo = new ItineraryRepository(_context);
            NotIncludedRepo = new NotIncludedRepository(_context);
            TourLocationRepo = new TourLocationRepository(_context);
            ActivityRepo = new ActivityRepository(_context);
            BlogCategoryRepo = new BlogCategoryRepository(_context);
            //AddOnOptionRepo = new AddOnOptionRepository(_context);
            //OptionOnTourRepo = new OptionOnTourRepositorty(_context);
            MessageRepo = new MessageRepository(_context);
            StoryRepo = new StoryRepository(_context);
            LocationOfStoryRepo = new LocationOfStoryRepository(_context);
            TourParticipantRepo = new TourParticipantRepository(_context);
            ContactRepo = new ContactRepository(_context);
        }

        public IUserRepository UserRepo { get; private set; }
        public IBlogRepository BlogRepo { get; private set; }
        public ITransactionRepository TransactionRepo { get; private set; }
        public IReviewerRepository ReviewerRepo { get; private set; }
        public ITourRepository TourRepo { get; private set; }
        public ITokenRepository TokenRepo { get; private set; }
        public IBookingRepository BookingRepo { get; private set; }
        public ILocationRepository LocationRepo { get; private set; }
        public IIncludedRepository IncludedRepo { get; private set; }
        public IItineraryRepository ItineraryRepo { get; private set; }
        public INotIncludedRepository NotIncludedRepo { get; private set; }
        public ITourLocationRepository TourLocationRepo { get; private set; }
        public IActivityRepository ActivityRepo { get; private set; }
        public IBlogCategoryRepository BlogCategoryRepo { get; private set; }
        //public IAddOnOptionRepository AddOnOptionRepo { get; private set; }
        //public IOptionOnTourRepositorty OptionOnTourRepo { get; private set; }
        public IMessageRepository MessageRepo { get; private set; }
        public IStoryRepository StoryRepo { get; private set; }
        public ILocationOfStoryRepository LocationOfStoryRepo { get; private set; }
        public ITourParticipantRepository TourParticipantRepo { get; private set; }
        public IContactRepository ContactRepo { get; private set; }
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
