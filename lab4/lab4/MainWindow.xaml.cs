using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using lab4.Data;

namespace lab4 // Assuming this namespace is correct
{
    public partial class MainWindow : Window
    {
        private AdoAssistant adoAssistant;
        private DataTable productsTable;

        // Enum to manage the state of the input panel
        private enum EditMode { Viewing, Adding, Editing }
        private EditMode currentMode = EditMode.Viewing;

        public MainWindow()
        {
            InitializeComponent();
            adoAssistant = new AdoAssistant();
            LoadData();
            UpdateUIMode(); // Set initial UI state
        }

        private void LoadData()
        {
            // Preserve selection if possible
            object selectedItem = listProducts.SelectedItem;
            int selectedArticle = -1;
            if (selectedItem is DataRowView rowView)
            {
                selectedArticle = (int)rowView["Article"];
            }

            productsTable = adoAssistant.LoadProducts(); // Use the correct method

            if (productsTable != null)
            {
                listProducts.ItemsSource = productsTable.DefaultView;
                if (selectedArticle != -1)
                {
                    SelectProductByArticle(selectedArticle); // Try to re-select
                }
                else if (listProducts.Items.Count > 0)
                {
                    listProducts.SelectedIndex = 0; // Select first if nothing was selected
                }
            }
            else
            {
                listProducts.ItemsSource = null;
            }
            // Ensure UI reflects the current selection or lack thereof
            ListProducts_SelectionChanged(null, null);
            UpdateUIMode(); // Re-apply UI state after loading
        }

        // --- State Management ---

        private void SetMode(EditMode newMode)
        {
            currentMode = newMode;
            UpdateUIMode();
        }

        private void UpdateUIMode()
        {
            switch (currentMode)
            {
                case EditMode.Viewing:
                    InputEditPanel.IsEnabled = false; // Disable the whole panel
                    SaveChangesPanel.Visibility = Visibility.Collapsed; // Hide Save/Cancel
                    listProducts.IsEnabled = true; // Enable the list
                    MainToolBar.IsEnabled = true; // Enable Toolbar
                    // Buttons that require selection might need disabling if nothing is selected
                    EditProductButton.IsEnabled = listProducts.SelectedItem != null;
                    DeleteProductButton.IsEnabled = listProducts.SelectedItem != null;
                    ArticleTextBox.IsReadOnly = true; // Keep Article RO even if panel enabled later
                    break;

                case EditMode.Adding:
                    InputEditPanel.IsEnabled = true; // Enable panel for input
                    SaveChangesPanel.Visibility = Visibility.Visible; // Show Save/Cancel
                    listProducts.IsEnabled = false; // Disable list during add
                    MainToolBar.IsEnabled = false; // Disable Toolbar during add
                    ArticleTextBox.IsReadOnly = false; // Article is editable when adding NEW
                    ClearInputFields();
                    ArticleTextBox.Focus();
                    break;

                case EditMode.Editing:
                    InputEditPanel.IsEnabled = true; // Enable panel for editing
                    SaveChangesPanel.Visibility = Visibility.Visible; // Show Save/Cancel
                    listProducts.IsEnabled = false; // Disable list during edit
                    MainToolBar.IsEnabled = false; // Disable Toolbar during edit
                    ArticleTextBox.IsReadOnly = true; // Article (PK) CANNOT be edited
                    // Data should already be populated by selection or Edit click
                    NameTextBox.Focus();
                    break;
            }
        }

        // --- Helper Methods ---

        private void ClearInputFields()
        {
            ArticleTextBox.Text = "";
            NameTextBox.Text = "";
            UnitTextBox.Text = "";
            QuantityTextBox.Text = "";
            PriceTextBox.Text = "";
        }

        private void PopulateInputFields(DataRowView rowView)
        {
            if (rowView == null)
            {
                ClearInputFields();
                return;
            }
            ArticleTextBox.Text = rowView["Article"].ToString();
            NameTextBox.Text = rowView["Name"]?.ToString() ?? ""; // Handle potential DBNull
            UnitTextBox.Text = rowView["Unit"]?.ToString() ?? "";   // Handle potential DBNull
            // Handle potential DBNull before converting to string
            QuantityTextBox.Text = rowView["Quantity"] != DBNull.Value ? rowView["Quantity"].ToString() : "";
            PriceTextBox.Text = rowView["Price"] != DBNull.Value
                ? ((decimal)rowView["Price"]).ToString("F2", CultureInfo.InvariantCulture)
                : "";
        }

        private bool ValidateInput(out int article, out string name, out string unit, out int quantity, out decimal price)
        {
            article = 0;
            name = "";
            unit = "";
            quantity = 0;
            price = 0;

            // Validate Article (only required if Adding)
            if (currentMode == EditMode.Adding)
            {
                if (!int.TryParse(ArticleTextBox.Text, out article) || article <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректний позитивний Артикул.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ArticleTextBox.Focus();
                    return false;
                }
            }
            else // If Editing, get article from the read-only textbox (or better, store it when edit starts)
            {
                if (!int.TryParse(ArticleTextBox.Text, out article)) // Should always parse if editing
                {
                    MessageBox.Show("Помилка: Не вдалося отримати Артикул для оновлення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }


            name = NameTextBox.Text; // Add validation if required (e.g., not empty)
            unit = UnitTextBox.Text; // Add validation if required

            if (!int.TryParse(QuantityTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) || quantity < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Кількість.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out price) || price < 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну невід'ємну Ціну (використовуйте '.' як десятковий роздільник).", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return false;
            }
            return true;
        }

        private void SelectProductByArticle(int article)
        {
            if (productsTable == null || listProducts == null)
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
            // Ensure the UI reflects the selection change
            ListProducts_SelectionChanged(null, null);
        }


        // --- Event Handlers ---

        private void ListProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentMode == EditMode.Viewing && listProducts.SelectedItem is DataRowView selectedRow)
            {
                PopulateInputFields(selectedRow);
                EditProductButton.IsEnabled = true; // Enable edit/delete now that item selected
                DeleteProductButton.IsEnabled = true;
            }
            else if (currentMode == EditMode.Viewing) // No selection or invalid selection
            {
                ClearInputFields();
                EditProductButton.IsEnabled = false; // Disable edit/delete
                DeleteProductButton.IsEnabled = false;
            }
            // Do nothing if Adding or Editing mode, selection is disabled anyway
        }


        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(EditMode.Adding);
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (listProducts.SelectedItem != null)
            {
                // Data should already be in fields due to selection change
                SetMode(EditMode.Editing);
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть товар для редагування.", "Редагування", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput(out int article, out string name, out string unit, out int quantity, out decimal price))
            {
                bool success = false;
                try
                {
                    if (currentMode == EditMode.Adding)
                    {
                        success = adoAssistant.AddProduct(article, name, unit, quantity, price);
                        if (success)
                            MessageBox.Show("Товар успішно додано!");
                    }
                    else if (currentMode == EditMode.Editing)
                    {
                        // 'article' comes from validation which reads the (now read-only) ArticleTextBox
                        success = adoAssistant.UpdateProduct(article, name, unit, quantity, price);
                        if (success)
                            MessageBox.Show("Товар успішно оновлено!");
                    }

                    if (success)
                    {
                        LoadData(); // Reload data to show changes
                        SelectProductByArticle(article); // Try to re-select the item
                        SetMode(EditMode.Viewing); // Return to viewing mode
                    }
                    // Error messages for DB operations are handled within AdoAssistant now
                }
                catch (Exception ex) // Catch unexpected errors during save process
                {
                    MessageBox.Show($"Сталася помилка при збереженні: {ex.Message}", "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // If validation fails, do nothing - messages are shown by ValidateInput
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(EditMode.Viewing);
            // Repopulate fields with current selection after canceling
            ListProducts_SelectionChanged(null, null);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode != EditMode.Viewing)
                return; // Should not happen if UI logic is correct

            if (listProducts.SelectedItem is DataRowView selectedRow)
            {
                int productArticle = (int)selectedRow["Article"];
                MessageBoxResult confirm = MessageBox.Show($"Ви впевнені, що хочете видалити товар з артикулом {productArticle}?",
                                                            "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (adoAssistant.DeleteProduct(productArticle))
                        {
                            MessageBox.Show("Товар успішно видалено!");
                            LoadData(); // Refresh list
                                        // Selection will be lost or changed, handled by LoadData/SelectionChanged
                            SetMode(EditMode.Viewing); // Ensure viewing mode
                        }
                        // DB errors handled in AdoAssistant
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при видаленні товару: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть товар для видалення.");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(EditMode.Viewing); // Ensure viewing mode before refresh
            adoAssistant.ClearCache(); // Clear cache if you implemented it
            LoadData();
        }
    }
}