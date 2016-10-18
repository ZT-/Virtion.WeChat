using System.Windows;

namespace Virtion.WeChat.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginWindow_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Close();
        }
    }
}
