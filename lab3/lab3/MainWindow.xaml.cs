using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _decimalInput;
        private string _binaryInput;
        private string _octalInput;
        private string _hexInput;
        private bool _isUpdating;

        public string DecimalInput
        {
            get => _decimalInput;
            set
            {
                if (_decimalInput != value)
                {
                    _decimalInput = value;
                    OnPropertyChanged("DecimalInput");
                    if (!_isUpdating)
                        ConvertFromDecimal();
                }
            }
        }
        public string BinaryInput
        {
            get => _binaryInput;
            set
            {
                if (_binaryInput != value)
                {
                    _binaryInput = value;
                    OnPropertyChanged("BinaryInput");
                    if (!_isUpdating)
                        ConvertFromBinary();
                }
            }
        }
        public string OctalInput
        {
            get => _octalInput;
            set
            {
                if (_octalInput != value)
                {
                    _octalInput = value;
                    OnPropertyChanged("OctalInput");
                    if (!_isUpdating)
                        ConvertFromOctal();
                }
            }
        }
        public string HexInput
        {
            get => _hexInput;
            set
            {
                if (_hexInput != value)
                {
                    _hexInput = value;
                    OnPropertyChanged("HexInput");
                    if (!_isUpdating)
                        ConvertFromHex();
                }
            }
        }

        public ICommand ClearCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            ClearCommand = new RelayCommand(ClearFields);
        }

        private void ConvertFromDecimal()
        {
            if (int.TryParse(DecimalInput, out int number))
            {
                _isUpdating = true;
                BinaryInput = Convert.ToString(number, 2);
                OctalInput = Convert.ToString(number, 8);
                HexInput = Convert.ToString(number, 16).ToUpper();
                _isUpdating = false;
            }
        }

        private void ConvertFromBinary()
        {
            if (TryParseBase(BinaryInput, 2, out int number))
            {
                _isUpdating = true;
                DecimalInput = number.ToString();
                OctalInput = Convert.ToString(number, 8);
                HexInput = Convert.ToString(number, 16).ToUpper();
                _isUpdating = false;
            }
        }

        private void ConvertFromOctal()
        {
            if (TryParseBase(OctalInput, 8, out int number))
            {
                _isUpdating = true;
                DecimalInput = number.ToString();
                BinaryInput = Convert.ToString(number, 2);
                HexInput = Convert.ToString(number, 16).ToUpper();
                _isUpdating = false;
            }
        }

        private void ConvertFromHex()
        {
            if (TryParseBase(HexInput, 16, out int number))
            {
                _isUpdating = true;
                DecimalInput = number.ToString();
                BinaryInput = Convert.ToString(number, 2);
                OctalInput = Convert.ToString(number, 8);
                _isUpdating = false;
            }
        }

        private bool TryParseBase(string input, int fromBase, out int result)
        {
            try
            {
                result = Convert.ToInt32(input, fromBase);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        private void ClearFields(object parameter)
        {
            _isUpdating = true;
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
            _execute = execute;
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