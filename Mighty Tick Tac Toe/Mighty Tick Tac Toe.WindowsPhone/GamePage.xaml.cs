using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    enum GameMode
    {
        TwoPlayer,
        AI_LVL_1,
        AI_LVL_2,
        AI_LVL_3
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        int currentPlayer = 1;
        int lastMoveRow = -1, lastMoveCol = -1;
        public GameEngine game = new GameEngine();
        Dictionary<UIElement, List<int>> elementToCell = new Dictionary<UIElement, List<int>>();
        Dictionary<List<int>, UIElement> cellToElement = new Dictionary<List<int>, UIElement>();
        Rectangle[,] rects = new Rectangle[9, 9];
        Image[,] imgs = new Image[9, 9];
        double cellHeight = 0, cellWidth = 0;
        double imgHeight = 0, imgWidth = 0;
        double turnImgWidth = 100, turnImgHeight = 50;
        double appMargin = 20;
        int rows = 9, cols = 9;
        double imgToCellPerc = 0.8;
        double inAnimationDurationSec = 0.4;
        EasingFunctionBase popInEasing = new BackEase();
        EasingFunctionBase popOutEasing = new BackEase();
        List<Storyboard> flashStoryboards = new List<Storyboard>();
        string xturnSrc = "Assets/xturn.png";
        string oturnSrc = "Assets/oturn.png";
        GameMode gameMode = GameMode.TwoPlayer;
        int greetingImgWidth = 220;
        int greetingImgHeight = 75;
        int gameOverImgWidth = 250;
        int gameOverImgHeight = 125;
        double greetingDurationSec = 2;
        string[] randomGreetings = new string[] { "awesome", "goodjob", "nicework" };

        bool UIEnabled = true;

        enum GameColor
        {
            BlackBackground,
            NormalCell,
            AvailableBoardGlow,
            BoardWonX,
            BoardWonO,
            BoardDraw,
            Border
        }

        public GamePage()
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                //Indicate the back button press is handled so the app does not exit
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationParams parameters = e.Parameter as NavigationParams;
            if (parameters != null)
            {
                gameMode = parameters.mode;
            }
            PaintGrid();
        }

        Brush GetColor(GameColor c)
        {
            switch (c)
            {
                // hard code all game colors
                case GameColor.BlackBackground:
                    return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                case GameColor.NormalCell:
                    return new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
                case GameColor.AvailableBoardGlow:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                case GameColor.BoardWonX:
                    return new SolidColorBrush(Color.FromArgb(255, 200, 255, 200));
                case GameColor.BoardWonO:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 200, 200));
                case GameColor.BoardDraw:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 200));
                case GameColor.Border:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        void PaintGrid()
        {
            // initializations
            GameGrid.Width = GameGrid.Height = Window.Current.Bounds.Width - 2 * appMargin;
            cellHeight = GameGrid.Height / rows;
            cellWidth = GameGrid.Width / cols;
            imgHeight = cellHeight * imgToCellPerc;
            imgWidth = cellWidth * imgToCellPerc;
            popOutEasing.EasingMode = EasingMode.EaseInOut;

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
                    int gridRow = i;
                    if (i > 2)
                        ++gridRow;
                    if (i > 5)
                        ++gridRow;
                    int gridCol = j;
                    if (j > 2)
                        ++gridCol;
                    if (j > 5)
                        ++gridCol;
                    Grid.SetRow(rects[i, j], gridRow);
                    Grid.SetColumn(rects[i, j], gridCol);
                    Border b = new Border();
                    b.BorderThickness = new Thickness(1);
                    b.BorderBrush = GetColor(GameColor.Border);
                    GameGrid.Children.Add(b);
                    Grid.SetRow(b, gridRow);
                    Grid.SetColumn(b, gridCol);
                }
            }
        }

        void FillCell(int row, int col, string c)
        {
            var img = new Image();
            string path;
            path = "ms-appx:///Assets/" + c + ".png";
            img.Source = new BitmapImage(new Uri(this.BaseUri, path));
            img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img.Stretch = Stretch.UniformToFill;
            img.Tapped += CellTapped;
            Canvas.SetZIndex(img, 0);
            imgs[row, col] = img;

            if (row > 2)
                row++;
            if (row > 6)
                row++;

            if (col > 2)
                col++;
            if (col > 6)
                col++;
            c = c.ToLower();
            if (c != "x" && c != "o")
            {
                StatusText.Text = "Wrong fill character: " + c;
                return;
            }
            GameGrid.Children.Add(img);
            Grid.SetRow(img, row);
            Grid.SetColumn(img, col);

            PopImage(
                img,
                0,
                imgToCellPerc,
                0,
                imgToCellPerc,
                0.5 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellWidth,
                0.5 * cellHeight,
                (1 - imgToCellPerc) / 2 * cellHeight,
                popInEasing);
        }

        private void PopImage(
            Image img,
            double scaleWidthFrom,
            double scaleWidthTo,
            double scaleHeightFrom,
            double scaleHeightTo,
            double moveWidthFrom,
            double moveWidthTo,
            double moveHeightFrom,
            double moveHeightTo,
            EasingFunctionBase easingFunc)
        {
            // animation
            img.RenderTransform = new CompositeTransform();
            Storyboard sb = new Storyboard();
            DoubleAnimation scalex = new DoubleAnimation()
            {
                From = scaleWidthFrom,
                To = scaleWidthTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation scaley = new DoubleAnimation()
            {
                From = scaleHeightFrom,
                To = scaleHeightTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movex = new DoubleAnimation()
            {
                // move from the center of the cell to its left
                From = moveWidthFrom,
                To = moveWidthTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movey = new DoubleAnimation()
            {
                // move from the center of the cell to its top
                From = moveHeightFrom,
                To = moveHeightTo,
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

        void TranslateToVirtualGridCoord(ref int coord)
        {
            if (coord > 2)
                --coord;
            if (coord > 5)
                --coord;
        }

        void CellTapped(object sender, TappedRoutedEventArgs args)
        {
            if (!UIEnabled)
                return;

            int gr = Grid.GetRow(sender as FrameworkElement);
            int gc = Grid.GetColumn(sender as FrameworkElement);

            TranslateToVirtualGridCoord(ref gr);
            TranslateToVirtualGridCoord(ref gc);

            int br = gr / 3;
            int bc = gc / 3;

            int cr = gr % 3;
            int cc = gc % 3;

            int player = currentPlayer;

            PlayMove(gc, gr, bc, br, cc, cr, true);
        }

        async void PlayMove(int gc, int gr, int bc, int br, int cc, int cr, Boolean isHuman)
        {
            // show last turn icon if this is the very first move
            if (lastMoveCol == -1 || lastMoveRow == -1)
            {
                LastTurnImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            lastMoveRow = gr;
            lastMoveCol = gc;

            var result = game.PlayMove(currentPlayer, bc, br, cc, cr);

            if (result >= MoveState.SUCCESS_GAME_ON)
            {
                // stop any flash animations
                ClearFlashing();

                // display turn tip for the *next* player
                //AnimateYourTurn();
                TurnImg.Source = new BitmapImage(new Uri(this.BaseUri, currentPlayer == 1 ? oturnSrc : xturnSrc));
                PopImage(
                    TurnImg,
                    0,
                    1,
                    0,
                    1,
                    0.5 * turnImgWidth,
                    0,
                    0.5 * turnImgHeight,
                    0,
                    popInEasing);

                // flash viable boards for next turn
                if (game.NextBoardCol == -1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            FlashCell(rects[i, j]);
                        }
                    }
                }
                else
                {
                    for (int i = 3 * cc; i < 3 * (cc + 1); i++)
                    {
                        for (int j = 3 * cr; j < 3 * (cr + 1); j++)
                        {
                            FlashCell(rects[j, i]);
                        }
                    }
                }
            }

            StatusText.Text = result.ToString();

            if (game.IsSuccess(result))
            {
                // put an X/O
                // get original cell coords first
                FillCell(gr, gc, currentPlayer == 1 ? "X" : "O");

                // play the corresponding sound effect
                if (currentPlayer == 1)
                {
                    ButtonXSound.Play();
                }
                else
                {
                    ButtonOSound.Play();
                }
            }

            switch (result)
            {
                case MoveState.SUCCESS_GAME_ON:

                    // nothing further for now
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_ON:

                    // if the winner is not AI, greet the board winner
                    if (!(currentPlayer == -1 && gameMode > GameMode.TwoPlayer))
                    {
                        ShowRandomGreeting();
                    }

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_DRAW:

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));

                    // then show the game over (draw) popup
                    GameOver(true);
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_WON:

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));

                    // then show the game over popup
                    GameOver(false);
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW:

                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));

                    // then show the game over (draw) popup
                    GameOver(true);
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_ON:

                    // color the draw board
                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));
                    break;
            }

            if (game.IsSuccessAndGameON(result))
            {
                currentPlayer *= -1;

                if (isHuman && gameMode > GameMode.TwoPlayer)
                {
                    int Cc, Cr;

                    // sleep pretend we're thinking
                    UIEnabled = false;
                    turnProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    await Task.Delay(TimeSpan.FromSeconds(new Random().NextDouble() ));
                    turnProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    switch (gameMode)
                    {
                        case GameMode.AI_LVL_1:
                            GreedyAI1.Play(game, currentPlayer, ref cc, ref cr, out Cc, out Cr, 1);
                            break;
                        case GameMode.AI_LVL_2:
                            GreedyAI1.Play(game, currentPlayer, ref cc, ref cr, out Cc, out Cr, 2);
                            break;
                        case GameMode.AI_LVL_3:
                            GreedyAI1.Play(game, currentPlayer, ref cc, ref cr, out Cc, out Cr, 3);
                            break;
                        
                        default:
                            Cc = -1;
                            Cr = -1;
                            break;
                    }

                    if (cc != -1)
                    {
                        PlayMove(cc * 3 + Cc, cr * 3 + Cr, cc, cr, Cc, Cr, false);
                    }

                    UIEnabled = true;
                }
            }
        }

        private void ClearFlashing()
        {
            foreach (Storyboard sb in flashStoryboards)
            {
                if (sb.GetCurrentState() == ClockState.Active)
                    sb.Stop();
            }
            flashStoryboards.Clear();
        }

        private void ColorBoard(int bc, int br, Brush color)
        {
            for (int i = 3 * bc; i < 3 * (bc + 1); i++)
            {
                for (int j = 3 * br; j < 3 * (br + 1); j++)
                {
                    rects[j, i].Fill = color;
                }
            }
        }

        private void GameOver(bool draw)
        {
            // play game over music
            GameWonSound.Play();

            // show game over popup
            Image gameOverImg = new Image()
            {
                Source = new BitmapImage(new Uri(
                this.BaseUri, draw ? "Assets/gameoverDraw.png" : currentPlayer == 1 ? "Assets/gameoverX.png" : "Assets/gameoverO.png")),
                Height = gameOverImgHeight,
                Width = gameOverImgWidth
            };

            Canvas.SetZIndex(gameOverImg, 100);
            CanvasGrid.Children.Add(gameOverImg);
            Grid.SetRow(gameOverImg, 1);
            Grid.SetColumnSpan(gameOverImg, 2);
            PopImage(
                gameOverImg,
                0,
                1,
                0,
                1,
                0.5 * gameOverImgWidth,
                0,
                0.5 * gameOverImgHeight,
                0,
                popInEasing);

            gameOverImg.Tapped += gameOverImg_Tapped;

            // hide next turn popup
            TurnImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // hide last turn icon
            LastTurnImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // stop all flashing cells
            ClearFlashing();
        }

        void gameOverImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PopImage(
                sender as Image,
                1,
                0,
                1,
                0,
                0,
                0.5 * gameOverImgWidth,
                0,
                0.5 * gameOverImgHeight,
                popOutEasing);
        }

        private async void ShowRandomGreeting()
        {
            // play board won sound
            BoardWonSound.Play();

            Image greetingImg = new Image();
            greetingImg.Source = new BitmapImage(new Uri(
                this.BaseUri, "Assets/greeting_" + randomGreetings[new Random().Next() % randomGreetings.Length] + ".png"));
            greetingImg.Height = greetingImgHeight;
            greetingImg.Width = greetingImgWidth;
            CanvasGrid.Children.Add(greetingImg);
            Grid.SetRow(greetingImg, 0);
            Grid.SetColumnSpan(greetingImg, 2);
            Canvas.SetZIndex(greetingImg, 10);

            PopImage(greetingImg, 0, 1, 0, 1, 0.5 * greetingImgWidth, 0, 0.5 * greetingImgHeight, 0, popInEasing);
            await Task.Delay(TimeSpan.FromSeconds(greetingDurationSec));
            PopImage(greetingImg, 1, 0, 1, 0, 0, 0.5 * greetingImgWidth, 0, 0.5 * greetingImgHeight, popOutEasing);
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
            PopImage(
                TurnImg,
                0.7,
                1,
                0.7,
                1,
                0.5 * turnImgWidth,
                0,
                0.5 * turnImgHeight,
                0, popInEasing);
        }

        private async void TurnImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!UIEnabled || lastMoveCol == -1 || lastMoveRow == -1)
                return;

            // disable UI so that user can't interrupt the pop back animation
            UIEnabled = false;

            PopImage(imgs[lastMoveRow, lastMoveCol],
                imgToCellPerc,
                1.5,
                imgToCellPerc,
                1.5,
                (1 - imgToCellPerc) / 2 * cellWidth,
                -0.25 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellHeight,
                -0.25 * cellHeight,
                popInEasing);

            await Task.Delay(TimeSpan.FromSeconds(1));

            PopImage(imgs[lastMoveRow, lastMoveCol],
                1.5,
                imgToCellPerc,
                1.5,
                imgToCellPerc,
                -0.25 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellWidth,
                -0.25 * cellHeight,
                (1 - imgToCellPerc) / 2 * cellHeight,
                popInEasing);

            UIEnabled = true;
        }
    }
}
