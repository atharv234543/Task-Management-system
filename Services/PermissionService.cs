using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _db;

        public PermissionService(AppDbContext db) { _db = db; }

        public bool CanSeeTask(User current, TaskItem task) =>
            current.Role switch
            {
                Role.SuperUser => true,
                Role.Manager => task.AssignedUser.ManagerId == current.Id || task.AssignedUserId == current.Id,
                Role.Employee => task.AssignedUserId == current.Id,
                _ => false
            };

        public bool CanModifyTask(User current, TaskItem task)
        {
            if (current.Role == Role.SuperUser) return true;
            if (current.Role == Role.Manager)
                return task.AssignedUser.ManagerId == current.Id || task.AssignedUserId == current.Id;
            return current.Role == Role.Employee && task.AssignedUserId == current.Id;
        }

        public async Task<bool> CanCreateTaskAsync(User current, int? assignedUserId)
        {
            if (current.Role == Role.SuperUser) return true;
            if (current.Role == Role.Manager)
            {
                var employees = await _db.Users.Where(u => u.ManagerId == current.Id).ToListAsync();
                return employees.Any() && assignedUserId != null && employees.Any(e => e.Id == assignedUserId.Value);
            }
            if (current.Role == Role.Employee)
                return false; // allow only creating their own tasks; adjust if you want disallow
            return false;
        }
    }
}
