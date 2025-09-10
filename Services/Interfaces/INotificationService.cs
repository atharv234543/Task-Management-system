using System;
using WpfApp1.Models;

namespace WpfApp1.Services.Interfaces
{
    public interface INotificationService
    {
        event EventHandler<TaskItem>? TaskDueSoon;
        event EventHandler<TaskItem>? TaskCompleted;
        void Start();
        void Stop();
    }
}
