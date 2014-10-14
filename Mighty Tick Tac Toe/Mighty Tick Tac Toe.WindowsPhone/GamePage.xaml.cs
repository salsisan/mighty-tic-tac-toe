using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Mighty_Tick_Tac_Toe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        GameEngine game = new GameEngine();
        Dictionary<UIElement, List<int>> elementToCell = new Dictionary<UIElement, List<int>>();
        Dictionary<List<int>, UIElement> cellToElement = new Dictionary<List<int>, UIElement>();

        int currentPlayer = 1;

        public GamePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PaintGrid();
        }

        void PaintGrid()
        {
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.IsTapEnabled = true;
                    rectangle.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    rectangle.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    rectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
                    rectangle.Tapped += CellTapped;
                    GameGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);

                    Border b = new Border();
                    b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    int leftThick = 1, topThick = 1, rightThick = 1, bottomThick = 1;
                    if (i % 3 == 0)
                    {
                        topThick = 4;
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    if (i == 8)
                    {
                        bottomThick = 4;
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    if (j % 3 == 0)
                    {
                        leftThick = 4;
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    if (j == 8)
                    {
                        rightThick = 4;
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    b.BorderThickness = new Thickness(leftThick, topThick, rightThick, bottomThick);
                    GameGrid.Children.Add(b);
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                }
            }

        }

        void CellTapped(object sender, TappedRoutedEventArgs args)
        {
            int br = Grid.GetRow(sender as Rectangle) / 3;
            int bc = Grid.GetColumn(sender as Rectangle) / 3;

            int cr = Grid.GetRow(sender as Rectangle) % 3;
            int cc = Grid.GetColumn(sender as Rectangle) % 3;

            int player = currentPlayer;
            var result = game.PlayMove(currentPlayer, bc, br, cc, cr);

            if (game.IsSuccess(result))
            {
                (sender as Rectangle).Fill = player == 1 ? new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) : new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                currentPlayer *= -1;
                // highlight the next board
            }

            StatusText.Text = "Error: " + result.ToString();
        }
    }
}
