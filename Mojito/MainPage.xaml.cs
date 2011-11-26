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
using RestSharp;
using HtmlAgilityPack;
using Mojito.Utilities;
using System.IO.IsolatedStorage;
using Mojito.Model;
using Microsoft.Phone.Shell;

namespace Mojito
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.ToggleProgressBar();

            RestClient client = new RestClient("https://wwws.mint.com");
            client.Authenticator = new SimpleAuthenticator("username", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Username"].ToString(), "EncryptionKey"), "password", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Password"].ToString(), "EncryptionKey"));
            client.FollowRedirects = true;

            RestRequest request = new RestRequest("loginUserSubmit.xevent", Method.POST);
            request.AddParameter("task", "L");

            client.ExecuteAsync(request, client_ExecuteAsyncCompleted);
        }

        private void client_ExecuteAsyncCompleted(RestResponse response)
        {
            if (response.ResponseUri == new Uri("https://wwws.mint.com/login.event?task=L"))
            {
                MessageBox.Show("Your login information could not be verified. Please check your email address and password and try again.", "Error", MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
            }
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response.Content);

            this.RenderAccounts(doc);
            this.RenderBudgets(doc);
            this.RenderNotifications(doc);

            this.ToggleProgressBar();
        }

        private void RenderAccounts(HtmlDocument doc)
        {
            List<Account> data = new List<Account>();

            try
            {
                var nodes = from e in doc.DocumentNode.DescendantNodes()
                            where e.Name == "li" &&
                                  e.Attributes.Contains("class") == true &&
                                  e.Attributes["class"].Value.Contains("account")
                            select e;
                foreach (HtmlNode node in nodes)
                {
                    Account a = new Account();
                    a.Name = (from e in node.DescendantNodes()
                              where e.Name == "span" &&
                                    e.Attributes.Contains("class") == true &&
                                    e.Attributes["class"].Value.Contains("nickname")
                              select e).First().InnerText;
                    a.Balance = (from e in node.DescendantNodes()
                                 where e.Name == "span" &&
                                       e.Attributes.Contains("class") == true &&
                                       e.Attributes["class"].Value.Contains("balance")
                                 select e).First().InnerText;
                    a.LastUpdated = (from e in node.DescendantNodes()
                                     where e.Name == "span" &&
                                           e.Attributes.Contains("class") == true &&
                                           e.Attributes["class"].Value.Contains("last-updated")
                                     select e).First().InnerText;

                    data.Add(a);
                }

                this.accounts_listBox1.ItemsSource = data.OrderBy(i => i.Name);

                double total = 0.00;
                foreach (Account a in data)
                {
                    total += Convert.ToDouble(a.Balance.Replace("$", "").Replace("–", "-"));
                }

                this.accounts_textBlock1.Text = String.Format("{0:c}", total);

                this.UpdateLiveTile(total);
            }
            catch
            {
                this.accounts_listBox1.Visibility = System.Windows.Visibility.Collapsed;
                this.accounts_textBlock1.Visibility = System.Windows.Visibility.Collapsed;
                this.accounts_textBlock0.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RenderBudgets(HtmlDocument doc)
        {
            List<Budget> data = new List<Budget>();

            try
            {
                var nodes = from e in doc.DocumentNode.DescendantNodes()
                            where e.Name == "tr" &&
                                  e.Attributes.Contains("id") == true &&
                                  e.Attributes["id"].Value.Contains("budget")
                            select e;
                foreach (HtmlNode node in nodes)
                {
                    Budget a = new Budget();
                    a.Name = (from e in node.DescendantNodes()
                              where e.Name == "th"
                              select e).First().InnerText.Replace("&amp;", "&").ToUpper();
                    a.Total = Convert.ToDouble((from e in node.DescendantNodes()
                                                where e.Name == "td" &&
                                                      e.Attributes.Contains("class") == true &&
                                                      e.Attributes["class"].Value.Contains("budget")
                                                select e).First().InnerText.Replace("$", ""));
                    a.Spent = Convert.ToDouble((from e in node.DescendantNodes()
                                                where e.Name == "td" &&
                                                      e.Attributes.Contains("class") == true &&
                                                      e.Attributes["class"].Value.Contains("bar")
                                                select e).First().InnerText.Replace("$", ""));

                    data.Add(a);
                }

                data.RemoveAt(0);
                this.budgets_listBox1.ItemsSource = data;
            }
            catch
            {
                this.budgets_listBox1.Visibility = System.Windows.Visibility.Collapsed;
                this.budgets_textBlock0.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RenderNotifications(HtmlDocument doc)
        {
            List<Notification> data = new List<Notification>();

            try
            {
                var nodes = from e in doc.DocumentNode.DescendantNodes()
                            where e.Name == "li" &&
                                  e.Attributes.Contains("class") == true &&
                                  e.Attributes["class"].Value.Contains("short")
                            select e;
                foreach (HtmlNode node in nodes)
                {
                    Notification a = new Notification();
                    a.Description = (from e in node.DescendantNodes()
                                     where e.Name == "span" &&
                                           e.Attributes.Contains("class") == true &&
                                           e.Attributes["class"].Value.Contains("headline")
                                     select e).First().InnerText;
                    a.Date = (from e in node.DescendantNodes()
                              where e.Name == "span" &&
                                    e.Attributes.Contains("class") == true &&
                                    e.Attributes["class"].Value.Contains("date")
                              select e).First().InnerText.ToUpper();

                    data.Add(a);
                }

                this.notifications_listBox1.ItemsSource = data.OrderByDescending(i => i.Date);
            }
            catch
            {
                this.notifications_listBox1.Visibility = System.Windows.Visibility.Collapsed;
                this.notifications_textBlock0.Visibility = System.Windows.Visibility.Visible;
            }           
        }

        private void UpdateLiveTile(double total)
        {
            ShellTile tile = ShellTile.ActiveTiles.First();

            StandardTileData tileData = new StandardTileData()
            {
                BackTitle = "Net Worth",
                BackContent = String.Format("{0:c}", total),
                BackBackgroundImage = new Uri("BackTileBackground.png", UriKind.Relative)
            };

            ShellTile.ActiveTiles.First().Update(tileData);
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

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.ToggleProgressBar();

            this.accounts_listBox1.Visibility = System.Windows.Visibility.Visible;
            this.accounts_textBlock1.Visibility = System.Windows.Visibility.Visible;
            this.accounts_textBlock0.Visibility = System.Windows.Visibility.Collapsed;

            this.budgets_listBox1.Visibility = System.Windows.Visibility.Visible;
            this.budgets_textBlock0.Visibility = System.Windows.Visibility.Collapsed;

            this.notifications_listBox1.Visibility = System.Windows.Visibility.Visible;
            this.notifications_textBlock0.Visibility = System.Windows.Visibility.Collapsed;

            RestClient client = new RestClient("https://wwws.mint.com");
            client.Authenticator = new SimpleAuthenticator("username", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Username"].ToString(), "EncryptionKey"), "password", Security.Decrypt(IsolatedStorageSettings.ApplicationSettings["Password"].ToString(), "EncryptionKey"));
            client.FollowRedirects = true;

            RestRequest request = new RestRequest("loginUserSubmit.xevent", Method.POST);
            request.AddParameter("task", "L");

            client.ExecuteAsync(request, client_ExecuteAsyncCompleted);
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove("Username");
            IsolatedStorageSettings.ApplicationSettings.Remove("Password");

            NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
        }
    }
}