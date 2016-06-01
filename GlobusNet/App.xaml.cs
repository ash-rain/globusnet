using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Net;
using System.IO;

namespace GlobusNet
{
    public partial class App : Application
    {
        public static int UserID;
        public static int UserRole;

        public static string WebFunction(string a) { return WebFunction(a, string.Empty, string.Empty, string.Empty, string.Empty); }
        public static string WebFunction(string a, string p0) { return WebFunction(a, p0, string.Empty, string.Empty, string.Empty); }
        public static string WebFunction(string a, string p0, string p1) { return WebFunction(a, p0, p1, string.Empty, string.Empty); }
        public static string WebFunction(string a, string p0, string p1, string p2) { return WebFunction(a, p0, p1, p2, string.Empty); }
        public static string WebFunction(string a, string p0, string p1, string p2, string p3)
        {
            string uri = string.Format("http://85.196.158.219/globus/ash.php?do={0}&d0={1}&d1={2}&d2={3}&d3={4}", a, p0, p1, p2, p3);
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
            StreamReader st = new StreamReader(((HttpWebResponse)myRequest.GetResponse()).GetResponseStream());
            return st.ReadToEnd();
        }
        public static List<string> TransferRow(string r)
        {
            return r.Split('\\').ToList();
        }
        public static List<string[]> TransferString(string r)
        {
            List<string[]> l = new List<string[]>();
            foreach (string s in r.Split('\\'))
                l.Add(s.Split('~'));
            return l;
        }
        public static void Restart()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
