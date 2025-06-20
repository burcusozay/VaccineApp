using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VaccineApp.Data.Context;
using VaccineApp.Data.Entities;

namespace VaccineApp.DataSeed
{
    public static class VaccineDbSeed
    {
        private static bool _isProduction;
        public static void SeedDatabase(IApplicationBuilder app, bool isProduction)
        {
            _isProduction = isProduction;

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }
        private static void SeedData(AppDbContext dbContext)
        {
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            // aşağıdaki komutun yaptığı işi yapıyor
            // dotnet ef database update --project VaccineApp.Data/VaccineApp.Data.csproj --startup-project VaccineApp.WebAPI/VaccineApp.WebAPI.csproj 
            //if (_isProduction)
            //{
            //    dbContext.Database.Migrate();
            //}
            if (!dbContext.Users.Any())
            {
                Console.WriteLine("Db is now seeding...");
                //dbContext.Notifications.AddRange(
                //    new Notification() { Title = "Dotnet", Content = "Microsoft", Target = "Free" },
                //    new Notification() { Title = "SqlServer", Content = "Microsoft", Target = "Free" },
                //    new Notification() { Title = "Kubernates", Content = "", Target = "" },
                //    new Notification() { Title = "RabbitMq", Content = "CNCF", Target = "Free" });
                dbContext.Roles.AddRange(new List<Role>()
                 {
                    new Role { Id=new Guid("64eb6b4b-1c86-4f07-9e7d-e4977a313c4d"), Name = "admin", NormalizedName = "admin".ToUpperInvariant() , Description = "admin" ,IsDeleted = false},
                    new Role { Id=new Guid("dbc313af-5087-4d55-91f4-f006a5336e6b"), Name = "test",  NormalizedName = "test".ToUpperInvariant()  , Description = "test"  ,IsDeleted = false},
                    new Role { Id=new Guid("f9405faf-a651-4e53-b228-5c6c1ff19933"), Name = "user",  NormalizedName = "user".ToUpperInvariant()  , Description = "user"  ,IsDeleted = false}
                });

                List<User> userList = new List<User>();
                userList.Add(new User { Id = new Guid("65b4bdc3-8835-4778-a109-52f1dd8c683c"), Username = "burcu.sozay", NormalizedUserName = "BURCU.SOZAY".ToUpperInvariant(), Email = "burcu.sozay@example.com", Name = "Burcu", Surname = "SÖZAY", PhoneNumber = "5366643625", IsDeleted = false });
                userList.Add(new User { Id = new Guid("fbcbf7a2-44d4-4491-8e6e-3e95552168d2"), Username = "rabbitMqUser", NormalizedUserName = "RABBITMQUSER".ToUpperInvariant(), Email = "test@example.com", Name = "Rabbit MQ", Surname = "User", PhoneNumber = "5333665117", IsDeleted = false });
                userList.Add(new User { Id = new Guid("51701f7b-1a86-4894-a6a1-09e93f9d27dd"), Username = "alper.koklu", NormalizedUserName = "ALPER.KOKLU".ToUpperInvariant(), Email = "alper.koklu@example.com", Name = "Alper", Surname = "KÖKLÜ", PhoneNumber = "5555555555", IsDeleted = false });

                userList.ForEach(user => user.PasswordHash = passwordHasher.HashPassword(user, "1234qqqQ."));
                dbContext.Users.AddRange(userList);

                dbContext.UserRoles.AddRange(new List<UserRole>()
                {
                     new UserRole() {Id=new Guid("86dfc346-7330-4ca7-a823-49b7d644a29b"), UserId = new Guid("65b4bdc3-8835-4778-a109-52f1dd8c683c"), RoleId = new Guid("64eb6b4b-1c86-4f07-9e7d-e4977a313c4d") },
                     new UserRole() {Id=new Guid("f77a0050-6a5f-4337-ae74-0fcf0f9879cb"), UserId = new Guid("fbcbf7a2-44d4-4491-8e6e-3e95552168d2"), RoleId = new Guid("dbc313af-5087-4d55-91f4-f006a5336e6b") },
                     new UserRole() {Id=new Guid("14bcaf75-25ba-4f5c-8af7-f94aac1c9604"), UserId = new Guid("51701f7b-1a86-4894-a6a1-09e93f9d27dd"), RoleId = new Guid("f9405faf-a651-4e53-b228-5c6c1ff19933")  }
                });

                dbContext.Freezers.AddRange(new List<Freezer>()
                {
                     new Freezer() { Name = "Dolap 1", CreatedDate = DateTime.UtcNow  },
                     new Freezer() { Name = "Dolap 2", CreatedDate = DateTime.UtcNow  },
                     new Freezer() { Name = "Dolap 3", CreatedDate = DateTime.UtcNow  }
                });

                dbContext.SaveChanges();
            }
            else
            {
                Console.WriteLine("Db is not null");
            }
        }
    }
}
