using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WalkCompanion.Controls
{
    public partial class PushPinControl : UserControl
    {
        public PushPinControl()
        {
            InitializeComponent();
        }

        public void ChangeBackgroundToRed()
        {
            imgborder.Background = new SolidColorBrush(Colors.Red);
            imgpath.Fill = new SolidColorBrush(Colors.Red);
        }

        public void ChangeBackgroundToGrey()
        {
            imgborder.Background = new SolidColorBrush(Colors.LightGray);
            imgpath.Fill = new SolidColorBrush(Colors.LightGray);
        }

        public void FillDescription(string title, string time)
        {
            Title.Text = title;
            Time.Text = time;

            if (string.IsNullOrWhiteSpace(time))
            {
                Time.Visibility = Visibility.Collapsed;
                MainStackPanel.Margin = new Thickness(0, -49, 0, 0);
            }
        }
    }
}
