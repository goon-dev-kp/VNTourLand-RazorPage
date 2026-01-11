using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;

        // User-IDs
        private static readonly Guid AdminID = Guid.Parse("D4DAB1C3-6D48-4B23-8369-2D1C9C828F22");
        private static readonly Guid UserID = Guid.Parse("A1DAB1C3-6D48-4B23-8369-2D1C9C828F22");
        private static readonly Guid ManagerID = Guid.Parse("B2DAB1C3-6D48-4B23-8369-2D1C9C828F22");
        private static readonly Guid SellerID = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF5");
        private static readonly Guid ManagerID2 = Guid.Parse("14567890-ABCD-EF12-3456-7890ABCDEFAB");
        private static readonly Guid SellerID2 = Guid.Parse("F4567890-ABCD-EF12-3456-7890ABCDEFAB");

        //BlogCategory
        private static readonly Guid Cat1 = Guid.Parse("A1E2C3D4-5F6A-7B8C-9D0E-1F2A3B4C5D6E");
        private static readonly Guid Cat2 = Guid.Parse("B2D3E4F5-6A7B-8C9D-0E1F-2A3B4C5D6E7F");
        private static readonly Guid Cat3 = Guid.Parse("C3D4E5F6-7A8B-9C0D-1E2F-3A4B5C6D7E8F");
        private static readonly Guid Cat4 = Guid.Parse("D4E5F6A7-8B9C-0D1E-2F3A-4B5C6D7E8F9A");
        private static readonly Guid Cat5 = Guid.Parse("E5F6A7B8-9C0D-1E2F-3A4B-5C6D7E8F9A0B");
        private static readonly Guid Cat6 = Guid.Parse("F6A7B8C9-0D1E-2F3A-4B5C-6D7E8F9A0B1C");
        private static readonly Guid Cat7 = Guid.Parse("A7B8C9D0-1E2F-3A4B-5C6D-7E8F9A0B1C2D");
        private static readonly Guid Cat8 = Guid.Parse("B8C9D0E1-2F3A-4B5C-6D7E-8F9A0B1C2D3E");
        private static readonly Guid Cat9 = Guid.Parse("C9D0E1F2-3A4B-5C6D-7E8F-9A0B1C2D3E4F");
        private static readonly Guid Cat10 = Guid.Parse("D0E1F2A3-4B5C-6D7E-8F9A-0B1C2D3E4F5A");

        //AddOnOption
        private static readonly Guid Option1 = Guid.Parse("E1F2A3B4-5C6D-7E8F-9A0B-1C2D3E4F5A6B");
        private static readonly Guid Option2 = Guid.Parse("F2A3B4C5-6D7E-8F9A-0B1C-2D3E4F5A6B7C");
        private static readonly Guid Option3 = Guid.Parse("A3B4C5D6-7E8F-9A0B-1C2D-3E4F5A6B7C8D");

        //Message
        private static readonly Guid Message1 = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF4");
        private static readonly Guid Message2 = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3");

        public DbSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public static void Seed (ModelBuilder modelBuilder)
        {
            SeedUser(modelBuilder);
            SeedCategory(modelBuilder);
            //SeesAddOnOption(modelBuilder);
            SeedMessage(modelBuilder);
        }
        private static void SeedMessage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>().HasData(
                new Message
                {
                    MessageId = Message1,
                    SenderId = ManagerID,
                    ReceiverId = SellerID,
                    Content = "Welcome to our travel platform! How can we assist you today?",
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                },
                new Message
                {
                    MessageId = Message2,
                    SenderId = SellerID,
                    ReceiverId = ManagerID,
                    Content = "Thank you! I'm looking for recommendations on travel destinations.",
                    SentAt = DateTime.UtcNow.AddMinutes(5),
                    IsRead = false
                }
            );
        }

        //private static void SeesAddOnOption(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<AddOnOption>().HasData(
        //        new AddOnOption
        //        {
        //            AddOnOptionId = Option1,
        //            Name = "Private Tour",
        //            Description = "Experience a personalized tour with a private guide.",
        //            Price = 100.00m
        //        },
                
        //        new AddOnOption
        //        {
        //            AddOnOptionId = Option3,
        //            Name = "Photography Package",
        //            Description = "Capture your memories with a professional photography package.",
        //            Price = 200.00m
        //        }
        //    );
        //}
        private static void SeedCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlogCategory>().HasData(
                new BlogCategory
                {
                    BlogCategoryId = Cat1,
                    CategoryName = "Travel Tips & Advice"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat2,
                    CategoryName = "Trip Reviews & Travel Diaries"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat3,
                    CategoryName = "Local Cuisine & Food Tours"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat4,
                    CategoryName = "Top Destinations"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat5,
                    CategoryName = "Packing & Travel Planning"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat6,
                    CategoryName = "Photography & Instagram Spots"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat7,
                    CategoryName = "Inspirational Travel Stories"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat8,
                    CategoryName = "International Travel"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat9,
                    CategoryName = "Thematic Travel (e.g., spiritual, adventure, wellness)"
                },
                new BlogCategory
                {
                    BlogCategoryId = Cat10,
                    CategoryName = "Travel News & Trends"
                }
                );
        }
        private static void SeedUser (ModelBuilder modelBuilder)
        {
            string fixedHashedPassword = "$2a$11$rTz6DZiEeBqhVrzF25CgTOBPf41jpn2Tg/nnIqnX8KS6uIerB/1dm";
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = AdminID,
                    UserName = "Admin",
                    Password = fixedHashedPassword,
                    Email = "admin@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    Role = RoleType.ADMIN,
                    IsActive = true

                },
                new User
                {
                    UserId = ManagerID,
                    UserName = "Manager",
                    Password = fixedHashedPassword,
                    Email = "manager@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,

                    Role = RoleType.MANAGER,
                    IsActive = true


                },
                new User
                {
                    UserId = UserID,
                    UserName = "User",
                    Password = fixedHashedPassword,
                    Email = "user@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,

                    Role = RoleType.USER,
                    IsActive = true


                },
                new User
                {
                    UserId = SellerID,
                    UserName = "Seller",
                    Password = fixedHashedPassword,
                    Email = "seller@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    Role = RoleType.SELLER,
                    IsActive = true


                },
                new User
                {
                    UserId = SellerID2,
                    UserName = "Seller-2",
                    Password = fixedHashedPassword,
                    Email = "seller-2@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    Role = RoleType.SELLER,
                    IsActive = true


                },
                new User
                {
                    UserId = ManagerID2,
                    UserName = "Manager-2",
                    Password = fixedHashedPassword,
                    Email = "manager-2@gmail.com",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    Role = RoleType.MANAGER,
                    IsActive = true


                }

                );
        }
    }
}
