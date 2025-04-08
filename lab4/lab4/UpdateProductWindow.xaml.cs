using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace lab4 // Make sure namespace matches your project structure
{
    public partial class UpdateProductWindow : Window
    {
        // Properties to hold the updated data
        public int Article { get; private set; } // Keep track of the original article
        public string ProductName { get; private set; }
        public string Unit { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        // Constructor to receive current data
        public UpdateProductWindow(int currentArticle, string currentName, string currentUnit, int currentQuantity, decimal currentPrice)
        {
            InitializeComponent();

            // Store the article being updated
            this.Article = currentArticle;

            // Populate fields with current data
            ArticleTextBox.Text = currentArticle.ToString();
            NameTextBox.Text = currentName;
            UnitTextBox.Text = currentUnit;
            QuantityTextBox.Text = currentQuantity.ToString();
            // Use InvariantCulture for consistent decimal point formatting
            PriceTextBox.Text = currentPrice.ToString("F2", CultureInfo.InvariantCulture);

            NameTextBox.Focus(); // Set focus to the first editable field
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Validation --- (Similar to Add window, but Article is read-only)
            string name = NameTextBox.Text;
            string unit = UnitTextBox.Text; // Allow empty based on DB schema

            if (!int.TryParse(QuantityTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Кількість.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Ціну (використовуйте '.' як десятковий роздільник).", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            // --- If validation passes, store updated values in properties ---
            // Article remains the same (it's the key)
            this.ProductName = name;
            this.Unit = unit;
            this.Quantity = quantity;
            this.Price = price;

            // Set DialogResult to true and close
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