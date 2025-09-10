using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly IPermissionService _permission;
        private readonly ILogger<TaskService> _logger;

        public TaskService(AppDbContext db, IPermissionService permission, ILogger<TaskService> logger)
        {
            _db = db;
            _permission = permission;
            _logger = logger;
        }

        public async Task<TaskItem> CreateAsync(User actor, TaskItem toCreate)
        {
            if (!await _permission.CanCreateTaskAsync(actor, toCreate.AssignedUserId))
                throw new UnauthorizedAccessException("Not allowed to create this task.");

            toCreate.CreatedAtUtc = DateTime.UtcNow;
            _db.Tasks.Add(toCreate);
            
            // Add history entry
            await AddHistoryAsync(toCreate.Id, actor.Id, "Created", null);
            
            // Save both task and history in one transaction
            await _db.SaveChangesAsync();
            
            _logger.LogInformation("Task {task} created by {user}", toCreate.Title, actor.Username);
            return toCreate;
        }

        public async Task<TaskItem?> UpdateAsync(User actor, TaskItem toUpdate)
        {
            var existing = await _db.Tasks.Include(t => t.AssignedUser).FirstOrDefaultAsync(t => t.Id == toUpdate.Id);
            if (existing == null) return null;
            if (!_permission.CanModifyTask(actor, existing)) throw new UnauthorizedAccessException();

            var oldStatus = existing.Status;
            existing.Title = toUpdate.Title;
            existing.Description = toUpdate.Description;
            existing.Priority = toUpdate.Priority;
            existing.Status = toUpdate.Status;
            existing.DueAtUtc = toUpdate.DueAtUtc;
            existing.AssignedUserId = toUpdate.AssignedUserId;

            // Add history entry if status changed
            if (oldStatus != existing.Status)
                await AddHistoryAsync(existing.Id, actor.Id, $"Status {oldStatus} -> {existing.Status}", null);

            // Save both task and history in one transaction
            await _db.SaveChangesAsync();

            _logger.LogInformation("Task {id} updated by {user}", existing.Id, actor.Username);
            return existing;
        }

        public async Task DeleteAsync(User actor, int taskId)
        {
            var t = await _db.Tasks.Include(x => x.AssignedUser).FirstOrDefaultAsync(x => x.Id == taskId);
            if (t == null) return;
            if (!_permission.CanModifyTask(actor, t)) throw new UnauthorizedAccessException();

            _db.Tasks.Remove(t);
            
            // Add history entry
            await AddHistoryAsync(taskId, actor.Id, "Deleted", null);
            
            // Save both deletion and history in one transaction
            await _db.SaveChangesAsync();
            
            _logger.LogInformation("Task {id} deleted by {user}", taskId, actor.Username);
        }

        public async Task<TaskItem?> GetByIdAsync(int taskId)
        {
            return await _db.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .Include(t => t.History)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<IList<TaskItem>> QueryAsync(User current, TaskFilter filter)
        {
            var q = _db.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .Include(t => t.History)
                .AsQueryable();

            q = current.Role switch
            {
                Role.SuperUser => q,
                Role.Manager => q.Where(t => t.AssignedUser.ManagerId == current.Id || t.AssignedUserId == current.Id),
                _ => q.Where(t => t.AssignedUserId == current.Id)
            };

            if (filter.AssignedUserId != null) q = q.Where(t => t.AssignedUserId == filter.AssignedUserId);
            if (filter.Priority != null) q = q.Where(t => t.Priority == filter.Priority);
            if (filter.Status != null) q = q.Where(t => t.Status == filter.Status);
            if (filter.DueBeforeUtc != null) q = q.Where(t => t.DueAtUtc != null && t.DueAtUtc <= filter.DueBeforeUtc);

            q = filter.SortBy switch
            {
                "Due" => filter.Desc ? q.OrderByDescending(t => t.DueAtUtc) : q.OrderBy(t => t.DueAtUtc),
                "Priority" => filter.Desc ? q.OrderByDescending(t => t.Priority) : q.OrderBy(t => t.Priority),
                _ => filter.Desc ? q.OrderByDescending(t => t.CreatedAtUtc) : q.OrderBy(t => t.CreatedAtUtc)
            };

            return await q.ToListAsync();
        }

        public async Task<TaskComment> AddCommentAsync(User actor, int taskId, string text)
        {
            // Debug: Check if Comments table exists
            var commentCount = await _db.Comments.CountAsync();
            _logger.LogInformation("Current comment count in database: {Count}", commentCount);
            
            var task = await _db.Tasks.Include(t => t.AssignedUser).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) throw new InvalidOperationException("Task not found");
            if (!_permission.CanModifyTask(actor, task)) throw new UnauthorizedAccessException();

            var c = new TaskComment 
            { 
                TaskItemId = taskId, 
                AuthorUserId = actor.Id, 
                Text = text, 
                CreatedAtUtc = DateTime.UtcNow
            };
            
            _db.Comments.Add(c);
            _logger.LogInformation("About to save comment: TaskId={TaskId}, AuthorUserId={AuthorUserId}, Text='{Text}'", 
                c.TaskItemId, c.AuthorUserId, c.Text);
            
            // Add history entry
            await AddHistoryAsync(taskId, actor.Id, "CommentAdded", text);
            
            // Save both comment and history in one transaction
            var changes = await _db.SaveChangesAsync();
            _logger.LogInformation("SaveChanges returned {Changes} changes", changes);
            
            _logger.LogInformation("User {user} commented on task {taskId}, comment ID: {commentId}", actor.Username, taskId, c.Id);
            return c;
        }

        public async Task<IList<User>> GetAvailableUsersAsync(User current)
        {
            return current.Role switch
            {
                Role.SuperUser => await _db.Users.ToListAsync(),
                Role.Manager => await _db.Users.Where(u => u.ManagerId == current.Id || u.Id == current.Id).ToListAsync(),
                Role.Employee => await _db.Users.Where(u => u.Id == current.Id).ToListAsync(),
                _ => new List<User>()
            };
        }

        public async Task<IList<TaskHistory>> GetTaskHistoryAsync(int taskId)
        {
            return await _db.History
                .Where(h => h.TaskItemId == taskId)
                .OrderByDescending(h => h.AtUtc)
                .ToListAsync();
        }

        private async Task AddHistoryAsync(int taskId, int actorId, string action, string? metadata)
        {
            var h = new TaskHistory { TaskItemId = taskId, ActorUserId = actorId, Action = action, AtUtc = DateTime.UtcNow, MetadataJson = metadata };
            _db.History.Add(h);
            // Don't call SaveChangesAsync here - let the calling method handle it
        }
    }
}
