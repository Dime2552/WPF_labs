// ConverterPage.xaml.cs
using System.Windows;
using System.Windows.Controls;

namespace lab6
{
    public partial class ConverterPage : Page
    {
        public ConverterPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Встановлюємо DataContext на SharedViewModel при завантаженні сторінки
            DataContext = MainWindow.SharedViewModel;
        }
    }
}