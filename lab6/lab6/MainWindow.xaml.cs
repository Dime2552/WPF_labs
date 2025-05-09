using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;

namespace lab6
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }

    public class ConversionEntry
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Decimal { get; set; }
        public string Binary { get; set; }
        public string Octal { get; set; }
        public string Hexadecimal { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _decimalInput;
        private string _binaryInput;
        private string _octalInput;
        private string _hexInput;
        private bool _isUpdating;

        private ObservableCollection<ConversionEntry> _historyEntries;
        public ObservableCollection<ConversionEntry> HistoryEntries
        {
            get => _historyEntries;
            set
            {
                _historyEntries = value;
                OnPropertyChanged(nameof(HistoryEntries));
            }
        }

        // --- Існуючі властивості ---
        public string Username
        {
            get => _username;
            set { if (_username != value) { _username = value; OnPropertyChanged(nameof(Username)); } }
        }
        public string DecimalInput
        {
            get => _decimalInput;
            set { if (_decimalInput != value) { _decimalInput = value; OnPropertyChanged(nameof(DecimalInput)); } }
        }
        public string BinaryInput
        {
            get => _binaryInput;
            set { if (_binaryInput != value) { _binaryInput = value; OnPropertyChanged(nameof(BinaryInput)); } }
        }
        public string OctalInput
        {
            get => _octalInput;
            set { if (_octalInput != value) { _octalInput = value; OnPropertyChanged(nameof(OctalInput)); } }
        }
        public string HexInput
        {
            get => _hexInput;
            set { if (_hexInput != value) { _hexInput = value; OnPropertyChanged(nameof(HexInput)); } }
        }

        // --- Команди ---
        public ICommand ClearCommand { get; }
        public ICommand ConvertAndSaveCommand { get; }
        public ICommand RefreshHistoryCommand { get; } // Нова команда

        public event PropertyChangedEventHandler PropertyChanged;

        // --- Конструктор ---
        public MainViewModel()
        {
            HistoryEntries = new ObservableCollection<ConversionEntry>();

            ClearCommand = new RelayCommand(ClearFields);
            ConvertAndSaveCommand = new RelayCommand(ExecuteConvertAndSave, CanExecuteConvertAndSave);
            RefreshHistoryCommand = new RelayCommand(LoadHistoryData); // Прив'язка команди оновлення

            LoadHistoryData(null);
        }

        // --- Логіка завантаження історії ---
        private void LoadHistoryData(object parameter)
        {
            HistoryEntries.Clear(); // Очищуємо перед завантаженням

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Рядок підключення для історії не знайдено.", "Помилка конфігурації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string query = "SELECT Id, Username, [Decimal], Binary, Octal, Hexadecimal FROM dbo.[Table] ORDER BY Id DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new ConversionEntry
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                    Decimal = reader.IsDBNull(reader.GetOrdinal("Decimal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Decimal")),
                                    Binary = reader.IsDBNull(reader.GetOrdinal("Binary")) ? string.Empty : reader.GetString(reader.GetOrdinal("Binary")),
                                    Octal = reader.IsDBNull(reader.GetOrdinal("Octal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Octal")),
                                    Hexadecimal = reader.IsDBNull(reader.GetOrdinal("Hexadecimal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Hexadecimal"))
                                };
                                HistoryEntries.Add(entry);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- Логіка конвертації ---
        private void UpdateFieldsFromNumber(int number)
        {
            _isUpdating = true;
            try
            {
                DecimalInput = number.ToString();
                BinaryInput = Convert.ToString(number, 2);
                OctalInput = Convert.ToString(number, 8);
                HexInput = Convert.ToString(number, 16).ToUpper();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private bool TryParseBase(string input, int fromBase, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;
            try
            {
                result = Convert.ToInt32(input, fromBase);
                return true;
            }
            catch { return false; }
        }

        // --- Логіка команд ---
        private bool CanExecuteConvertAndSave(object parameter)
        {
            return true;
        }

        private void ExecuteConvertAndSave(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Будь ласка, введіть ім'я користувача перед збереженням.",
                               "Потрібне ім'я користувача", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int numberValue = 0;
            bool conversionSuccess = false;

            if (TryParseBase(DecimalInput, 10, out numberValue))
            { conversionSuccess = true; }
            else if (TryParseBase(BinaryInput, 2, out numberValue))
            { conversionSuccess = true; }
            else if (TryParseBase(OctalInput, 8, out numberValue))
            { conversionSuccess = true; }
            else if (TryParseBase(HexInput, 16, out numberValue))
            { conversionSuccess = true; }

            if (conversionSuccess)
            {
                UpdateFieldsFromNumber(numberValue);
                SaveToDatabase();
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть дійсне число в одне з полів.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveToDatabase()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Рядок підключення не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int nextId = 1;
            string getMaxIdQuery = "SELECT MAX(Id) FROM dbo.[Table]";
            string insertQuery = @"INSERT INTO dbo.[Table] (Id, Username, [Decimal], Binary, Octal, Hexadecimal)
                           VALUES (@Id, @Username, @Decimal, @Binary, @Octal, @Hexadecimal)";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand getMaxIdCommand = new SqlCommand(getMaxIdQuery, connection))
                    {
                        object result = getMaxIdCommand.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            nextId = Convert.ToInt32(result) + 1;
                        }
                    }

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", nextId);
                        command.Parameters.AddWithValue("@Username", Username);
                        command.Parameters.AddWithValue("@Decimal", (object)DecimalInput ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Binary", (object)BinaryInput ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Octal", (object)OctalInput ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Hexadecimal", (object)HexInput ?? DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"Дані успішно збережено! Призначено ID: {nextId}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Оновлюємо історію після успішного збереження
                            LoadHistoryData(null);
                        }
                        else
                        {
                            MessageBox.Show("Не вдалося зберегти дані.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при роботі з БД: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFields(object parameter)
        {
            _isUpdating = true;
            Username = string.Empty;
            DecimalInput = string.Empty;
            BinaryInput = string.Empty;
            OctalInput = string.Empty;
            HexInput = string.Empty;
            _isUpdating = false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}