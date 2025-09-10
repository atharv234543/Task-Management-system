using CommunityToolkit.Mvvm.ComponentModel;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public partial class SimpleDashboardViewModel : ObservableObject
    {
        [ObservableProperty] private User _currentUser;

        public SimpleDashboardViewModel(User user)
        {
            CurrentUser = user;
        }
    }
}
