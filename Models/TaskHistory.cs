using System;

namespace WpfApp1.Models
{
    public class TaskHistory
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public string Action { get; set; } = "";
        public int ActorUserId { get; set; }
        public DateTime AtUtc { get; set; }
        public string? MetadataJson { get; set; }
    }
}
