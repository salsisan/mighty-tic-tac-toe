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
        int currentPlayer = 1;
        GameEngine game = new GameEngine();
        Dictionary<UIElement, List<int>> elementToCell = new Dictionary<UIElement, List<int>>();
        Dictionary<List<int>, UIElement> cellToElement = new Dictionary<List<int>, UIElement>();
        Rectangle[,] rects = new Rectangle[9, 9];
        Border[,] borders = new Border[9, 9];
        double cellHeight = 0, cellWidth = 0;
        double imgHeight = 0, imgWidth = 0;
        double yourTurnImgWidth = 110, yourTurnImgHeight = 40;
        double appMargin = 20;
        int rows = 9, cols = 9;
        double imgToCellPerc = 0.8;
        double inAnimationDurationSec = 0.4;
        EasingFunctionBase easingFunc = new BackEase();
        List<Storyboard> flashStoryboards = new List<Storyboard>();
        string xturnSrc = "Assets/xturn.png";
        string oturnSrc = "Assets/oturn.png";

        enum GameColor
        {
            BlackBackground,
            NormalCell,
            AvailableBoardGlow,
            BoardWonX,
            BoardWonO,
            BoardDraw
        }

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

        Brush GetColor(GameColor c)
        {
            switch (c)
            {
                // hard code all game colors
                case GameColor.BlackBackground:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                case GameColor.NormalCell:
                    return new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
                case GameColor.AvailableBoardGlow:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                case GameColor.BoardWonX:
                    return new SolidColorBrush(Color.FromArgb(255, 200, 255, 200));
                case GameColor.BoardWonO:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 200, 200));
                case GameColor.BoardDraw:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            }
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        void PaintGrid()
        {
            GameGrid.Width = GameGrid.Height = Window.Current.Bounds.Width - 2 * appMargin;
            cellHeight = GameGrid.Height / rows;
            cellWidth = GameGrid.Width / cols;
            imgHeight = cellHeight * imgToCellPerc;
            imgWidth = cellWidth * imgToCellPerc;

            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    rects[i, j] = new Rectangle();
                    rects[i, j].IsTapEnabled = true;
                    rects[i, j].HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    rects[i, j].VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    rects[i, j].Fill = GetColor(GameColor.NormalCell);
                    rects[i, j].Tapped += CellTapped;
                    GameGrid.Children.Add(rects[i, j]);
                    Grid.SetRow(rects[i, j], i);
                    Grid.SetColumn(rects[i, j], j);

                    borders[i, j] = new Border();
                    borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                    int leftThick = 1, topThick = 1, rightThick = 1, bottomThick = 1;
                    if (i % 3 == 0)
                    {
                        topThick = 4;
                        borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                    }
                    if (i == 8)
                    {
                        bottomThick = 4;
                        borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                    }
                    if (j % 3 == 0)
                    {
                        leftThick = 4;
                        borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                    }
                    if (j == 8)
                    {
                        rightThick = 4;
                        borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                    }
                    borders[i, j].BorderThickness = new Thickness(leftThick, topThick, rightThick, bottomThick);
                    GameGrid.Children.Add(borders[i, j]);
                    Grid.SetRow(borders[i, j], i);
                    Grid.SetColumn(borders[i, j], j);
                }
            }

        }

        void FillCell(int row, int col, string c)
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
            img.Stretch = Stretch.UniformToFill;
            img.Tapped += CellTapped;
            Canvas.SetZIndex(img, 0);

            GameGrid.Children.Add(img);
            Grid.SetRow(img, row);
            Grid.SetColumn(img, col);

            // animation
            img.RenderTransform = new CompositeTransform();
            Storyboard sb = new Storyboard();
            DoubleAnimation scalex = new DoubleAnimation()
            {
                From = 0,
                To = imgToCellPerc,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation scaley = new DoubleAnimation()
            {
                From = 0,
                To = imgToCellPerc,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movex = new DoubleAnimation()
            {
                // move from the center of the cell to its left
                From = 0.5 * imgWidth,
                To = (1 - imgToCellPerc) / 2 * cellWidth,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movey = new DoubleAnimation()
            {
                // move from the center of the cell to its top
                From = 0.5 * imgHeight,
                To = (1 - imgToCellPerc) / 2 * cellHeight,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            sb.Children.Add(scalex);
            sb.Children.Add(scaley);
            Storyboard.SetTargetProperty(scalex, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
            Storyboard.SetTargetProperty(scaley, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
            Storyboard.SetTarget(scalex, img);
            Storyboard.SetTarget(scaley, img);

            sb.Children.Add(movex);
            sb.Children.Add(movey);
            Storyboard.SetTargetProperty(movex, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(movey, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTarget(movex, img);
            Storyboard.SetTarget(movey, img);

            sb.Begin();
        }

        void CellTapped(object sender, TappedRoutedEventArgs args)
        {
            int gr = Grid.GetRow(sender as FrameworkElement);
            int gc = Grid.GetColumn(sender as FrameworkElement);

            int br = gr / 3;
            int bc = gc / 3;

            int cr = gr % 3;
            int cc = gc % 3;

            int player = currentPlayer;
            var result = game.PlayMove(currentPlayer, bc, br, cc, cr);

            if (result >= MoveState.SUCCESS_GAME_ON)
            {
                // stop any flash animations
                foreach (Storyboard sb in flashStoryboards)
                {
                    if (sb.GetCurrentState() == ClockState.Active)
                        sb.Stop();
                }

                // display turn tip for the *next* player
                AnimateYourTurn();

                if (game.NextBoardCol == -1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            borders[i, j].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                            FlashCell(rects[i, j]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            borders[i, j].BorderBrush = GetColor(GameColor.BlackBackground);
                        }
                    }

                    for (int i = 3 * cc; i < 3 * (cc + 1); i++)
                    {
                        for (int j = 3 * cr; j < 3 * (cr + 1); j++)
                        {
                            borders[j, i].BorderBrush = GetColor(GameColor.AvailableBoardGlow);
                            FlashCell(rects[j, i]);
                        }
                    }
                }
            }

            StatusText.Text = result.ToString();
            if (game.IsSuccess(result))
            {
                // put an X/O
                FillCell(gr, gc, currentPlayer == 1 ? "X" : "O");
            }

            switch (result)
            {
                case MoveState.SUCCESS_GAME_ON:

                    // nothing further for now
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_ON:
                case MoveState.SUCCESS_BOARD_WON_GAME_DRAW:
                case MoveState.SUCCESS_BOARD_WON_GAME_WON:

                    // color the won board
                    for (int i = 3 * bc; i < 3 * (bc + 1); i++)
                    {
                        for (int j = 3 * br; j < 3 * (br + 1); j++)
                        {
                            rects[j, i].Fill = (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO);
                        }
                    }
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW:
                case MoveState.SUCCESS_BOARD_DRAW_GAME_ON:

                    for (int i = 3 * bc; i < 3 * (bc + 1); i++)
                    {
                        for (int j = 3 * br; j < 3 * (br + 1); j++)
                        {
                            rects[j, i].Fill = GetColor(GameColor.BoardDraw);
                        }
                    }
                    break;

            }

            if (game.IsSuccess(result))
            {
                currentPlayer *= -1;
            }
        }

        void FlashCell(UIElement target)
        {
            target.RenderTransform = new CompositeTransform();
            DoubleAnimation opacityAnim = new DoubleAnimation();
            opacityAnim.From = 1;
            opacityAnim.To = 0.3;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(1000);
            opacityAnim.EasingFunction = new CubicEase();

            Storyboard flashStoryboard = new Storyboard();
            flashStoryboard.Duration = TimeSpan.FromMilliseconds(1000);
            flashStoryboard.Children.Add(opacityAnim);
            flashStoryboard.AutoReverse = true;
            flashStoryboard.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(opacityAnim, target);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            flashStoryboard.Begin();
            flashStoryboards.Add(flashStoryboard);
        }

        void AnimateYourTurn()
        {
            TurnImg.Source = new BitmapImage(new Uri(this.BaseUri, currentPlayer == 1 ? oturnSrc : xturnSrc));

            // animation
            TurnImg.RenderTransform = new CompositeTransform();
            Storyboard sb = new Storyboard();
            DoubleAnimation scalex = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation scaley = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movex = new DoubleAnimation()
            {
                // move from the center of the cell to its left
                From = 0.5 * yourTurnImgWidth,
                To = 0,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movey = new DoubleAnimation()
            {
                // move from the center of the cell to its top
                From = 0.5 * yourTurnImgHeight,
                To = 0,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            sb.Children.Add(scalex);
            sb.Children.Add(scaley);
            Storyboard.SetTargetProperty(scalex, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
            Storyboard.SetTargetProperty(scaley, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
            Storyboard.SetTarget(scalex, TurnImg);
            Storyboard.SetTarget(scaley, TurnImg);

            sb.Children.Add(movex);
            sb.Children.Add(movey);
            Storyboard.SetTargetProperty(movex, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(movey, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTarget(movex, TurnImg);
            Storyboard.SetTarget(movey, TurnImg);

            sb.Begin();
        }
    }
}
