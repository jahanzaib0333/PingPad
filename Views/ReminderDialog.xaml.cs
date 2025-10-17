using System;
using System.Windows;

namespace PingPad.Views
{
    public partial class ReminderDialog : Window
    {
        public DateTime? SelectedDateTime { get; private set; }

        public ReminderDialog()
        {
            InitializeComponent();
            Dp.SelectedDate = DateTime.Today;
            CbAmPm.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Dp.SelectedDate.HasValue)
            {
                MessageBox.Show("Select a date.", "PingPad");
                return;
            }

            var raw = TbTime.Text.Trim();
            var parts = raw.Split(':');
            if (parts.Length < 2 || !int.TryParse(parts[0], out int hour) || !int.TryParse(parts[1], out int minute))
            {
                MessageBox.Show("Enter time as HH:mm (e.g. 05:30 or 17:45).", "PingPad");
                return;
            }

            var isPm = (CbAmPm.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == "PM";
            if (isPm && hour < 12) hour += 12;
            if (!isPm && hour == 12) hour = 0;

            SelectedDateTime = Dp.SelectedDate.Value.Date + new TimeSpan(hour, minute, 0);
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
