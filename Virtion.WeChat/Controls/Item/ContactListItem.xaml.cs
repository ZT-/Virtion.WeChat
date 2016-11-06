using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Virtion.WeChat.Struct;

namespace Virtion.WeChat.Controls.Item
{
    public partial class ContactListItem : UserControl
    {
        public string DisplayName
        {
            get
            {
                return this.TB_Name.Text;
            }
            set
            {
                this.TB_Name.Text = value;
            }
        }
        public User User;

        public static SolidColorBrush HightLightBackgroundBrush
                         = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));

        public static SolidColorBrush NormalBackgroundBrush
                         = new SolidColorBrush(Colors.Transparent);

        public static SolidColorBrush HightLightTextBrush
                         = new SolidColorBrush(Colors.White);

        public static SolidColorBrush NormalTextBrush
                        = new SolidColorBrush(Colors.Black);

        public ContactListItem()
        {
            InitializeComponent();
        }

        private int tipNumber;

        public void AddTipNumber()
        {
            this.tipNumber ++;
            this.B_Tip.Visibility = System.Windows.Visibility.Visible;
            this.L_Number.Content = this.tipNumber;
        }

        public void RemoveTip()
        {
            this.B_Tip.Visibility = System.Windows.Visibility.Hidden;
            this.tipNumber = 0;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = HightLightBackgroundBrush;
            this.Foreground = HightLightTextBrush;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = NormalBackgroundBrush;
            this.Foreground = NormalTextBrush;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow.AvatarConverter.SetRequest(User,this.I_Avatar);
        }
    }
}
