using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;
using System;

namespace WpfApp1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskComment> Comments { get; set; }
        public DbSet<TaskHistory> History { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User entity
            builder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure TaskItem entity with cascade delete
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting users who have tasks

            // Configure TaskComment with cascade delete
            builder.Entity<TaskComment>()
                .HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade); // Delete comments when task is deleted

            builder.Entity<TaskComment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting users who have comments

            // Configure TaskHistory with cascade delete
            builder.Entity<TaskHistory>()
                .HasOne(h => h.TaskItem)
                .WithMany(t => t.History)
                .HasForeignKey(h => h.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade); // Delete history when task is deleted

            // Configure enum conversions to store as strings
            builder.Entity<User>()
                .Property(e => e.Role)
                .HasConversion(
                    v => v.ToString(),
                    v => (Role)Enum.Parse(typeof(Role), v));

            builder.Entity<TaskItem>()
                .Property(e => e.Priority)
                .HasConversion(
                    v => v.ToString(),
                    v => (Priority)Enum.Parse(typeof(Priority), v));

            builder.Entity<TaskItem>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (Models.TaskStatus)Enum.Parse(typeof(Models.TaskStatus), v));
        }
    }
}
