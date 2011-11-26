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
using Mojito.Model;
using RestSharp;
using HtmlAgilityPack;
using System.IO.IsolatedStorage;
using Mojito.Utilities;

namespace Mojito
{
    public partial class OldMainPage : PhoneApplicationPage
    {
        // Constructor
        public OldMainPage()
        {
            InitializeComponent();

            this.ToggleProgressBar();
            
            RestClient client = new RestClient("https://wwws.mint.com");
            client.Authenticator = new SimpleAuthenticator("username", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Username"].ToString(), "EncryptionKey"),
                                                           "password", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Password"].ToString(), "EncryptionKey"));
            client.FollowRedirects = true;

            RestRequest request = new RestRequest("loginUserSubmit.xevent", Method.POST);
            request.AddParameter("task", "L");

            client.ExecuteAsync(request, client_ExecuteAsyncCompleted);
        }

        private void client_ExecuteAsyncCompleted(RestResponse response)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response.Content);

            List<Account> data = new List<Account>();
            
            var nodes = from e in doc.DocumentNode.DescendantNodes()
                        where e.Name == "li" &&
                              e.Attributes.Contains("class") == true &&
                              e.Attributes["class"].Value == "account"
                        select e;
            foreach (HtmlNode node in nodes)
            {
                Account a = new Account();
                a.Name = (from e in node.DescendantNodes()
                          where e.Name == "span" &&
                                e.Attributes.Contains("class") == true &&
                                e.Attributes["class"].Value == "nickname"
                          select e).First().InnerText;
                a.Balance = (from e in node.DescendantNodes()
                             where e.Name == "span" &&
                                   e.Attributes.Contains("class") == true &&
                                   e.Attributes["class"].Value == "balance"
                             select e).First().InnerText;
                a.LastUpdated = (from e in node.DescendantNodes()
                                 where e.Name == "span" &&
                                       e.Attributes.Contains("class") == true &&
                                       e.Attributes["class"].Value == "last-updated"
                                 select e).First().InnerText;

                data.Add(a);
            }

            this.listBox1.ItemsSource = data.OrderBy(i => i.Name);

            double total = 0.00;
            foreach (Account a in data)
            {
                total += Convert.ToDouble(a.Balance.Replace("$", "").Replace("–", "-"));
            }

            this.textBlock1.Text = String.Format("{0:c}", total);

            this.ToggleProgressBar();
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