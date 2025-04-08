using System;
using System.Data;
using System.Globalization; // For decimal parsing/formatting
using System.Windows;
using System.Windows.Controls;
using lab4.Data;

namespace lab4 // Make sure namespace matches your project structure
{
    public partial class MainWindow : Window
    {
        private AdoAssistant adoAssistant; // Renamed for clarity
        private DataTable productsTable; // To hold the data

        public MainWindow()
        {
            InitializeComponent();
            adoAssistant = new AdoAssistant();
            // Load data when the window is initialized
            LoadData();
        }

        private void LoadData()
        {
            // Завантажуємо або перезавантажуємо дані з бази
            productsTable = adoAssistant.LoadProducts(); // Use the correct method

            if (productsTable != null && productsTable.Rows.Count > 0)
            {
                // Оновлюємо ItemsSource для ListBox
                listProducts.ItemsSource = productsTable.DefaultView; // Bind to the correct ListBox
                // listProducts.SelectedIndex = 0; // Optionally select the first item
            }
            else
            {
                listProducts.ItemsSource = null; // Clear listbox if no data
                // MessageBox.Show("Дані товарів не завантажені або таблиця порожня.");
            }
        }

        // --- Button Click Handlers ---

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            bool? dialogResult = addProductWindow.ShowDialog();

            if (dialogResult == true)
            {
                // Data was entered and OK was clicked in the AddProductWindow
                try
                {
                    // Retrieve data from the AddProductWindow properties or controls
                    int article = addProductWindow.Article;
                    string name = addProductWindow.ProductName; // Use properties
                    string unit = addProductWindow.Unit;
                    int quantity = addProductWindow.Quantity;
                    decimal price = addProductWindow.Price;

                    // Add the new product using AdoAssistant
                    if (adoAssistant.AddProduct(article, name, unit, quantity, price))
                    {
                        MessageBox.Show("Товар успішно додано!");
                        // Refresh data in the ListBox
                        LoadData();
                        // Optionally, try to select the newly added item
                        SelectProductByArticle(article);
                    }
                    // Error messages are handled within AdoAssistant methods now
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при додаванні товару: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Перевірка, чи вибрано товар у списку
            if (listProducts.SelectedItem is DataRowView selectedRow) // Use 'is' pattern matching
            {
                // Отримуємо Артикул вибраного товару
                int productArticle = (int)selectedRow["Article"]; // Use the alias/column name

                // Confirm deletion
                MessageBoxResult confirm = MessageBox.Show($"Ви впевнені, що хочете видалити товар з артикулом {productArticle}?",
                                                            "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Запит на видалення товару з бази даних
                        if (adoAssistant.DeleteProduct(productArticle))
                        {
                            MessageBox.Show("Товар успішно видалено!");
                            // Перезавантажуємо дані після видалення
                            LoadData();
                        }
                        // Error messages handled in AdoAssistant
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при видаленні товару: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть товар для видалення.");
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Перевірка, чи вибрано товар для оновлення
            if (listProducts.SelectedItem is DataRowView selectedRow)
            {
                try
                {
                    // Отримуємо поточні дані вибраного товару
                    int productArticle = (int)selectedRow["Article"];
                    string name = selectedRow["Name"].ToString();
                    string unit = selectedRow["Unit"].ToString();
                    // Handle potential DBNull for nullable types if necessary before casting
                    int quantity = selectedRow["Quantity"] != DBNull.Value ? (int)selectedRow["Quantity"] : 0; // Example handling
                    decimal price = selectedRow["Price"] != DBNull.Value ? (decimal)selectedRow["Price"] : 0.0m; // Example handling


                    // Відкриваємо вікно для оновлення даних товару
                    UpdateProductWindow updateProductWindow = new UpdateProductWindow(productArticle, name, unit, quantity, price);
                    bool? dialogResult = updateProductWindow.ShowDialog();

                    // Якщо користувач натиснув "OK"
                    if (dialogResult == true)
                    {
                        // Отримуємо оновлені дані з вікна
                        string updatedName = updateProductWindow.ProductName; // Use properties
                        string updatedUnit = updateProductWindow.Unit;
                        int updatedQuantity = updateProductWindow.Quantity;
                        decimal updatedPrice = updateProductWindow.Price;

                        // Оновлюємо дані товару в базі
                        if (adoAssistant.UpdateProduct(productArticle, updatedName, updatedUnit, updatedQuantity, updatedPrice))
                        {
                            MessageBox.Show("Дані товару успішно оновлено!");
                            // Перезавантажуємо дані після оновлення
                            LoadData();
                            // Re-select the updated item
                            SelectProductByArticle(productArticle);
                        }
                        // Errors handled in AdoAssistant
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при підготовці до оновлення товару: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть товар для оновлення.");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Force reload data from database
            productsTable = null; // Clear the cached table
            adoAssistant.ClearCache();
            LoadData();
        }

        // Helper method to select an item in the listbox by Article
        private void SelectProductByArticle(int article)
        {
            if (productsTable == null)
                return;

            for (int i = 0; i < productsTable.DefaultView.Count; i++)
            {
                if ((int)productsTable.DefaultView[i]["Article"] == article)
                {
                    listProducts.SelectedIndex = i;
                    listProducts.ScrollIntoView(listProducts.SelectedItem);
                    break;
                }
            }
        }

        /* Optional: If you want details to update immediately on selection change
        private void ListProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The DataContext binding handles this automatically now
            // You could add specific logic here if needed when selection changes
        }
        */
    }
}