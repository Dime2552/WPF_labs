// MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Linq;

namespace lab6
{
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
        private bool _isUpdatingInternally;

        private ObservableCollection<ConversionEntry> _historyEntries; // Оригінальний повний список

        private ObservableCollection<ConversionEntry> _filteredHistoryEntries;
        public ObservableCollection<ConversionEntry> FilteredHistoryEntries
        {
            get => _filteredHistoryEntries;
            set { _filteredHistoryEntries = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                    (ClearSearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private int? _editingEntryId = null; // Зберігає ID запису, який зараз редагується, або null

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string DecimalInput
        {
            get => _decimalInput;
            set { SetNumericInput(ref _decimalInput, value, 10, nameof(DecimalInput)); }
        }
        public string BinaryInput
        {
            get => _binaryInput;
            set { SetNumericInput(ref _binaryInput, value, 2, nameof(BinaryInput)); }
        }
        public string OctalInput
        {
            get => _octalInput;
            set { SetNumericInput(ref _octalInput, value, 8, nameof(OctalInput)); }
        }
        public string HexInput
        {
            get => _hexInput;
            set { SetNumericInput(ref _hexInput, value, 16, nameof(HexInput)); }
        }

        public ICommand ClearCommand { get; }
        public ICommand ConvertAndSaveCommand { get; }
        public ICommand RefreshHistoryCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _historyEntries = new ObservableCollection<ConversionEntry>();
            FilteredHistoryEntries = new ObservableCollection<ConversionEntry>();

            ClearCommand = new RelayCommand(ExecuteClearFields, CanExecuteClearFields);
            ConvertAndSaveCommand = new RelayCommand(ExecuteConvertAndSave, CanExecuteConvertAndSave);
            RefreshHistoryCommand = new RelayCommand(LoadHistoryData);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch, CanExecuteClearSearch);

            LoadHistoryData(null);
        }

        private void SetNumericInput(ref string backingField, string newValue, int fromBase, string propertyName)
        {
            if (backingField == newValue && !_isUpdatingInternally)
                return;
            if (_isUpdatingInternally && backingField == newValue)
                return;

            backingField = newValue;
            OnPropertyChanged(propertyName);

            if (_isUpdatingInternally)
                return;

            int numberValue = 0;
            bool parsedSuccessfully = false;

            if (!string.IsNullOrWhiteSpace(newValue))
            {
                if (TryParseBase(newValue, fromBase, out numberValue))
                {
                    parsedSuccessfully = true;
                }
            }

            if (parsedSuccessfully)
            {
                _isUpdatingInternally = true;
                try
                {
                    if (propertyName != nameof(DecimalInput))
                        DecimalInput = numberValue.ToString();
                    if (propertyName != nameof(BinaryInput))
                        BinaryInput = Convert.ToString(numberValue, 2);
                    if (propertyName != nameof(OctalInput))
                        OctalInput = Convert.ToString(numberValue, 8);
                    if (propertyName != nameof(HexInput))
                        HexInput = Convert.ToString(numberValue, 16).ToUpper();
                }
                finally
                {
                    _isUpdatingInternally = false;
                }
            }
            CommandManager.InvalidateRequerySuggested();
        }

        public void LoadForEditing(ConversionEntry entry)
        {
            if (entry == null)
                return;

            _isUpdatingInternally = true;
            try
            {
                Username = entry.Username;
                DecimalInput = entry.Decimal;
                BinaryInput = entry.Binary;
                OctalInput = entry.Octal;
                HexInput = entry.Hexadecimal;
                _editingEntryId = entry.Id; // ЗБЕРІГАЄМО ID ДЛЯ РЕДАГУВАННЯ
            }
            finally
            {
                _isUpdatingInternally = false;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void LoadHistoryData(object parameter)
        {
            _historyEntries.Clear();
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
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _historyEntries.Add(new ConversionEntry
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Decimal = reader.IsDBNull(reader.GetOrdinal("Decimal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Decimal")),
                                Binary = reader.IsDBNull(reader.GetOrdinal("Binary")) ? string.Empty : reader.GetString(reader.GetOrdinal("Binary")),
                                Octal = reader.IsDBNull(reader.GetOrdinal("Octal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Octal")),
                                Hexadecimal = reader.IsDBNull(reader.GetOrdinal("Hexadecimal")) ? string.Empty : reader.GetString(reader.GetOrdinal("Hexadecimal"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredHistoryEntries.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var entry in _historyEntries)
                {
                    FilteredHistoryEntries.Add(entry);
                }
            }
            else
            {
                string lowerSearchText = SearchText.ToLowerInvariant();
                var query = _historyEntries.Where(entry =>
                    (entry.Username?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (entry.Decimal?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (entry.Binary?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (entry.Octal?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (entry.Hexadecimal?.ToLowerInvariant().Contains(lowerSearchText) ?? false)
                );

                foreach (var entry in query)
                {
                    FilteredHistoryEntries.Add(entry);
                }
            }
        }

        private bool CanExecuteClearSearch(object parameter)
        {
            return !string.IsNullOrWhiteSpace(SearchText);
        }

        private void ExecuteClearSearch(object parameter)
        {
            SearchText = string.Empty;
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

        private bool CanExecuteConvertAndSave(object parameter)
        {
            bool hasValidNumber = false;
            if (!string.IsNullOrWhiteSpace(DecimalInput) && TryParseBase(DecimalInput, 10, out _))
                hasValidNumber = true;
            else if (!string.IsNullOrWhiteSpace(BinaryInput) && TryParseBase(BinaryInput, 2, out _))
                hasValidNumber = true;
            else if (!string.IsNullOrWhiteSpace(OctalInput) && TryParseBase(OctalInput, 8, out _))
                hasValidNumber = true;
            else if (!string.IsNullOrWhiteSpace(HexInput) && TryParseBase(HexInput, 16, out _))
                hasValidNumber = true;

            return !string.IsNullOrWhiteSpace(Username) && hasValidNumber;
        }

        private void ExecuteConvertAndSave(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Будь ласка, введіть ім'я користувача.", "Потрібне ім'я користувача", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int numberValue = 0;
            bool conversionSuccess = false;

            if (!string.IsNullOrWhiteSpace(DecimalInput) && TryParseBase(DecimalInput, 10, out numberValue))
                conversionSuccess = true;
            else if (!string.IsNullOrWhiteSpace(BinaryInput) && TryParseBase(BinaryInput, 2, out numberValue))
                conversionSuccess = true;
            else if (!string.IsNullOrWhiteSpace(OctalInput) && TryParseBase(OctalInput, 8, out numberValue))
                conversionSuccess = true;
            else if (!string.IsNullOrWhiteSpace(HexInput) && TryParseBase(HexInput, 16, out numberValue))
                conversionSuccess = true;

            if (conversionSuccess)
            {
                _isUpdatingInternally = true;
                try
                {
                    this.DecimalInput = numberValue.ToString();
                    this.BinaryInput = Convert.ToString(numberValue, 2);
                    this.OctalInput = Convert.ToString(numberValue, 8);
                    this.HexInput = Convert.ToString(numberValue, 16).ToUpper();
                }
                finally { _isUpdatingInternally = false; }

                SaveToDatabase();
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть дійсне число в одне з полів для конвертації.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveToDatabase()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            { MessageBox.Show("Рядок підключення не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query;
                    SqlCommand command;

                    if (_editingEntryId.HasValue)
                    {
                        query = @"UPDATE dbo.[Table] SET 
                                    Username = @Username, 
                                    [Decimal] = @Decimal, 
                                    Binary = @Binary, 
                                    Octal = @Octal, 
                                    Hexadecimal = @Hexadecimal 
                                  WHERE Id = @Id";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", _editingEntryId.Value);
                    }
                    else
                    {
                        int nextId = 1;
                        string getMaxIdQuery = "SELECT MAX(Id) FROM dbo.[Table]";
                        using (SqlCommand getMaxIdCommand = new SqlCommand(getMaxIdQuery, connection))
                        {
                            object result = getMaxIdCommand.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                                nextId = Convert.ToInt32(result) + 1;
                        }

                        query = "INSERT INTO dbo.[Table] (Id, Username, [Decimal], Binary, Octal, Hexadecimal) VALUES (@Id, @Username, @Decimal, @Binary, @Octal, @Hexadecimal)";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", nextId);
                    }

                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Decimal", (object)DecimalInput ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Binary", (object)BinaryInput ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Octal", (object)OctalInput ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Hexadecimal", (object)HexInput ?? DBNull.Value);

                    if (command.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show(_editingEntryId.HasValue ? "Дані успішно оновлено!" : $"Дані успішно збережено!",
                                        "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadHistoryData(null);
                        _editingEntryId = null;
                    }
                    else
                    {
                        MessageBox.Show(_editingEntryId.HasValue ? "Не вдалося оновити дані." : "Не вдалося зберегти дані.",
                                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Помилка при роботі з БД: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanExecuteClearFields(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) ||
                   !string.IsNullOrWhiteSpace(DecimalInput) ||
                   !string.IsNullOrWhiteSpace(BinaryInput) ||
                   !string.IsNullOrWhiteSpace(OctalInput) ||
                   !string.IsNullOrWhiteSpace(HexInput) ||
                   _editingEntryId.HasValue;
        }

        private void ExecuteClearFields(object parameter)
        {
            _isUpdatingInternally = true;
            try
            {
                Username = string.Empty;
                DecimalInput = string.Empty;
                BinaryInput = string.Empty;
                OctalInput = string.Empty;
                HexInput = string.Empty;
                _editingEntryId = null; // Скидаємо режим редагування
            }
            finally { _isUpdatingInternally = false; }
            CommandManager.InvalidateRequerySuggested();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}