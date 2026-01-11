using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repositories.Interface;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepo { get; }
        IBlogRepository BlogRepo { get; }
        ITransactionRepository TransactionRepo { get; }
        IReviewerRepository ReviewerRepo { get; }
        ITourRepository TourRepo { get; }
        ITokenRepository TokenRepo { get; }
        IBookingRepository BookingRepo { get; }
        ILocationRepository LocationRepo { get; }
        IItineraryRepository ItineraryRepo { get; }
        IIncludedRepository IncludedRepo { get; }
        INotIncludedRepository NotIncludedRepo { get; }
        ITourLocationRepository TourLocationRepo { get; }
        IActivityRepository ActivityRepo { get; }
        IBlogCategoryRepository BlogCategoryRepo { get; }
        //IAddOnOptionRepository AddOnOptionRepo { get; }
        //IOptionOnTourRepositorty OptionOnTourRepo { get; }
        IMessageRepository MessageRepo { get; }
        IStoryRepository StoryRepo { get; }
        ILocationOfStoryRepository LocationOfStoryRepo { get; }
        ITourParticipantRepository TourParticipantRepo { get; }
        IContactRepository ContactRepo { get; }
        Task<int> SaveAsync();
        Task<bool> SaveChangeAsync();
    }
}
