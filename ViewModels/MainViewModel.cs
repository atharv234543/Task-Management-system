using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty] private User? _currentUser;
        [ObservableProperty] private object? _currentViewModel;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            // Don't navigate immediately - let the UI handle it
        }

        public void NavigateTo(object vm) => CurrentViewModel = vm;

        public void NavigateToLogin()
        {
            var loginVm = _serviceProvider.GetRequiredService<LoginViewModel>();
            var loginView = new Views.LoginView { DataContext = loginVm };
            CurrentViewModel = loginView;
        }

        public void NavigateToDashboard(User user)
        {
            var dashboardVm = new DashboardViewModel(
                user,
                _serviceProvider.GetRequiredService<ITaskService>(),
                _serviceProvider.GetRequiredService<INotificationService>(),
                _serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DashboardViewModel>>()
            );
            var dashboardView = new Views.DashboardView { DataContext = dashboardVm };
            CurrentViewModel = dashboardView;
        }
    }
}
