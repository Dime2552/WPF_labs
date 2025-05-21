using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace lab6
{
    public partial class MainWindow : Window
    {
        public static MainViewModel SharedViewModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            if (SharedViewModel == null)
            {
                SharedViewModel = new MainViewModel();
            }

            MainFrame.Navigated += MainFrame_Navigated;

            MainFrame.Navigate(new Uri("MainNavigationPage.xaml", UriKind.Relative));
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}