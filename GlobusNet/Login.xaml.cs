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
    public partial class Login : Window
    {
        public Login()
        {
            List<string> users = App.TransferRow(App.WebFunction("get_app_users", "name"));
            InitializeComponent();
            tbUser.ItemsSource = users;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] response = App.WebFunction("app_login", tbUser.Text, tbPass.Password).Split('*');
            int role = int.Parse(response[0]);
            int id = int.Parse(response[1]);
            if (role > 0 && id > 0)
            {
                App.UserID = id;
                App.UserRole = role;
                Window w = (Window)new MainWindow();
                w.Show();
                Close();
            }
            else MessageBox.Show("Невалидно потребителско име или парола!");
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void tbUser_KeyUp(object sender, KeyEventArgs e) { if (e.Key.Equals(Key.Enter)) tbPass.Focus(); }
        private void tbPass_KeyUp(object sender, KeyEventArgs e) { if (e.Key.Equals(Key.Enter)) Button_Click(null, new RoutedEventArgs()); }
    }
}
