using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Only seed if no users exist
            if (await db.Users.AnyAsync()) 
                return;

            // Create users one by one to avoid relationship issues
            var superUser = new User
            { 
                Username = "super", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"), 
                Role = Role.SuperUser 
            };
            db.Users.Add(superUser);
            await db.SaveChangesAsync();

            var manager = new User 
            { 
                Username = "manager", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), 
                Role = Role.Manager
            };
            db.Users.Add(manager);
            await db.SaveChangesAsync();

            var alice = new User 
            { 
                Username = "alice", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("alice123"), 
                Role = Role.Employee,
                ManagerId = manager.Id
            };
            db.Users.Add(alice);
            await db.SaveChangesAsync();

            var bob = new User 
            { 
                Username = "bob", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("bob123"), 
                Role = Role.Employee,
                ManagerId = manager.Id
            };
            db.Users.Add(bob);
            await db.SaveChangesAsync();

            // Add some sample tasks
            var tasks = new[]
            {
                new TaskItem
                {
                    Title = "Review quarterly reports",
                    Description = "Analyze Q3 performance metrics and prepare summary",
                    Priority = Priority.High,
                    Status = Models.TaskStatus.InProgress,
                    AssignedUserId = alice.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    DueAtUtc = DateTime.UtcNow.AddDays(3)
                },
                new TaskItem
                {
                    Title = "Update website content",
                    Description = "Refresh homepage with new product information",
                    Priority = Priority.Medium,
                    Status = Models.TaskStatus.New,
                    AssignedUserId = bob.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    DueAtUtc = DateTime.UtcNow.AddDays(7)
                },
                new TaskItem
                {
                    Title = "System maintenance",
                    Description = "Perform routine server maintenance and updates",
                    Priority = Priority.High,
                    Status = Models.TaskStatus.Completed,
                    AssignedUserId = superUser.Id,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                    DueAtUtc = DateTime.UtcNow.AddDays(-1)
                }
            };

            db.Tasks.AddRange(tasks);
            await db.SaveChangesAsync();
        }
    }
}
