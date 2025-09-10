using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services.Interfaces;

namespace WpfApp1.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginViewModel> _logger;

        public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [ObservableProperty] private string username = "";
        [ObservableProperty] private string password = "";
        [ObservableProperty] private string? errorMessage;

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = null;
                
                var user = await _authService.AuthenticateAsync(Username, Password);
                if (user == null)
                {
                    ErrorMessage = "Invalid credentials";
                    return;
                }

                // Navigate to dashboard using MainViewModel
                if (Application.Current.MainWindow?.DataContext is MainViewModel main)
                {
                    main.CurrentUser = user;
                    main.NavigateToDashboard(user);
                }
                else
                {
                    ErrorMessage = "Main window not found";
                }

                _logger.LogInformation("User {u} logged in", user.Username);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                _logger.LogError(ex, "Login error for {u}", Username);
            }
        }
    }
}
