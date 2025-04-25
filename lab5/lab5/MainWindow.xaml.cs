using lab5.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace lab5
{
    public partial class MainWindow : Window
    {
        private Lab5DBEntities db = new Lab5DBEntities();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            // --- Вкладка 1: Товари ---
            ProductsDataGrid.ItemsSource = db.Товари
                .Select(t => new
                {
                    t.Артикул,
                    Одиниця = t.Одиниці_виміру != null ? t.Одиниці_виміру.Одиниця_виміру : "(не вказано)",
                    t.Кількість,
                    t.Ціна
                })
                .ToList();

            // --- Вкладка 2: Одиниці виміру ---
            var units = db.Одиниці_виміру
                .Select(u => new
                {
                    u.Код_одиниці_виміру,
                    u.Одиниця_виміру
                })
                .ToList();
            UnitsDataGrid.ItemsSource = units; // Використовуємо завантажені дані

            // --- Вкладка 3: Загальна кількість по одиницях виміру ---
            var quantityByUnit = db.Товари
               .Where(t => t.Код_одиниці_виміру != null)
               .GroupBy(t => t.Код_одиниці_виміру)
               .Select(g => new
               {
                   ОдиницяВиміру = db.Одиниці_виміру
                                     .Where(u => u.Код_одиниці_виміру == g.Key)
                                     .Select(u => u.Одиниця_виміру)
                                     .FirstOrDefault() ?? "(невідома одиниця)",
                   // Перевірка на null, якщо Кількість може бути Nullable
                   ЗагальнаКількість = g.Sum(t => t.Кількість ?? 0)
               })
               .ToList();
            QuantityByUnitDataGrid.ItemsSource = quantityByUnit;


            // --- Заповнення ComboBox для Вкладки 5 ---
            // Використовуємо дані, завантажені для Вкладки 2
            UnitComboBox.ItemsSource = units;
            UnitComboBox.DisplayMemberPath = "Одиниця_виміру"; // Що показувати користувачу
            UnitComboBox.SelectedValuePath = "Код_одиниці_виміру"; // Яке значення використовувати програмно

            // Опціонально: Встановити початкове значення (наприклад, перший елемент)
            if (units.Any()) // Перевірка, чи список не порожній
            {
                UnitComboBox.SelectedItem = null;
                ProductsByUnitDataGrid.ItemsSource = new List<object>(); // Очистити таблицю результатів
            }
        }

        // --- Вкладка 4: Обробник кнопки "Пошук товару" за артикулом ---
        private void SearchProductButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = ProductSkuTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(searchText) && int.TryParse(searchText, out int searchSku))
            {
                FilteredProductsDataGrid.ItemsSource = db.Товари
                    .Where(t => t.Артикул == searchSku)
                    .Select(t => new
                    {
                        t.Артикул,
                        Одиниця = t.Одиниці_виміру != null ? t.Одиниці_виміру.Одиниця_виміру : "(не вказано)",
                        t.Кількість,
                        t.Ціна
                    })
                    .ToList();
            }
            else
            {
                FilteredProductsDataGrid.ItemsSource = new List<object>();
                if (!string.IsNullOrEmpty(searchText)) // Якщо щось введено, але це не число
                {
                    MessageBox.Show("Будь ласка, введіть дійсний числовий артикул.", "Помилка пошуку", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // --- Вкладка 5: Пошук товару за одиницею виміру ---
        private void SearchByUnitButton_Click(object sender, RoutedEventArgs e)
        {
            // Перевіряємо, чи вибрано значення в ComboBox
            if (UnitComboBox.SelectedValue != null)
            {
                // Отримуємо вибраний Код_одиниці_виміру
                // Тип SelectedValue відповідає типу SelectedValuePath (в нашому випадку Код_одиниці_виміру)
                int selectedUnitId = (int)UnitComboBox.SelectedValue;

                // Фільтруємо товари за вибраним Код_одиниці_виміру
                ProductsByUnitDataGrid.ItemsSource = db.Товари
                    .Where(t => t.Код_одиниці_виміру == selectedUnitId)
                    .Select(t => new
                    {
                        t.Артикул,
                        Одиниця = t.Одиниці_виміру != null ? t.Одиниці_виміру.Одиниця_виміру : "(не вказано)",
                        t.Кількість,
                        t.Ціна
                    })
                    .ToList();
            }
            else
            {
                // Якщо нічого не вибрано, очищуємо таблицю і показуємо повідомлення
                ProductsByUnitDataGrid.ItemsSource = new List<object>();
                MessageBox.Show("Будь ласка, оберіть одиницю виміру зі списку.", "Пошук неможливий", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        protected override void OnClosed(System.EventArgs e)
        {
            db?.Dispose();
            base.OnClosed(e);
        }
    }
}