using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Mighty_Tick_Tac_Toe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("zune:reviewapp?appid=appb4780074-fe48-47b8-abfd-31d7e52b7456"));
            // "zune:reviewapp"
            // "zune:reviewapp?appid=appA1E4E8C3-C0AD-44FC-B070-C54F379D9450"
            // "ms-windows-store:REVIEW?PFN=A1E4E8C3-C0AD-44FC-B070-C54F379D9450_75cr2b68sm664"

        }

        private void HomeIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                //Indicate the back button press is handled so the app does not exit
                e.Handled = true;
                Frame.Navigate(typeof(StartPage));
            }
        }
    }
}
