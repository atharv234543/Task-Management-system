using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly User _currentUser;
        private readonly ITaskService _taskService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DashboardViewModel> _logger;

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<User> AvailableUsers { get; } = new ObservableCollection<User>();

        [ObservableProperty] private TaskItem? selectedTask;
        [ObservableProperty] private Priority? filterPriority;
        [ObservableProperty] private Models.TaskStatus? filterStatus;
        [ObservableProperty] private int? filterAssignedUserId;
        [ObservableProperty] private DateTime? filterDueBefore;
        [ObservableProperty] private string? sortBy = "Created";
        [ObservableProperty] private bool sortDescending = false;
        [ObservableProperty] private string? newCommentText;
        [ObservableProperty] private bool isLoading;

        public User CurrentUser => _currentUser;

        public DashboardViewModel(User currentUser, ITaskService taskService, INotificationService notificationService, ILogger<DashboardViewModel> logger)
        {
            _currentUser = currentUser;
            _taskService = taskService;
            _notificationService = notificationService;
            _logger = logger;

            // Subscribe to notification events
            _notificationService.TaskDueSoon += OnTaskDueSoon;
            _notificationService.TaskCompleted += OnTaskCompleted;
            _notificationService.Start();

            // Load initial data
            _ = LoadTasksAsync();
            _ = LoadAvailableUsersAsync();
        }

        private void OnTaskDueSoon(object? sender, TaskItem task)
        {
            // Check if current user should be notified about this task
            if (_currentUser.Role == Role.SuperUser || 
                task.AssignedUserId == _currentUser.Id || 
                (_currentUser.Role == Role.Manager && task.AssignedUser.ManagerId == _currentUser.Id))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Task '{task.Title}' is due soon!", "Due Soon", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void OnTaskCompleted(object? sender, TaskItem task)
        {
            if (_currentUser.Role == Role.SuperUser || 
                task.AssignedUserId == _currentUser.Id || 
                (_currentUser.Role == Role.Manager && task.AssignedUser.ManagerId == _currentUser.Id))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Task '{task.Title}' has been completed!", "Task Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        [RelayCommand]
        public async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                Tasks.Clear();
                
                // Debug: Log filter values
                _logger.LogInformation("Applying filters - Priority: {Priority}, Status: {Status}, AssignedUserId: {AssignedUserId}", 
                    FilterPriority, FilterStatus, FilterAssignedUserId);
                
                var filter = new TaskFilter
                {
                    AssignedUserId = FilterAssignedUserId,
                    Priority = FilterPriority,
                    Status = FilterStatus,
                    DueBeforeUtc = FilterDueBefore?.ToUniversalTime(),
                    SortBy = SortBy,
                    Desc = SortDescending
                };

                var tasks = await _taskService.QueryAsync(_currentUser, filter);
                
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }

                _logger.LogInformation("Loaded {Count} tasks for user {Username}", tasks.Count, _currentUser.Username);
                
                // Show a message if no tasks found
                if (tasks.Count == 0)
                {
                    System.Windows.MessageBox.Show("No tasks found matching the current filters.", "No Results", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tasks");
                System.Windows.MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadAvailableUsersAsync()
        {
            try
            {
                AvailableUsers.Clear();
                
                // Load users based on current user's role
                var users = await _taskService.GetAvailableUsersAsync(_currentUser);
                
                foreach (var user in users)
                {
                    AvailableUsers.Add(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load available users");
            }
        }

        [RelayCommand]
        public async Task CreateTaskAsync()
        {
            try
            {
                // Check if manager has employees assigned
                if (_currentUser.Role == Role.Manager && !AvailableUsers.Any(u => u.ManagerId == _currentUser.Id))
                {
                    MessageBox.Show("You cannot create tasks because you have no employees assigned to you.", 
                                  "No Employees", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newTask = new TaskItem
                {
                    Title = "New Task",
                    Description = "",
                    Priority = Priority.Medium,
                    Status = Models.TaskStatus.New,
                    CreatedAtUtc = DateTime.UtcNow,
                    DueAtUtc = DateTime.UtcNow.AddDays(7),
                    AssignedUserId = _currentUser.Role == Role.Employee ? _currentUser.Id : AvailableUsers.FirstOrDefault()?.Id ?? _currentUser.Id
                };

                var created = await _taskService.CreateAsync(_currentUser, newTask);
                Tasks.Add(created);
                SelectedTask = created;

                _logger.LogInformation("Created new task '{Title}' by user {Username}", created.Title, _currentUser.Username);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't have permission to create this task.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create task");
                MessageBox.Show("Failed to create task. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task SaveTaskAsync()
        {
            if (SelectedTask == null) return;

            try
            {
                var updated = await _taskService.UpdateAsync(_currentUser, SelectedTask);
                if (updated != null)
                {
                    // Refresh the task in the list
                    var index = Tasks.IndexOf(SelectedTask);
                    if (index >= 0)
                    {
                        Tasks[index] = updated;
                        SelectedTask = updated;
                    }
                    
                    MessageBox.Show("Task saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _logger.LogInformation("Updated task '{Title}' by user {Username}", updated.Title, _currentUser.Username);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't have permission to edit this task.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save task");
                MessageBox.Show("Failed to save task. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteTaskAsync()
        {
            if (SelectedTask == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the task '{SelectedTask.Title}'?", 
                                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _taskService.DeleteAsync(_currentUser, SelectedTask.Id);
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
                
                MessageBox.Show("Task deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _logger.LogInformation("Deleted task by user {Username}", _currentUser.Username);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't have permission to delete this task.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete task");
                MessageBox.Show("Failed to delete task. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task AddCommentAsync(string text)
        {
            if (SelectedTask == null || string.IsNullOrWhiteSpace(text)) return;

            try
            {
                var comment = await _taskService.AddCommentAsync(_currentUser, SelectedTask.Id, text);
                
                // Instead of reloading all tasks, just refresh the selected task's comments
                var updatedTask = await _taskService.GetByIdAsync(SelectedTask.Id);
                if (updatedTask != null)
                {
                    // Update the selected task with fresh data
                    var index = Tasks.IndexOf(SelectedTask);
                    if (index >= 0)
                    {
                        Tasks[index] = updatedTask;
                    }
                    SelectedTask = updatedTask;
                }
                
                _logger.LogInformation("Added comment to task '{Title}' by user {Username}", SelectedTask.Title, _currentUser.Username);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't have permission to add comments to this task.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add comment");
                MessageBox.Show("Failed to add comment. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void Logout()
        {
            try
            {
                _notificationService.Stop();
                _logger.LogInformation("User {Username} logged out", _currentUser.Username);
                
                // Navigate back to login
                if (Application.Current.MainWindow?.DataContext is MainViewModel main)
                {
                    main.CurrentUser = null;
                    main.NavigateToLogin();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        // Filter commands
        [RelayCommand]
        public void ClearFilters()
        {
            FilterPriority = null;
            FilterStatus = null;
            FilterAssignedUserId = null;
            FilterDueBefore = null;
            SortBy = "Created";
            SortDescending = false;
        }

        [RelayCommand]
        public async Task ApplyFiltersAsync()
        {
            await LoadTasksAsync();
        }

        // Dispose pattern for cleanup
        public void Dispose()
        {
            _notificationService.Stop();
        }
    }
}
