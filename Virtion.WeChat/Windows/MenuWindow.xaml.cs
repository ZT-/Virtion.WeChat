using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Virtion.WeChat;
using WeChat;
using WeChat.Controls.Item;

namespace Virtion.Windows
{
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
        }

        public void SetPosition(int left, int top)
        {
            this.Left = left - this.ActualWidth;
            this.Top = top - this.ActualHeight;
            this.Activate();
   
            //Console.WriteLine("SetPosition");
        }

        public BaseMenuItem SetMenuItem(String name, String awesomeCharIcon, MouseButtonEventHandler callback)
        {
            BaseMenuItem item = new BaseMenuItem()
            {
                Text = name,
                AwesomeCharIcon = awesomeCharIcon
            };
            item.MouseDown += (s, e) =>
            {
                callback(s, e);
                this.Hide();
            };
            this.SP_Content.Children.Add(item);
            return item;
        }

        private void CreateMenuItem()
        {
            this.SetMenuItem("退出", '\uf08b'.ToString(), (s, e) =>
            {
                App.Current.Shutdown();
            });
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreateMenuItem();
            this.Activate();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
