using WalkCompanion.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;

namespace WalkCompanion
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.RateAndReview);
            appBarMenuItem.Click += new EventHandler(RateReview_Click);
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        private void RateReview_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
        }

        private void OpenEmail(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.To = "UnhandledXcept@outlook.com";
            task.Show();
        }

        private void OpenFacebook(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("https://www.facebook.com/UnhandledXcept", UriKind.Absolute);
            task.Show();
        }

        private void OpenTwitter(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("https://twitter.com/unhandledxcept", UriKind.Absolute);
            task.Show();
        }


    }
}