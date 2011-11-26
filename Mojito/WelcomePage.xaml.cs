using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Mojito.Utilities;
using System.IO.IsolatedStorage;

namespace Mojito
{
    public partial class WelcomePage : PhoneApplicationPage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleProgressBar();
            
            IsolatedStorageSettings.ApplicationSettings["Username"] = Security.Encrypt(this.textBox1.Text, App.EncryptionKey);
            IsolatedStorageSettings.ApplicationSettings["Password"] = Security.Encrypt(this.textBox2.Password, App.EncryptionKey);

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));

            this.ToggleProgressBar();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "";
            this.textBox2.Password = "";
        }

        private void ToggleProgressBar()
        {
            if (this.progressBar1.Visibility == System.Windows.Visibility.Collapsed)
            {
                this.progressBar1.IsIndeterminate = true;
                this.progressBar1.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.progressBar1.Visibility = System.Windows.Visibility.Collapsed;
                this.progressBar1.IsIndeterminate = false;
            }
        }
    }
}