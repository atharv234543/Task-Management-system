using System.Windows.Controls;
using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is LoginViewModel viewModel)
            {
                // Handle username text changes
                UsernameTextBox.TextChanged += (s, args) => viewModel.Username = UsernameTextBox.Text;
                
                // Handle password changes
                PasswordBox.PasswordChanged += (s, args) => viewModel.Password = PasswordBox.Password;
                
                // Handle login button click
                LoginButton.Click += (s, args) => viewModel.LoginCommand.Execute(null);
                
                // Update error message visibility
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(LoginViewModel.ErrorMessage))
                    {
                        ErrorMessageText.Text = viewModel.ErrorMessage ?? "";
                        ErrorMessageText.Visibility = string.IsNullOrEmpty(viewModel.ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
                    }
                };
            }
        }
    }
}


