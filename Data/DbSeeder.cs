using LibraryManagementSystem.Models.Entities;

namespace LibraryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context, IConfiguration config)
        {
            context.Database.EnsureCreated();

            // Seed Admin
            if (!context.Users.Any(u => u.UserType == "Admin"))
            {
                var adminPassword = config["AppSettings:AdminDefaultPassword"] ?? "Admin@123";
                context.Users.Add(new User
                {
                    UserType = "Admin",
                    FullName = "System Admin",
                    Email = "admin@library.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
            }

            // Seed one default Librarian with unique LibrarianId
            if (!context.Users.Any(u => u.UserType == "Librarian"))
            {
                var prefix = config["AppSettings:LibrarianIdPrefix"] ?? "LIB";
                var librarianId = $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                context.Users.Add(new User
                {
                    UserType = "Librarian",
                    FullName = "Head Librarian",
                    Email = "librarian@library.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Librarian@123"),
                    LibrarianId = librarianId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();

                Console.WriteLine($"=== LIBRARIAN ID GENERATED: {librarianId} ===");
                Console.WriteLine("=== DEFAULT PASSWORD: Librarian@123 ===");
            }
        }
    }
}