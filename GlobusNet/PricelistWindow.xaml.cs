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
    public partial class PricelistWindow : Window
    {
        DGPricelist pp;
        public PricelistWindow(DGPricelist p)
        {
            InitializeComponent();
            plBranch.Text = p.Branch;
            plService.Text = p.Service;
            plPrice1.Text = p.Price1.ToString();
            plPrice3.Text = p.Price3.ToString();
            plPrice6.Text = p.Price6.ToString();
            pp = p;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) { Close(); }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (plPrice1.Text.Length > 0 && plPrice3.Text.Length > 0 && plPrice6.Text.Length > 0)
            {
                string resp = App.WebFunction("save_pricelist", pp.bid.ToString(), pp.sid.ToString(), string.Format("{0},{1},{2}", plPrice1.Text, plPrice3.Text, plPrice6.Text));
                (Owner as MainWindow).InitializeObjects();
                (Owner as MainWindow).plDg.ItemsSource = null;
                (Owner as MainWindow).plDg.ItemsSource = MainWindow.dgPricelist;
                Close();
            }
            else MessageBox.Show("Има празни полета!");
        }
    }
}
