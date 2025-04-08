using System;
using System.Globalization;
using System.Windows;

namespace lab4 // Make sure namespace matches your project structure
{
    public partial class AddProductWindow : Window
    {
        // Properties to hold the entered data
        public int Article { get; private set; }
        public string ProductName { get; private set; } // Renamed to avoid conflict with Window.Name
        public string Unit { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }


        public AddProductWindow()
        {
            InitializeComponent();
            ArticleTextBox.Focus(); // Set focus to the first field
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Validation ---
            if (!int.TryParse(ArticleTextBox.Text, out int article) || article <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректний позитивний Артикул.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                ArticleTextBox.Focus();
                return;
            }

            // Name and Unit can be empty or null based on DB schema, add checks if required not null
            string name = NameTextBox.Text;
            string unit = UnitTextBox.Text;


            if (!int.TryParse(QuantityTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Кількість.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            // Use CultureInfo.InvariantCulture for decimal point consistency, or CurrentCulture if comma is expected
            if (!decimal.TryParse(PriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Ціну (використовуйте '.' як десятковий роздільник).", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            // --- If validation passes, store values in properties ---
            this.Article = article;
            this.ProductName = name;
            this.Unit = unit;
            this.Quantity = quantity;
            this.Price = price;


            // Set DialogResult to true and close the window
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}