using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GlobusNet
{
    public partial class PaymentWindow : Window
    {
        bool IsNew = false;
        string uId;
        string cId;
        string sId;
        public PaymentWindow(GUser gu, GPayment gp)
        {
            InitializeComponent();
            int cid = App.UserID;
            if (gp == null)
            {
                IsNew = true;
                pmDate.SelectedDate = DateTime.Now;
                pmSpan.SelectedIndex = 0;
            }
            else
            {
                cid = gp.Cashier;
                pmDate.SelectedDate = gp.Paid;
                pmSpan.SelectedIndex = (gp.Span == 6 ? 2 : (gp.Span == 3 ? 1 : 0));
            }
            pmDate.IsEnabled = IsNew;
            btn1.Visibility = (IsNew ? Visibility.Visible : Visibility.Hidden);
            btn2.Visibility = (IsNew ? Visibility.Visible : Visibility.Hidden);
            pmSpan.Visibility = (IsNew ? Visibility.Visible : Visibility.Hidden);
            pmUser.Text = gu.Fullname;
            pmService.Text = gu.Subscription.Service.Name;
            pmCashier.Text = App.WebFunction("get_app_user_name", cid.ToString());
            uId = gu.Id.ToString();
            cId = cid.ToString();
            sId = gu.Subscription.Service.Id.ToString();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan tstamp = ((DateTime)pmDate.SelectedDate - new DateTime(1970, 1, 1)).Add(DateTime.Now.TimeOfDay);
            App.WebFunction("pay", uId, sId, (pmSpan.SelectedItem as ComboBoxItem).Tag.ToString(), string.Format("{0}|{1}", cId, tstamp.TotalSeconds));
            (Owner as MainWindow).ReloadUsers();
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
