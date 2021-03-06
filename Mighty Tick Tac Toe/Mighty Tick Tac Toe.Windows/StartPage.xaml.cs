﻿using Mighty_Tick_Tac_Toe.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Mighty_Tick_Tac_Toe
{
    class NavigationParams
    {
        public GameMode mode;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        int wins = 0, losses = 0, draws = 0, winStreak = 0, longestWinStreak = 0;

        public StartPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            UpdateStatsTextBoxes();
        }

        private void UpdateStatsTextBoxes()
        {
            GameEngine.GetStats(ref wins, ref losses, ref draws, ref winStreak, ref longestWinStreak);
            StatsWinsTxt.Text = wins.ToString();
            StatsLossesTxt.Text = losses.ToString();
            StatsDrawsTxt.Text = draws.ToString();
            StatsWinStreakTxt.Text = winStreak.ToString();
            StatsLongestWinStreakTxt.Text = longestWinStreak.ToString();

            if (GameEngine.GetStatsPanelVisibility())
            {
                StatsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StatsBtn.Content = "Hide Game Stats";
            }
            else
            {
                StatsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                StatsBtn.Content = "Show Game Stats";
            }
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void singlePlayerBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationParams parameters = new NavigationParams();
            parameters.mode = GameMode.AI_LVL_3;
            this.Frame.Navigate(typeof(GamePage), parameters);
        }

        private void MultiplayerBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationParams parameters = new NavigationParams();
            parameters.mode = GameMode.TwoPlayer;
            this.Frame.Navigate(typeof(GamePage), parameters);
        }

        private void GameRulesBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GameRulesPage));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage));
        }

        private void StatsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StatsPanel.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                StatsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StatsBtn.Content = "Hide Game Stats";
            }
            else
            {
                StatsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                StatsBtn.Content = "Show Game Stats";
            }
            GameEngine.SetStatsPanelVisibility(StatsPanel.Visibility == Windows.UI.Xaml.Visibility.Visible);
        }

        private void ResetStatsBtn_Click(object sender, RoutedEventArgs e)
        {
            GameEngine.ResetStats();
            UpdateStatsTextBoxes();
        }
    
    }
}
