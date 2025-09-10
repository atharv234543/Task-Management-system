using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services.Interfaces
{
	public interface IPermissionService
	{
		bool CanSeeTask(User current, TaskItem task);
		bool CanModifyTask(User current, TaskItem task);
		Task<bool> CanCreateTaskAsync(User current, int? assignedUserId);
	}
}
