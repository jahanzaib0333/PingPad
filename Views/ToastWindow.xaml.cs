using System;
using System.Windows;
using System.Windows.Threading;

namespace PingPad.Views
{
    public partial class ToastWindow : Window
    {
        private readonly DispatcherTimer _autoClose;
        private readonly StickyNoteWindow _targetNote;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ToastWindow(string appName, string reminderTime, string noteTitle, string notePreview, StickyNoteWindow? targetNote = null)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();

            Title = appName;
            TimeBlock.Text = reminderTime;   // dynamic reminder time
            TitleText.Text = noteTitle;      // sticky note title
            ContentBlock.Text = notePreview; // note content
#pragma warning disable CS8601 // Possible null reference assignment.
            _targetNote = targetNote;
#pragma warning restore CS8601 // Possible null reference assignment.

            Loaded += ToastWindow_Loaded;

            // Auto-close after 8 seconds
            _autoClose = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
            _autoClose.Tick += (s, e) => Close();
            _autoClose.Start();

            MouseLeftButtonUp += ToastWindow_MouseLeftButtonUp;
        }

        private void ToastWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PositionAtBottomRight();
        }

        private void PositionAtBottomRight()
        {
            var wa = SystemParameters.WorkArea;
            Left = wa.Right - Width - 12;
            Top = wa.Bottom - Height - 12;
        }

        private void ToastWindow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_targetNote != null)
                {
                    _targetNote.Activate();
                    // Try to trigger its highlight animation if it exists
                    _targetNote.GetType().GetMethod("StartHighlightAnimation",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?.Invoke(_targetNote, null);
                }
            }
            catch
            {
                // Safe fail
            }
            Close();
        }
    }
}
