using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Logging;
using WpfApp1.Services;
using WpfApp1.Services.Interfaces;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            SerilogConfig.Configure();

            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Configure SQLite database
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        var baseDirectory = AppContext.BaseDirectory;
                        var databasePath = Path.Combine(baseDirectory, "taskmanager.db");
                        options.UseSqlite($"Data Source={databasePath}");
                    });
                    
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IPermissionService, PermissionService>();
                    services.AddScoped<ITaskService, TaskService>();
                    services.AddSingleton<INotificationService, NotificationService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddSingleton<MainViewModel>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Create and show the main window
                var mainWindow = new MainWindow();
                var mainViewModel = AppHost!.Services.GetRequiredService<MainViewModel>();
                mainWindow.DataContext = mainViewModel;
                
                mainWindow.Show();
                MainWindow = mainWindow;
                
                // Initialize database and navigate to login asynchronously
                _ = InitializeAsync(mainViewModel);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Application startup failed: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                System.Environment.Exit(1);
            }
        }

        private async Task InitializeAsync(MainViewModel mainViewModel)
        {
            try
            {
                using var scope = AppHost!.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Create database
                await db.Database.EnsureCreatedAsync();
                
                // Seed initial data
                await DbSeeder.SeedAsync(db);
                
                mainViewModel.NavigateToLogin();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
