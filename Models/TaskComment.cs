using System;

namespace WpfApp1.Models
{
    public class TaskComment
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public int AuthorUserId { get; set; }
        public User? Author { get; set; }
        public string Text { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }
}
