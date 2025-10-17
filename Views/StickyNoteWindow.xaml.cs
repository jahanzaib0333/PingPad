#nullable disable
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PingPad.Views
{
    public partial class StickyNoteWindow : Window
    {
        private DispatcherTimer _reminderChecker;
        private DateTime? _reminderAt;
        private bool _isHighlighting;

        public StickyNoteWindow()
        {
            InitializeComponent();

            if (NoteBox.Document.Blocks.Count == 0)
                NoteBox.Document.Blocks.Add(new Paragraph(new Run("")));

            _reminderChecker = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _reminderChecker.Tick += ReminderChecker_Tick;
            _reminderChecker.Start();
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitlePlaceholder.Visibility = string.IsNullOrWhiteSpace(TitleTextBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            new StickyNoteWindow().Show();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete this note?", "PingPad", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnBold_Click(object sender, RoutedEventArgs e) => EditingCommands.ToggleBold.Execute(null, NoteBox);
        private void BtnItalic_Click(object sender, RoutedEventArgs e) => EditingCommands.ToggleItalic.Execute(null, NoteBox);
        private void BtnUnderline_Click(object sender, RoutedEventArgs e) => EditingCommands.ToggleUnderline.Execute(null, NoteBox);
        private void BtnBullets_Click(object sender, RoutedEventArgs e) => EditingCommands.ToggleBullets.Execute(null, NoteBox);

        private void BtnStrike_Click(object sender, RoutedEventArgs e)
        {
            var sel = NoteBox.Selection;
            if (sel == null || sel.IsEmpty) return;

            var current = sel.GetPropertyValue(Inline.TextDecorationsProperty);
            if (current == DependencyProperty.UnsetValue || !HasDecoration(current, TextDecorationLocation.Strikethrough))
                sel.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            else
                sel.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }

        private static bool HasDecoration(object currentValue, TextDecorationLocation location)
        {
            if (currentValue == null || currentValue == DependencyProperty.UnsetValue) return false;
            if (currentValue is TextDecorationCollection coll) return coll.Any(d => d.Location == location);
            return false;
        }

        private void BtnColor_Click(object sender, RoutedEventArgs e) => PalettePopup.IsOpen = true;

        private void ColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string key)
            {
                ApplyColorKey(key);
                PalettePopup.IsOpen = false;
            }
        }

        private void ApplyColorKey(string key)
        {
            SolidColorBrush brush = key switch
            {
                "Yellow" => new SolidColorBrush(Color.FromRgb(255, 241, 120)),
                "Pink" => new SolidColorBrush(Color.FromRgb(255, 210, 230)),
                "Blue" => new SolidColorBrush(Color.FromRgb(210, 230, 255)),
                "Green" => new SolidColorBrush(Color.FromRgb(210, 255, 210)),
                "Purple" => new SolidColorBrush(Color.FromRgb(235, 210, 255)),
                "Gray" => new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                "White" => Brushes.White,
                _ => new SolidColorBrush(Color.FromRgb(255, 241, 120))
            };
            RootBorder.Background = brush;
        }

        private void BtnReminder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ReminderDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                _reminderAt = dlg.SelectedDateTime;
                MessageBox.Show($"Reminder set for {_reminderAt.Value:f}", "PingPad");
            }
        }

        private void ReminderChecker_Tick(object sender, EventArgs e)
        {
            if (_reminderAt.HasValue && DateTime.Now >= _reminderAt.Value)
            {
                _reminderChecker.Stop();
                ShowToastAndHighlight();
                _reminderAt = null;
                _reminderChecker.Start();
            }
        }

        private void NoteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NotePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void NoteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(NoteBox.Document.ContentStart, NoteBox.Document.ContentEnd);
            bool isEmpty = string.IsNullOrWhiteSpace(textRange.Text) || textRange.Text == "\r\n";
            NotePlaceholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowToastAndHighlight()
        {
            string noteTitle = string.IsNullOrWhiteSpace(TitleTextBox.Text)
                ? "PingPad Note" : TitleTextBox.Text.Trim();

            string preview = new TextRange(NoteBox.Document.ContentStart, NoteBox.Document.ContentEnd).Text.Trim();
            if (preview.Length > 400) preview = preview.Substring(0, 400) + "...";

            string reminderTime = _reminderAt?.ToString("hh:mm tt") ?? DateTime.Now.ToString("hh:mm tt");

            var toast = new ToastWindow("PingPad", $"Reminder! {reminderTime}", noteTitle, preview, this);
            toast.Show();
        }

        private void StartHighlightAnimation()
        {
            if (_isHighlighting) return;
            _isHighlighting = true;

            var origBrush = RootBorder.BorderBrush as SolidColorBrush ?? new SolidColorBrush(Colors.Transparent);
            var highlightBrush = Brushes.Red;
            var animatedBrush = new SolidColorBrush(origBrush.Color);
            RootBorder.BorderBrush = animatedBrush;

            var colorAnim = new ColorAnimation
            {
                From = origBrush.Color,
                To = highlightBrush.Color,
                Duration = TimeSpan.FromMilliseconds(350),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(6)
            };

            var thicknessAnim = new ThicknessAnimation
            {
                From = new Thickness(1),
                To = new Thickness(6),
                Duration = TimeSpan.FromMilliseconds(350),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(6)
            };

            colorAnim.Completed += (s, e) =>
            {
                RootBorder.BorderBrush = origBrush;
                RootBorder.BorderThickness = new Thickness(1);
                _isHighlighting = false;
            };

            animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            RootBorder.BeginAnimation(Border.BorderThicknessProperty, thicknessAnim);
        }
    }
}
