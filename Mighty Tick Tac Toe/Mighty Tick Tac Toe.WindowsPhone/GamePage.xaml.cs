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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
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
        Rectangle[,] rects = new Rectangle[9, 9];
        Border[,] borders = new Border[9, 9];

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
                    rects[i, j] = new Rectangle();
                    rects[i, j].IsTapEnabled = true;
                    rects[i, j].HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    rects[i, j].VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    rects[i, j].Fill = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
                    rects[i, j].Tapped += CellTapped;
                    GameGrid.Children.Add(rects[i, j]);
                    Grid.SetRow(rects[i, j], i);
                    Grid.SetColumn(rects[i, j], j);

                    borders[i, j] = new Border();
                    borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                    int leftThick = 1, topThick = 1, rightThick = 1, bottomThick = 1;
                    if (i % 3 == 0)
                    {
                        topThick = 4;
                        borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                    }
                    if (i == 8)
                    {
                        bottomThick = 4;
                        borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                    }
                    if (j % 3 == 0)
                    {
                        leftThick = 4;
                        borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                    }
                    if (j == 8)
                    {
                        rightThick = 4;
                        borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                    }
                    borders[i, j].BorderThickness = new Thickness(leftThick, topThick, rightThick, bottomThick);
                    GameGrid.Children.Add(borders[i, j]);
                    Grid.SetRow(borders[i, j], i);
                    Grid.SetColumn(borders[i, j], j);
                }
            }

        }

        void FillCell(int row, int col, string c, double height, double width)
        {
            c = c.ToLower();
            if (c != "x" && c != "o")
            {
                StatusText.Text = "Wrong fill character: " + c;
                return;
            }
            var img = new Image();
            string path;
            path = "ms-appx:///Assets/" + c + ".png";
            img.Source = new BitmapImage(new Uri(this.BaseUri, path));
            img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img.Height = height;
            img.Width = width;
            img.Stretch = Stretch.UniformToFill;
            img.Tapped += CellTapped;
            Canvas.SetZIndex(img, 0);

            GameGrid.Children.Add(img);
            Grid.SetRow(img, row);
            Grid.SetColumn(img, col);

            // animation
            img.RenderTransform = new ScaleTransform();
            Storyboard sb = new Storyboard();
            DoubleAnimation scalex = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            DoubleAnimation scaley = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            DoubleAnimation movex = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            DoubleAnimation movey = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            sb.Children.Add(scalex);
            sb.Children.Add(scaley);
            //sb.Children.Add(movex);
            //sb.Children.Add(movey);
            Storyboard.SetTargetProperty(scalex, "(Image.RenderTransform).(ScaleTransform.ScaleX)");
            Storyboard.SetTargetProperty(scaley, "(Image.RenderTransform).(ScaleTransform.ScaleY)");
            //Storyboard.SetTargetProperty(movex, "(Image.RenderTransform).(TranslateTransform.X)");
            //Storyboard.SetTargetProperty(movey, "(Image.RenderTransform).(TranslateTransform.Y)");
            Storyboard.SetTarget(scalex, img);
            Storyboard.SetTarget(scaley, img);
            //Storyboard.SetTarget(movex, img);
            //Storyboard.SetTarget(movey, img);
            sb.Begin();
        }

        void CellTapped(object sender, TappedRoutedEventArgs args)
        {
            int gr = Grid.GetRow(sender as Rectangle);
            int gc = Grid.GetColumn(sender as Rectangle);

            int br = gr / 3;
            int bc = gc / 3;

            int cr = gr % 3;
            int cc = gc % 3;

            int player = currentPlayer;
            var result = game.PlayMove(currentPlayer, bc, br, cc, cr);

            if (result >= MoveState.SUCCESS_GAME_ON)
            {
                currentPlayer *= -1;

                if (game.NextBoardCol == -1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            borders[i, j].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        }
                    }

                    for (int i = 3 * cc; i < 3 * (cc + 1); i++)
                    {
                        for (int j = 3 * cr; j < 3 * (cr + 1); j++)
                        {
                            borders[j, i].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                        }
                    }
                }
            }

            StatusText.Text = result.ToString();

            switch (result)
            {
                case MoveState.SUCCESS_GAME_ON:

                    FillCell(gr, gc, currentPlayer == 1 ? "X" : "O", (sender as Rectangle).Height, (sender as Rectangle).Width);
                    GameGrid.Children.Remove(sender as Rectangle);
                    //rects[gr, gc].Fill = (currentPlayer == 1) ? new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) : new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_ON:
                case MoveState.SUCCESS_BOARD_WON_GAME_DRAW:
                case MoveState.SUCCESS_BOARD_WON_GAME_WON:

                    for (int i = 3 * bc; i < 3 * (bc + 1); i++)
                    {
                        for (int j = 3 * br; j < 3 * (br + 1); j++)
                        {
                            rects[j, i].Fill = (currentPlayer == 1) ? new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) : new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                        }
                    }
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW:
                case MoveState.SUCCESS_BOARD_DRAW_GAME_ON:

                    for (int i = 3 * bc; i < 3 * (bc + 1); i++)
                    {
                        for (int j = 3 * br; j < 3 * (br + 1); j++)
                        {
                            rects[j, i].Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                        }
                    }
                    break;

            }

        }
    }
}
