using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace lab6
{
    public partial class MainNavigationPage : Page
    {
        public MainNavigationPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Кожна сторінка, завантажена у Frame, має доступ до NavigationService
            this.NavigationService?.Navigate(e.Uri);
            e.Handled = true;
        }
    }
}