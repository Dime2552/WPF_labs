using System.Windows.Input;

namespace lab6
{
    public static class DataCommands
    {
        public static readonly RoutedUICommand Undo = new RoutedUICommand(
            "Скасувати", "Undo", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.Z, ModifierKeys.Control) }
        );

        public static readonly RoutedUICommand New = new RoutedUICommand(
            "Створити", "New", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) }
        );

        public static readonly RoutedUICommand Edit = new RoutedUICommand(
            "Редагувати", "Edit", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.E, ModifierKeys.Control) } // Або F2
        );

        // Команда Save залишається, але не буде використовуватися на HistoryPage
        public static readonly RoutedUICommand Save = new RoutedUICommand(
            "Зберегти", "Save", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) }
        );

        // Команда Find залишається, але не буде використовуватися на HistoryPage
        public static readonly RoutedUICommand Find = new RoutedUICommand(
            "Знайти", "Find", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control) }
        );

        public static readonly RoutedUICommand Delete = new RoutedUICommand(
            "Видалити", "Delete", typeof(DataCommands),
            new InputGestureCollection() { new KeyGesture(Key.D, ModifierKeys.Control) }
        );
    }
}