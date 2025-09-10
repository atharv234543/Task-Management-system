# Task Management System

A comprehensive WPF desktop application built with .NET 8, implementing a role-based task management system with MVVM architecture.

## Features

### 🔐 Login System
- **Username/Password Authentication**: Secure login with BCrypt password hashing
- **Role-Based Access Control**: Three user roles (Super User, Manager, Employee)
- **Automatic Dashboard Redirect**: Users are redirected to appropriate dashboards based on their role

### 👥 Role-Based Task Display
- **Super User**: Can view and manage all tasks for all users
- **Manager**: Can view and manage tasks for themselves and their assigned employees
- **Employee**: Can view and update only their own tasks

### 📋 Task CRUD Operations
- **Create**: Add new tasks with full details (title, description, priority, status, due date, assignee)
- **Read**: View tasks with comprehensive filtering and sorting
- **Update**: Modify existing tasks with permission validation
- **Delete**: Remove tasks with confirmation dialogs

### 🔍 Advanced Filtering & Sorting
- **LINQ-based Filtering**: Filter by priority, status, assigned user, or due date
- **Multiple Sort Options**: Sort by due date, priority, or creation date (ascending/descending)
- **Real-time Updates**: Filters apply immediately to the task list

### ⚡ Async Operations
- **Database Operations**: All database interactions are asynchronous
- **Non-blocking UI**: User interface remains responsive during data operations
- **Error Handling**: Comprehensive exception handling with user-friendly messages

### 🔔 Event Handling & Notifications
- **Task Due Notifications**: Automatic alerts for tasks due within 30 minutes
- **Completion Notifications**: Notifications when tasks are marked as completed
- **Role-based Notifications**: Users only receive notifications for relevant tasks

### 🛡️ Exception Handling
- **Graceful Error Recovery**: Comprehensive error handling throughout the application
- **User-friendly Messages**: Clear error messages for invalid inputs or failed operations
- **Logging Integration**: All errors are logged for debugging and monitoring

### 📝 Logging System
- **Serilog Integration**: Professional logging using Serilog NuGet package
- **Action Logging**: All user actions are logged (login, logout, CRUD operations)
- **Structured Logging**: Detailed logging with context information

### 🏗️ MVVM Architecture
- **Separation of Concerns**: Clean separation between UI, business logic, and data layers
- **Data Binding**: Full XAML data binding with ObservableCollection for real-time updates
- **Command Pattern**: RelayCommand implementation for button actions
- **Dependency Injection**: Service container for loose coupling

### 📊 Task History & Comments
- **Complete Audit Trail**: Full history of all task modifications
- **Collaborative Comments**: Add comments to tasks for team collaboration
- **Timestamp Tracking**: All actions and comments are timestamped

### 🔒 Permission Checks
- **Manager Validation**: Prevents managers from creating tasks if no employees are assigned
- **Employee Restrictions**: Employees can only interact with their own tasks
- **Role-based Validation**: All operations validate user permissions before execution

## Technology Stack

- **Framework**: .NET 8 WPF
- **Database**: SQLite with Entity Framework Core
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog
- **Password Hashing**: BCrypt.Net-Next
- **UI Framework**: WPF with XAML

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Installation
1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build and run the application

### Demo Users
The application comes with pre-configured demo users:

| Username | Password | Role |
|----------|----------|------|
| super | super123 | Super User |
| manager | manager123 | Manager |
| alice | alice123 | Employee |
| bob | bob123 | Employee |

## Project Structure

```
WpfApp1/
├── Data/                    # Database context and seeding
├── Models/                  # Entity models and enums
├── Services/                # Business logic services
│   ├── Interfaces/         # Service interfaces
├── ViewModels/             # MVVM view models
├── Views/                  # XAML user interfaces
├── Converters/             # Value converters
└── Logging/               # Serilog configuration
```

## Key Components

### Models
- **User**: User entity with role-based properties
- **TaskItem**: Task entity with full CRUD support
- **TaskComment**: Comment system for collaboration
- **TaskHistory**: Complete audit trail
- **Enums**: Role, Priority, TaskStatus definitions

### Services
- **AuthService**: Authentication and user management
- **TaskService**: Task CRUD operations with permissions
- **PermissionService**: Role-based permission validation
- **NotificationService**: Event handling and notifications

### ViewModels
- **LoginViewModel**: Login functionality with error handling
- **DashboardViewModel**: Main task management interface
- **MainViewModel**: Navigation and application state

## Database Schema

The application uses SQLite with Entity Framework Core for data persistence. The database is automatically created on first run with sample data.

## SOLID Principles Implementation

- **Single Responsibility**: Each class has a single, well-defined purpose
- **Open/Closed**: Services are extensible through interfaces
- **Liskov Substitution**: Interface implementations are interchangeable
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Dependencies injected through constructor

## Security Features

- **Password Hashing**: BCrypt for secure password storage
- **Role-based Access**: Granular permission system
- **Input Validation**: Comprehensive validation on all inputs
- **SQL Injection Prevention**: Entity Framework parameterized queries

## Performance Considerations

- **Async/Await**: Non-blocking database operations
- **Lazy Loading**: Efficient data loading strategies
- **Caching**: Service-level caching where appropriate
- **UI Responsiveness**: Background operations don't block UI

## Future Enhancements

- User registration functionality
- Email notifications
- File attachments for tasks
- Advanced reporting and analytics
- Mobile companion app
- API for external integrations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
