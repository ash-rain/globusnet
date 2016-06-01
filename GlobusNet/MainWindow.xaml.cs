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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlobusNet
{
    public partial class MainWindow : Window
    {
        public static List<GService> Svc = new List<GService>();
        public static List<GBranch> Branches = new List<GBranch>();
        public static List<DGPricelist> dgPricelist = new List<DGPricelist>();
        public bool IsAdmin { get { return (App.UserRole > 1); } }
        public bool IsACL = false;
        GUser SUser { get { return (dgUsers.SelectedItem as GUser); } }
        public MainWindow()
        {
            InitializeObjects();
            InitializeComponent();
            Rebind();

            System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            dt.Tick += delegate(object s, EventArgs e) { if (dgUsers.SelectedIndex < 0 && dgUsers.Items.Count > 0) dgUsers.SelectedIndex = 0; };
            dt.Start();

            ReloadUsers();
        }
        public void Rebind()
        {
            List<int> constrainment = new List<int>();

            dgUsers.SelectionChanged += delegate(object s, SelectionChangedEventArgs e) { if (dgUsers.SelectedIndex >= 0) { dgUsersSI = dgUsers.SelectedIndex; ReloadInfo(); } sidePane.Visibility = (dgUsers.SelectedIndex < 0 ? Visibility.Collapsed : Visibility.Visible); };
            adACLcb.SelectionChanged += delegate(object s, SelectionChangedEventArgs e) { AdminGetACL(); };
            tabAdmin.Visibility = (IsAdmin ? Visibility.Visible : Visibility.Hidden);
            ueAddress.IsReadOnly = !IsAdmin;
            ueFullname.IsReadOnly = !IsAdmin;
            ueIPAddress.IsReadOnly = !IsAdmin;
            ueName.IsReadOnly = !IsAdmin;
            uePhone.IsReadOnly = !IsAdmin;
            pwNew.Visibility = (IsAdmin ? Visibility.Visible : Visibility.Collapsed);
            pwTb.Visibility = pwNew.Visibility;
            cbBranch.IsEnabled = IsAdmin;
            ueSave.Visibility = (IsAdmin ? Visibility.Visible : Visibility.Hidden);
            plEdit.Visibility = (IsAdmin ? Visibility.Visible : Visibility.Hidden);
            if (!IsAdmin)
            {
                plGrid.Margin = new Thickness(0);
                plAddPane.Visibility = Visibility.Collapsed;
                string wr = App.WebFunction("get_constrainments", App.UserID.ToString());
                if (wr.Length > 0) foreach (string[] s in App.TransferString(wr)) constrainment.Add(int.Parse(s[0]));
            }
            tbLoginName.Text = App.WebFunction("get_app_user_name", App.UserID.ToString());

            cbBranch.ItemsSource = Branches;
            plaNBranch.ItemsSource = Branches;
            plaNService.ItemsSource = Svc;
            List<string> br = new List<string>() { "Всички клонове" };
            if (constrainment.Count == 0) foreach (GBranch b in Branches) br.Add(b.Name);
            else
            {
                IsACL = true;
                br.Clear();
                foreach (GBranch b in Branches) if (constrainment.Contains(b.Id)) br.Add(b.Name);
            }
            List<string> au = new List<string>();
            foreach (string[] s in App.TransferString(App.WebFunction("get_cashiers", "fullname"))) au.Add(s[0]);
            adACLcb.ItemsSource = au;
            AdminGetACL();

            List<string> sv = new List<string>() { "Всички услуги" };
            foreach (GService s in Svc) sv.Add(s.Name);
            dfBR.ItemsSource = br;
            plfBranches.ItemsSource = br;
            plfServices.ItemsSource = sv;
            plDg.ItemsSource = dgPricelist;

            adaucp.Items.Clear();
            foreach (string[] apusr in App.TransferString(App.WebFunction("get_app_users", "id,name"))) adaucp.Items.Add(new ComboBoxItem() { Content = apusr[1], Tag = apusr[0] });

            if (IsACL)
            {
                dgUsers.Items.Filter = dgFilter;
                plDg.Items.Filter = plFilter;
            }
        }
        private void AdminGetACL()
        {
            List<string> r = App.TransferRow(App.WebFunction("get_acl", adACLcb.SelectedValue.ToString()));
            List<DGacl> ac = new List<DGacl>();
            foreach (GBranch b in Branches)
                ac.Add(new DGacl() { c = (r.Contains(b.Id.ToString())), Branch = b.Name, Bid = b.Id });
            adACLdg.ItemsSource = ac;
        }

        int dgUsersSI = 0;
        public void ReloadUsers()
        {
            List<GUser> u = new List<GUser>();
            foreach (string[] s in App.TransferString(App.WebFunction("get_users", "*")))
                u.Add(new GUser(int.Parse(s[0])) { Name = s[1], Fullname = s[2], Phone = s[4], Branch = (from b in Branches where b.Id == int.Parse(s[5]) select b).Single<GBranch>(), Address = s[6], IPAddress = s[7], Status = Convert.ToBoolean(int.Parse(s[8])), StatRequest = int.Parse(s[9]) });
            dgUsers.ItemsSource = u;
            dgUsers.SelectedIndex = dgUsersSI;
        }
        public void ReloadInfo()
        {
            SidePanel.DataContext = SUser;
            dgPay.SelectedIndex = 0;
            pwNew.Password = string.Empty;
            btnFUser.Content = (IsAdmin ? (SUser.Status ? "Изключване" : "Включване") : (SUser.Status ? "Заявка за изключване" : "Заявка за включване"));
            tbRequest.Inlines.Clear();
            tbRequest.Inlines.Add(new Run((SUser.StatRequest > 0 ? (SUser.StatRequest == 2 ? "Отказ на заявено включване" : "Отказ на заявено изключване") : string.Empty)));
            btnFUser.IsEnabled = true;
            if (!IsAdmin && SUser.StatRequest > 0) btnFUser.IsEnabled = false;
        }
        public void InitializeObjects()
        {
            Svc.Clear();
            foreach (string[] s in App.TransferString(App.WebFunction("get_services")))
                Svc.Add(new GService() { Id = int.Parse(s[0]), Name = s[1] });
            Branches.Clear();
            dgPricelist.Clear();
            foreach (string[] s in App.TransferString(App.WebFunction("get_branches")))
            {
                List<GPricelist> pl = new List<GPricelist>();
                foreach (string[] S in App.TransferString(App.WebFunction("get_pricelist", s[2])))
                {
                    GService ggs = Helper.GetServiceById(int.Parse(S[1]));
                    float ggp = float.Parse(S[3]);
                    int gsp = int.Parse(S[2]);
                    GPricelist gpl = new GPricelist() { Id = int.Parse(S[0]), Service = ggs, Span = gsp, Price = ggp };
                    int c = (from pe in dgPricelist where pe.Branch == s[1] && pe.Service == ggs.Name select pe).Count<DGPricelist>();
                    switch(c)
                    {
                        case 0:
                            dgPricelist.Add(new DGPricelist() { Branch = s[1], bid = int.Parse(s[0]), sid = ggs.Id, Service = ggs.Name, Price1 = ggp });
                            break;
                        case 1:
                            if (gsp == 3) dgPricelist.Last<DGPricelist>().Price3 = ggp;
                            else dgPricelist.Last<DGPricelist>().Price6 = ggp;
                            break;
                    }
                    pl.Add(gpl);
                }
                Branches.Add(new GBranch() { Id = int.Parse(s[0]), Name = s[1], Pricelist = pl });
            }
        }

        private void Hl_ANew_Click(object sender, RoutedEventArgs e)
        {
            new UserWindow() { Owner = this }.ShowDialog();
        }
        private void Hl_ADel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Това действие е необратимо! Сигурни ли сте?", "Изтриване на потребител", MessageBoxButton.YesNo))
            {
                App.WebFunction("del_user", SUser.Id.ToString());
                ReloadUsers();
                MessageBox.Show("Потребителят е изтрит.");
            }
        }
        private void Hl_Save_Click(object sender, RoutedEventArgs e)
        {
            string uid = SUser.Id.ToString();
            App.WebFunction("edit_user", uid, "name", ueName.Text);
            App.WebFunction("edit_user", uid, "fullname", ueFullname.Text);
            App.WebFunction("edit_user", uid, "phone", uePhone.Text);
            App.WebFunction("edit_user", uid, "address", ueAddress.Text);
            App.WebFunction("edit_user", uid, "ipaddr", ueIPAddress.Text);
            App.WebFunction("edit_user", uid, "branchid", (cbBranch.SelectedItem as GBranch).Id.ToString());
            if (pwNew.Password.Length > 0 && MessageBox.Show("Това действие ще промени текущата парола на потребителя. Сигурни ли сте?", "Промяна на парола", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                App.WebFunction("change_pass", uid, pwNew.Password);
            ReloadUsers();
            MessageBox.Show("Промените бяха успешно запазени");
        }
        private void Hl_Add_Click(object sender, RoutedEventArgs e)
        {
            if (SUser.Subscription != null)
                new PaymentWindow(SUser, null) { Owner = this }.ShowDialog();
            else MessageBox.Show("Този потребител не е абониран за услуги! Не може да добавите плащане.");
        }
        private void Hl_Del_Click(object sender, RoutedEventArgs e)
        {
            if (dgPay.SelectedIndex >= 0 && MessageBox.Show("Сигурни ли сте, че желаете да изтриете този запис?", "Изтриване на запис", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string r = App.WebFunction("payment_delete", (dgPay.SelectedItem as GPayment).Id.ToString());
                ReloadUsers();
            }
        }
        private void Hl_Pro_Click(object sender, RoutedEventArgs e)
        {
            if (dgPay.SelectedIndex >= 0)
                new PaymentWindow(SUser, (dgPay.SelectedItem as GPayment)) { Owner = this }.ShowDialog();
        }
        private void Hl_DelRequest_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("edit_user", SUser.Id.ToString(), "sreq", "0");
            ReloadUsers();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e) { dgUsers.Items.Filter = dgFilter; }
        private void dfCB_SelectionChanged(object sender, SelectionChangedEventArgs e) { dgUsers.Items.Filter = dgFilter; }
        bool dgFilter(object i)
        {
            bool fText = true;
            bool fBond = true;
            bool fBran = true;
            string f = dfTB.Text.ToLower();
            if (f.Length > 0) fText = (i as GUser).Fullname.ToLower().Contains(f) || (i as GUser).Address.ToLower().Contains(f);
            if (dfCB.SelectedIndex > 0) fBond = (dfCB.SelectedIndex == 1 ? ((i as GUser).DaysBalance < 0) : ((i as GUser).DaysBalance >= 0));
            if (dfBR.SelectedIndex > 0 || IsACL) fBran = ((i as GUser).Branch.Name.Equals(dfBR.SelectedItem.ToString()));
            return (fText && fBond && fBran);
        }

        private void Hl_Logout_Click(object sender, RoutedEventArgs e)
        {
            App.Restart();
        }

        private void btnFUser_Click(object sender, RoutedEventArgs e)
        {
            string f = (IsAdmin ? "stat" : "sreq");
            string v = (IsAdmin ? (SUser.Status ? "0" : "1") : (SUser.Status ? "1" : "2"));
            App.WebFunction("edit_user", SUser.Id.ToString(), f, v);
            if (IsAdmin) App.WebFunction("edit_user", SUser.Id.ToString(), "sreq", "0");
            ReloadUsers();
        }

        string rrq = "За да влязат промените в сила е необходимо рестартиране на системата. Рестарт сега?";
        private void plaBranch_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("add_branch", plaBranch.Text);
            plaBranch.Text = string.Empty;
            if (MessageBox.Show(rrq, "Добавяне на клон", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                App.Restart();
        }
        private void plaPrice_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("add_service", plaService.Text);
            plaService.Text = string.Empty;
            if (MessageBox.Show(rrq, "Добавяне на услуга", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                App.Restart();
        }
        private void plaService_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("add_price", (plaNService.SelectedItem as GService).Id.ToString(), (plaNBranch.SelectedItem as GBranch).Id.ToString(), string.Format("{0},{1},{2}", plaN1.Text, plaN2.Text, plaN3.Text));
            plaNBranch.SelectedIndex = -1;
            plaNService.SelectedIndex = -1;
            plaN1.Text = "0";
            plaN2.Text = "0";
            plaN3.Text = "0";
            if (MessageBox.Show(rrq, "Добавяне на услуга", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                App.Restart();
        }
        private void plf_SelectionChanged(object sender, SelectionChangedEventArgs e) { plDg.Items.Filter = plFilter; }
        bool plFilter(object i)
        {
            bool fSvc = true;
            bool fBra = true;
            if (plfServices.SelectedIndex > 0) fSvc = (i as DGPricelist).Service.Equals(plfServices.SelectedItem.ToString());
            if (plfBranches.SelectedIndex > 0 || IsACL) fBra = (i as DGPricelist).Branch.Equals(plfBranches.SelectedItem.ToString());
            return (fSvc && fBra);
        }
        private void btnACL_Click(object sender, RoutedEventArgs e)
        {
            string s = string.Empty;
            foreach (DGacl a in adACLdg.Items) if (a.c) s += a.Bid.ToString() + ",";
            if(s.Length > 0) s = s.Substring(0, s.Length - 1);
            App.WebFunction("set_acl", adACLcb.SelectedValue.ToString(), s);
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("set_var", "adminmail", ademail.Text);
            App.WebFunction("set_var", "mail-on-dis-req", cbiostr(adcb0));
            App.WebFunction("set_var", "mail-on-con-req", cbiostr(adcb1));
            App.WebFunction("set_var", "mail-on-con", cbiostr(adcb2));
            App.WebFunction("set_var", "mail-on-dis", cbiostr(adcb3));
            App.WebFunction("set_var", "mail-on-pay", cbiostr(adcb4));
            App.WebFunction("set_var", "mail-on-pay-del", cbiostr(adcb5));
            App.WebFunction("set_var", "mail-on-new-usr", cbiostr(adcb6));
        }
        string cbiostr(CheckBox cb) { return ((bool)cb.IsChecked ? "1" : "0"); }
        bool cbiostr(string vl) { return vl.Equals("1"); }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as TabControl).SelectedIndex == 3)
            {
                adaucp.SelectedIndex = 0;
                ademail.Text = App.WebFunction("get_var", "adminmail");
                adcb0.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-dis-req"));
                adcb1.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-con-req"));
                adcb2.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-con"));
                adcb3.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-dis"));
                adcb4.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-pay"));
                adcb5.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-pay-del"));
                adcb6.IsChecked = cbiostr(App.WebFunction("get_var", "mail-on-new-usr"));
            }
        }
        private void plEdit_Click(object sender, RoutedEventArgs e)
        {
            new PricelistWindow((plDg.SelectedItem as DGPricelist)) { Owner = this }.ShowDialog();
        }
        private void plDg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) { plEdit.IsEnabled = ((sender as DataGrid).SelectedIndex >= 0); }

        private void btnChangeAUPass_Click(object sender, RoutedEventArgs e)
        {
            App.WebFunction("set_app_user_pass", (adaucp.SelectedItem as ComboBoxItem).Tag.ToString(), adaunp.Text);
            MessageBox.Show(string.Format("Новата парола на {0} е \"{1}\"", (adaucp.SelectedItem as ComboBoxItem).Content, adaunp.Text));
            adaunp.Text = "";
        }
    }
    public class DGacl
    {
        public bool c { get; set; }
        public string Branch { get; set; }
        public int Bid { get; set; }
    }
    public class DGPricelist
    {
        public string Branch { get; set; }
        public string Service { get; set; }
        public float Price1 { get; set; }
        public float Price3 { get; set; }
        public float Price6 { get; set; }
        public int bid { get; set; }
        public int sid { get; set; }
    }
    public class GUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Fullname { get; set; }
        public GSubscription Subscription { get; set; }
        public List<GPayment> Payments { get; set; }
        public string Phone { get; set; }
        public GBranch Branch { get; set; }
        public string Address { get; set; }
        public string IPAddress { get; set; }
        public bool Status { get; set; }
        public int StatRequest { get; set; }
        public float Bond
        {
            get
            {
                float p = 0f;
                //foreach(GSubscription gs in Subscriptions)
                //p += (gs.MonthsActive * gs.Service.);
                return p;
            }
        }
        public float PaymentSum
        {
            get
            {
                return Payments.Sum(i => i.Estimate);
            }
        }
        public double DaysBalance
        {
            get
            {
                if (Payments.Count == 0) return 0;
                DateTime paitTill = Payments.Last().Paid.AddMonths(Payments.Last().Span);
                return (double)(int)paitTill.Subtract(DateTime.Now).TotalDays;
            }
        }
        public SolidColorBrush DaysBalanceColor
        {
            get { return new SolidColorBrush((DaysBalance < 0 ? Colors.Red : (DaysBalance == 0 ? Colors.White : Colors.Black))); }
        }
        public string StatusImage { get { return string.Format("/stat{0}.png", (DaysBalance < 0 ? 0 : 1)); } }
        public string OnlineStatusImage { get { return string.Format("/ostat{0}.png", (Status ? 1 : 0)); } }
        public string StarImage { get { return string.Format("/star{0}.png", StatRequest); } }

        private int PaidMonths()
        {
            return Payments.Sum(i => i.Span);
        }

        public GUser(int id)
        {
            Id = id;
            //Subscriptions = new List<GSubscription>();
            string r = App.WebFunction("get_subscriptions", Id.ToString());
            if (r.Length > 0)
                foreach (string[] s in App.TransferString(r))
                {
                    GSubscription gs = new GSubscription() { Id = int.Parse(s[0]), Service = Helper.GetServiceById(int.Parse(s[1])), Started = DateTime.Parse(s[2]), Ended = null };
                    if (s[3].Length > 0) gs.Ended = DateTime.Parse(s[3]);
                    gs.MonthsActive = (int)Helper.MonthsBetween(gs.Started, gs.Ended);
                    Subscription = gs;
                    break; // get first subscription only
                }
            Payments = new List<GPayment>();
            r = App.WebFunction("get_payments", Id.ToString());
            if (r.Length > 0)
                foreach (string[] s in App.TransferString(r))
                    Payments.Add(new GPayment() { Id = int.Parse(s[0]), Service = Helper.GetServiceById(int.Parse(s[1])), Span = int.Parse(s[2]), Paid = DateTime.Parse(s[3]), Till = DateTime.Parse(s[4]), Estimate = float.Parse(s[5]), Cashier = int.Parse(s[6]) });
        }
    }
    public class GPayment
    {
        public int Id { get; set; }
        public GService Service { get; set; }
        public int Span { get; set; }
        public DateTime Paid { get; set; }
        public DateTime Till { get; set; }
        public float Estimate { get; set; }
        public int Cashier { get; set; }
    }
    public class GSubscription
    {
        public int Id { get; set; }
        public GService Service { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Ended { get; set; }
        public int MonthsActive { get; set; }
    }
    public class GService
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class GBranch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GPricelist> Pricelist { get; set; }
    }
    public class GPricelist
    {
        public int Id { get; set; }
        public GService Service { get; set; }
        public int Span { get; set; }
        public float Price { get; set; }
    }
}
