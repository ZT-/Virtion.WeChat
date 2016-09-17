using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WeChat.Controls.Item
{
    public partial class BaseMenuItem : UserControl
    {
        public String Text
        {
            get
            {
                return this.TextContent.Text;
            }
            set
            {
                this.TextContent.Text = value;
            }
        }

        public String AwesomeCharIcon
        {
            get
            {
                return this.IconContent.Text;
            }
            set
            {
                this.IconContent.Text =  value;
            }
        }

        public BaseMenuItem()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.G_Content.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));

        }
        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.G_Content.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEEE"));
        }
    }
}
