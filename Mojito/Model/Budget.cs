using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Mojito.Model
{
    public class Budget
    {
        public string Name
        {
            get;
            set;
        }

        public Double Total
        {
            get;
            set;
        }

        public Double Spent
        {
            get;
            set;
        }

        public string Display
        {
            get
            {
                return String.Format("{0:c} / {1:c}", Spent, Total);
            }
        }

        public BitmapImage GraphImage
        {
            get
            {
                if (Spent <= Total)
                {
                    Uri uri = new Uri("http://chart.apis.google.com/chart?chbh=a&chs=433x15&cht=bhs&chco=6DB322&chds=0," + Total + "&chd=t:" + Spent, UriKind.Absolute);
                    return new BitmapImage(uri);
                }
                else
                {
                    Uri uri = new Uri("http://chart.apis.google.com/chart?chbh=a&chs=433x15&cht=bhs&chco=BD1A00&chds=0," + Total + "&chd=t:" + Spent, UriKind.Absolute);
                    return new BitmapImage(uri);
                }
            }
        }
    }
}
