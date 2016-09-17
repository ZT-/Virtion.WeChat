using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Virtion.UI.Control
{
    public partial class LoadingMask : UserControl
    {
        public static bool IsError;
        public bool IsLoading
        {
            get
            {
                if (this.TB_Loading.Visibility == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                this.ShowLoading(value);
            }
        }
        public string Tip
        {
            get
            {
                return this.TB_Text.Text;
            }
            set
            {
                this.TB_Text.Text = value;
            }
        }

        private Timer timer;

        public LoadingMask()
        {
            InitializeComponent();

            IsError = false;
        }


        public void ShowLoading(bool isShow)
        {
            if (isShow == true)
            {
                timer = new Timer();
                timer.Interval = 100;
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Elapsed += Timer_Elapsed;

                this.TB_Loading.Visibility = Visibility.Visible;
            }
            else
            {
                if (timer != null)
                {
                    timer.Enabled = false;
                    timer = null;
                }
                this.TB_Loading.Visibility = Visibility.Hidden;
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (IsError == true)
                {
                    this.ShowLoading(false);
                }
            }));
        }

    }
}
