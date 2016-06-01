using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobusNet
{
    public class Helper
    {
        public static double MonthsBetween(DateTime d1, DateTime? d2)
        {
            DateTime e = (d2.Equals(null) ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15) : (DateTime)d2);
            if (d1 > e) return 0;
            DateTime d = DateTime.MinValue + e.Subtract(d1);
            return ((d.Year - 1) * 12) + (d.Month - 1);
        }
        public static GService GetServiceById(int sid)
        {
            return (from S in MainWindow.Svc where S.Id == sid select S).Single<GService>();
        }
        public static void Dummy() { }
    }
}
