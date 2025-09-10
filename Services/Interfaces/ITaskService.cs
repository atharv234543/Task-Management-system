using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services.Interfaces
{
    public class TaskFilter
    {
        public int? AssignedUserId { get; set; }
        public Priority? Priority { get; set; }
        public Models.TaskStatus? Status { get; set; }
        public DateTime? DueBeforeUtc { get; set; }
        public string? SortBy { get; set; } // "Due"|"Priority"|"Created"
        public bool Desc { get; set; }
    }

    public interface ITaskService
    {
        Task<TaskItem> CreateAsync(User actor, TaskItem toCreate);
        Task<TaskItem?> UpdateAsync(User actor, TaskItem toUpdate);
        Task DeleteAsync(User actor, int taskId);
        Task<TaskItem?> GetByIdAsync(int taskId);
        Task<IList<TaskItem>> QueryAsync(User current, TaskFilter filter);
        Task<TaskComment> AddCommentAsync(User actor, int taskId, string text);
        Task<IList<User>> GetAvailableUsersAsync(User current);
        Task<IList<TaskHistory>> GetTaskHistoryAsync(int taskId);
    }
}
