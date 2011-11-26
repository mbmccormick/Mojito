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

namespace Mojito.Model
{
    public class Account
    {
        public string Name
        {
            get;
            set;
        }

        public string Balance
        {
            get;
            set;
        }

        public string LastUpdated
        {
            get;
            set;
        }
    }
}
