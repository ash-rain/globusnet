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
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            cbSvc.ItemsSource = MainWindow.Svc;
            cbSvc.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) { Close(); }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (tbPass1.Password == tbPass2.Password)
            {
                if(tbName.Text.Length > 0 && tbPass1.Password.Length > 0 && tbUser.Text.Length > 0)
                {
                    if (cb.SelectedIndex == 0)
                    {
                        string ins = App.WebFunction("add_user", tbUser.Text, tbPass1.Password, tbName.Text);
                        App.WebFunction("subscribe", ins, (cbSvc.SelectedItem as GService).Id.ToString());
                    }
                    else
                        App.WebFunction("add_app_user", tbUser.Text, tbPass1.Password, tbName.Text, cb.SelectedIndex.ToString());
                    (Owner as MainWindow).ReloadUsers();
                    Close();
                }
                else MessageBox.Show("Оставени са празни полета");
            }
            else MessageBox.Show("Паролата не е потвърдена");
        }
    }
}
