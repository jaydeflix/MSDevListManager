using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Net.Http;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Tweetinvi;
using Tweetinvi.Models;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MSDevListManager
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
       

        string ConsumerKey = "BDPQEoOINIH6LOrZBoMlrjslZ";
        string ConsumerSecret = "JDQdEExH8p72bNYHsA5G4mR0ZSALcWqJHxsO76XrlkKRgTXAmg";
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        Regex validHandle = new Regex(@"\b(@|)([A-Za-z]+[A-Za-z0-9_]+)");

        public MainPage()
        {

            this.InitializeComponent();

            StringCollection handleList = new StringCollection();


            if (!(localSettings.Values.ContainsKey("handlesCS")))
            {
                handleList.Add("Please add an account.");
                handleCB.ItemsSource = handleList;
                handleCB.SelectedIndex = 0;
            }
            else
            {
                if (handleList.Contains("Please add an account."))
                    handleList.Remove("Please add an account.");
                string hCS = localSettings.Values["handlesCS"].ToString();
                string[] hAR = hCS.Split(',');
                foreach (string hSIN in hAR)
                {
                    handleList.Add(hSIN);
                }
                handleCB.ItemsSource = handleList;
                handleCB.SelectedIndex = 0;
            }


            
        }

        public async void twitterCheck(object sender, RoutedEventArgs e)
        {
            Button origin = (Button)sender;
            string urlAuth = (String)origin.Tag;

            var uri = new Uri(urlAuth);
            Windows.System.Launcher.LaunchUriAsync(uri);
            return;
        }

            //var authCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);
        
        public void findAccount_Click(object sender, RoutedEventArgs e)
        {   //verify handle matches regex
            if (!(validHandle.IsMatch(handleInput.Text)))
            { FlyoutBase.ShowAttachedFlyout((FrameworkElement)findAccount); }
            else
            {
                try
                { //verify handle exists
                    var webHandler = new HttpClientHandler();
                    var webClient = new HttpClient(webHandler);
                    String url = @"http://twitter.com/" + handleInput.Text;
                    var pageExist = webClient.GetAsync(new Uri(url));
                    HttpResponseMessage handleCheck = pageExist.Result;
                    int statusCode = (int)handleCheck.StatusCode;

                if (statusCode.ToString() == "404") //handle doesn't exist
                    {
                        FlyoutBase.ShowAttachedFlyout((FrameworkElement)findAccount);
                        return;
                    }
                else
                    {
                        var btn = sender as Button;

                        var appCredentials = new TwitterCredentials("ConsumerKey", "ConsumerSecret");
                        var authenticationContext = AuthFlow.InitAuthentication(appCredentials);
                        string urlAuth = authenticationContext.AuthorizationURL;

                        ContentDialog dialog = new ContentDialog()
                        {
                            Title = "Twitter Authentication",
                            //RequestedTheme = ElementTheme.Dark,
                            //FullSizeDesired = true,
                            MaxWidth = this.ActualWidth // Required for Mobile!
                        };

                        var panel = new StackPanel();

                        panel.Children.Add(new TextBlock
                        {
                            Text = "Click the button below to launch your browser to complete the Twitter authentication and receive a PIN number.",
                            TextWrapping = TextWrapping.Wrap,
                        });

                        Button launchTwitter = new Button();
                        launchTwitter.Content = "Launch Twitter";
                        launchTwitter.Click += twitterCheck;
                        launchTwitter.Tag = urlAuth;
                        panel.Children.Add(launchTwitter);


                        TextBox twtPin = new TextBox();
                        twtPin.Text = "Insert Pin Here";
                        panel.Children.Add(twtPin);
                      
                        dialog.Content = panel;

                        
                        dialog.PrimaryButtonText = "Verify Pin";
                        dialog.CloseButtonText = "Cancel";

                        var result = dialog.ShowAsync();
                        
                        if (result.GetResults() == ContentDialogResult.Primary)
                        {
                            var userCredentials = 
                                AuthFlow.CreateCredentialsFromVerifierCode(twtPin.Text, authenticationContext);
                            Auth.SetCredentials(userCredentials);
                            Windows.Storage.ApplicationDataCompositeValue composite =
                            new Windows.Storage.ApplicationDataCompositeValue();
                            composite["userKey"] = userCredentials.ConsumerKey;
                            composite["userSecret"] = userCredentials.ConsumerSecret;

                            localSettings.Values[handleInput.Text] = composite;
                        }

                    }
                }
                catch
                {
                    // Details in ex.Message and ex.HResult.       
                }
                

            }
        }

        private void handleKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                findAccount_Click(sender,e);
            }
        }
    }
}
