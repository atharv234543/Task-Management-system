using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public TaskStatus Status { get; set; } = TaskStatus.New;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DueAtUtc { get; set; }

        public int AssignedUserId { get; set; }
        public User AssignedUser { get; set; } = null!;

        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    }
}
