// HistoryPage.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Navigation;

namespace lab6
{
    public partial class HistoryPage : Page
    {
        private MainViewModel _viewModel;

        public HistoryPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = MainWindow.SharedViewModel;
            DataContext = _viewModel;
            CommandManager.InvalidateRequerySuggested();
        }

        // --- Undo ---
        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false; // Поки не реалізовано
            e.Handled = true;
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Команда 'Скасувати' викликана (реалізація не передбачена).", "Інформація");
            e.Handled = true;
        }

        // --- New ---
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Команда "Створити" (очистити поля конвертера і перейти) тепер завжди доступна з HistoryPage
            e.CanExecute = true;
            e.Handled = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Якщо ViewModel існує, викликаємо очищення полів
            _viewModel?.ClearCommand?.Execute(null);
            // Повідомлення та навігація
            MessageBox.Show("Поля конвертера очищено. Перейдіть на сторінку конвертера для введення нових даних.", "Створено новий запис");
            this.NavigationService?.Navigate(new Uri("ConverterPage.xaml", UriKind.Relative));
            e.Handled = true;
        }

        // --- Edit ---
        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HistoryDataGrid != null)
            {
                e.CanExecute = HistoryDataGrid.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is ConversionEntry selectedEntry)
            {
                _viewModel?.LoadForEditing(selectedEntry);
                this.NavigationService?.Navigate(new Uri("ConverterPage.xaml", UriKind.Relative));
                MessageBox.Show($"Дані для запису ID: {selectedEntry.Id} завантажено в конвертер. \nПісля змін збережіть їх знову.", "Редагування");
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть запис для редагування.", "Редагування");
            }
            e.Handled = true;
        }

        // Обробники для SaveCommand та FindCommand видалені,
        // оскільки відповідні CommandBinding та UI елементи видалені з HistoryPage.xaml

        // --- Delete ---
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HistoryDataGrid != null)
            {
                e.CanExecute = HistoryDataGrid.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is ConversionEntry selectedEntry)
            {
                var result = MessageBox.Show($"Видалити запис ID: {selectedEntry.Id} ({selectedEntry.Username})?",
                                             "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteEntryFromDatabase(selectedEntry.Id);
                    _viewModel?.RefreshHistoryCommand.Execute(null);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть запис для видалення.", "Видалення");
            }
            e.Handled = true;
        }

        private void DeleteEntryFromDatabase(int entryId)
        {
            // ... (код без змін) ...
            string cs = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(cs))
            { MessageBox.Show("Рядок підключення не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.[Table] WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", entryId);
                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                        MessageBox.Show($"Запис ID: {entryId} видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show($"Не вдалося видалити запис ID: {entryId}.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Помилка видалення з БД: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}