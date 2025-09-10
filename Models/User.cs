using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public Role Role { get; set; }
        public int? ManagerId { get; set; }
        public User? Manager { get; set; }
        public ICollection<User> Employees { get; set; } = new List<User>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
