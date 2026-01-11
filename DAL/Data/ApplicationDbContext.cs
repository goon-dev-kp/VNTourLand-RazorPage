using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<Itinerary> Itineraries { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Included> IncludedItems { get; set; }
        public DbSet<NotIncluded> NotIncludedItems { get; set; }
        //public DbSet<AddOnOption> AddOnOptions { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Reviewer> Reviewers { get; set; }
        //public DbSet<OptionOnTour> OptionOnTours { get; set; }
        public DbSet<TourLocation> TourLocations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<RequestOfCustomer> RequestOfCustomer { get; set; }
        public DbSet<LocationOfStory> locationOfStories { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<TourParticipant> TourParticipants { get; set; }
        public DbSet<Contact>  Contacts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Activity>().HasKey(a => a.ActivityId);
            modelBuilder.Entity<Booking>().HasKey(b => b.BookingId);
            modelBuilder.Entity<Included>().HasKey(i => i.IncludedId);
            modelBuilder.Entity<NotIncluded>().HasKey(n => n.NotIncludedId);
            //modelBuilder.Entity<AddOnOption>().HasKey(a => a.AddOnOptionId);
            modelBuilder.Entity<Tour>().HasKey(t => t.TourId);
            modelBuilder.Entity<Transaction>().HasKey(t => t.TransactionId);
            modelBuilder.Entity<Itinerary>().HasKey(i => i.ItineraryId);
            modelBuilder.Entity<Location>().HasKey(l => l.LocationId);
            modelBuilder.Entity<Blog>().HasKey(p => p.BlogId);
            modelBuilder.Entity<Reviewer>().HasKey(r => r.ReviewerId);
            modelBuilder.Entity<RefreshToken>().HasKey(rt => rt.RefreshTokenId);
            modelBuilder.Entity<TourLocation>().HasKey(rt => rt.TourLocationId);
            modelBuilder.Entity<BlogCategory>().HasKey(rt => rt.BlogCategoryId);
            //modelBuilder.Entity<OptionOnTour>().HasKey(rt => rt.OptionOnTourId);
            modelBuilder.Entity<Message>().HasKey(m => m.MessageId);
            modelBuilder.Entity<RequestOfCustomer>().HasKey(m => m.RequestId);
            modelBuilder.Entity<Story>().HasKey(st => st.StoryId);
            modelBuilder.Entity<LocationOfStory>().HasKey(st => st.LocationOfStoryId);
            modelBuilder.Entity<TourParticipant>().HasKey(tp => tp.TourParticipantId);
            modelBuilder.Entity<Contact>().HasKey(c => c.ContactId);

            // Relationships

            // RefreshToken - User (1-n)
            modelBuilder.Entity<RefreshToken>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Booking (1-n)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            // Booking - Tour (n-1)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tour)
                .WithMany(t => t.Bookings) // Liên kết đến collection Bookings trong Tour
                .HasForeignKey(b => b.TourId);


            // Tour - Itinerary (1-n)
            modelBuilder.Entity<Itinerary>()
                .HasOne(i => i.Tour)
                .WithMany(t => t.Itineraries)
                .HasForeignKey(i => i.TourId);

            // Itinerary - Activity (1-n)
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Itinerary)
                .WithMany(i => i.Activities)
                .HasForeignKey(a => a.ItineraryId);

            //// Tour - OptionOnTour - AddOfTionOnTour (1-n)
            //modelBuilder.Entity<OptionOnTour>()
            //    .HasOne(o => o.Booking)
            //    .WithMany(b => b.OptionOnTours)
            //    .HasForeignKey(o => o.BookingId);

            //modelBuilder.Entity<OptionOnTour>()
            //    .HasOne(o => o.AddOnOption)
            //    .WithMany(a => a.OptionOnTours)
            //    .HasForeignKey(o => o.AddOnOptionId);

            // Tour - Included (1-n)
            modelBuilder.Entity<Included>()
                .HasOne(i => i.Tour)
                .WithMany(t => t.Included)
                .HasForeignKey(i => i.TourId);

            // Tour - NotIncluded (1-n)
            modelBuilder.Entity<NotIncluded>()
                .HasOne(n => n.Tour)
                .WithMany(t => t.NotIncluded)
                .HasForeignKey(n => n.TourId);

            // Booking - Transaction (1-N)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Transactions) // Booking có nhiều Transactions
                .HasForeignKey(t => t.BookingId);


            // Tour - Location (1-n)
            modelBuilder.Entity<TourLocation>()
                .HasOne(tl => tl.Tour)
                .WithMany(t => t.TourLocations)
                .HasForeignKey(tl => tl.TourId);

            modelBuilder.Entity<TourLocation>()
                .HasOne(tl => tl.Location)
                .WithMany(l => l.TourLocations)
                .HasForeignKey(tl => tl.LocationId);

            // Post - Reviewer (1-n)
            modelBuilder.Entity<Reviewer>()
                 .HasOne(r => r.Blog)
                 .WithMany(b => b.Reviewers)
                 .HasForeignKey(r => r.BlogId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reviewer>()
                .HasOne(r => r.Story)
                .WithMany(s => s.Reviewers)
                .HasForeignKey(r => r.StoryId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Reviewer>()
                .HasOne(r => r.Tour)
                .WithMany(t => t.Reviewers)
                .HasForeignKey(r => r.TourId)
                .OnDelete(DeleteBehavior.Restrict);

            // reviewer - user (1-n)
            modelBuilder.Entity<Reviewer>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviewers) // nếu bạn có ICollection<Reviewer> trong User
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // hoặc Restrict nếu bạn muốn giữ đánh giá khi xóa user

            //Message - user 
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);




            //Blog - Category (1-n)
            // Blog - BlogCategory (n-1)
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.BlogCategory)
                .WithMany(c => c.Blogs)
                .HasForeignKey(b => b.BlogCategoryId);

            //user - TourParticipant (1-n)
            modelBuilder.Entity<TourParticipant>()
                .HasOne(tp => tp.Tour)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TourId);

            modelBuilder.Entity<TourParticipant>()
                .HasOne(tp => tp.User)
                .WithMany(u => u.TourParticipants)
                .HasForeignKey(tp => tp.UserId);

            // User - RequestOfCustomer (1-n)
            // User - RequestOfCustomer (1-n)
            modelBuilder.Entity<RequestOfCustomer>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.RequestsOfCustomer)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình 1 Location có nhiều Story
            modelBuilder.Entity<Story>()
                .HasOne(s => s.LocationOfStory)
                .WithMany(l => l.Stories)
                .HasForeignKey(s => s.LocationOfStoryId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Location sẽ xóa Story liên quan


            // Enum conversions
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<int>();

            var stringListComparer = new ValueComparer<List<string>>(
         (c1, c2) => c1.SequenceEqual(c2),
         c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
         c => c.ToList()
     );

            modelBuilder.Entity<LocationOfStory>()
                .Property(e => e.BannerImageUrl)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .Metadata.SetValueComparer(stringListComparer); // ✅ THÊM comparer



            base.OnModelCreating(modelBuilder);
            DbSeeder.Seed(modelBuilder);

        }

    }


}
