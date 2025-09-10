using System.Windows.Controls;
using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private async void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel viewModel && !string.IsNullOrWhiteSpace(NewCommentTextBox.Text))
            {
                await viewModel.AddCommentAsync(NewCommentTextBox.Text);
                NewCommentTextBox.Clear();
            }
        }
    }
}


