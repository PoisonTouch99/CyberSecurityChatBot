using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CyberSecurityChatBot.ViewModels;

namespace CyberSecurityChatBot.Views
{
    /// <summary>
    /// Code-behind for MainWindow. Handles auto-scroll on new messages.
    /// Converters are declared in XAML (Window.Resources) — NOT here —
    /// because InitializeComponent() parses XAML before any code below it runs,
    /// so Resources.Add() after InitializeComponent() is always too late.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChatViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as ChatViewModel;

            if (_viewModel != null)
            {
                ((INotifyCollectionChanged)_viewModel.Messages)
                    .CollectionChanged += Messages_CollectionChanged;
            }

            InputBox.Focus();
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                ChatScrollViewer.ScrollToEnd();
        }
    }

    // ── Value Converters (referenced from Window.Resources in XAML via xmlns:local) ──

    /// <summary>True → Visible, False → Collapsed. Used for user message bubbles.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>True → Collapsed, False → Visible. Used for bot message bubbles.</summary>
    public class BoolToInvisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
