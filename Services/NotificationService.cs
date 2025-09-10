using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<NotificationService> _logger;
        private System.Timers.Timer? _timer;
        public event EventHandler<TaskItem>? TaskDueSoon;
        public event EventHandler<TaskItem>? TaskCompleted;

        public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Start()
        {
            _timer = new System.Timers.Timer(60000); // every minute
            _timer.Elapsed += async (_, __) => await ScanAsync();
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void Stop() => _timer?.Stop();

        private async Task ScanAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var dueWindow = now.AddMinutes(30);

                var dueTasks = await _db.Tasks.Where(t => t.DueAtUtc != null && t.DueAtUtc <= dueWindow && t.Status != Models.TaskStatus.Completed).ToListAsync();
                foreach (var t in dueTasks) TaskDueSoon?.Invoke(this, t);

                // Completed detection: look for recently created history entries that indicate status changes
                var recentCompletions = await _db.History
                    .Where(h => h.Action.Contains("Status") && h.AtUtc >= now.AddMinutes(-1))
                    .Select(h => h.TaskItemId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in recentCompletions)
                {
                    var task = await _db.Tasks.FindAsync(id);
                    if (task != null && task.Status == Models.TaskStatus.Completed) TaskCompleted?.Invoke(this, task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification scan failed");
            }
        }
    }
}
