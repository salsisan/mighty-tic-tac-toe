using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
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
        List<Storyboard> flashStoryboards = new List<Storyboard>();
        string xturnSrc = "Assets/xturn.png";
        string oturnSrc = "Assets/oturn.png";
        GameMode gameMode = GameMode.TwoPlayer;
        int greetingImgWidth = 220;
        int greetingImgHeight = 75;
        int gameOverImgWidth = 284;
        int gameOverImgHeight = 153;
        int newGameImgWidth = 200;
        int newGameImgHeight = 70;
        double greetingDurationSec = 2;
        string[] randomGreetings = new string[] { "awesome", "goodjob", "nicework" };
        bool soundEffectsEnabled = true;
        MoveState lastMoveState = MoveState.SUCCESS_GAME_ON;

        WavePlayer wavePlayer = new WavePlayer();

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
            wavePlayer.AddWave("playerX", "Assets/Sounds/Tap1.wav");
            wavePlayer.AddWave("playerO", "Assets/Sounds/Tap2.wav");
            wavePlayer.AddWave("boardWon", "Assets/Sounds/boardWon.wav");
            wavePlayer.AddWave("gameWon", "Assets/Sounds/gameWon.wav");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        async void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            // if at least one move has been played, and the game is still running
            if (lastMoveCol > -1 && lastMoveState != MoveState.GAME_OVER)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to leave this game? Your progress will be lost!");
                bool? exitSelected = null;
                dialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler((cmd) => exitSelected = true)));
                dialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler((cmd) => exitSelected = false)));
                await dialog.ShowAsync();

                if (exitSelected.HasValue && !exitSelected.Value)
                {
                    e.Handled = true;
                    return;
                }
            }

            if (Frame.CanGoBack)
            {
                //Indicate the back button press is handled so the app does not exit
                e.Handled = true;
                Frame.Navigate(typeof(StartPage));
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
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
            Animations.popOutEasing.EasingMode = EasingMode.EaseInOut;

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

            Animations.PopUIElement(
                img,
                0,
                imgToCellPerc,
                0,
                imgToCellPerc,
                0.5 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellWidth,
                0.5 * cellHeight,
                (1 - imgToCellPerc) / 2 * cellHeight,
                Animations.popInEasing);
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
            if (!UIEnabled || lastMoveState == MoveState.GAME_OVER)
                return;

            int gr = Grid.GetRow(sender as FrameworkElement);
            int gc = Grid.GetColumn(sender as FrameworkElement);

            TranslateToVirtualGridCoord(ref gr);
            TranslateToVirtualGridCoord(ref gc);
            System.Diagnostics.Debug.WriteLine("Tapped cell: " + gr + ", " + gc);

            //if (gr == 0 && gc == 0)
            //{
            //    lastMoveState = MoveState.SUCCESS_BOARD_DRAW_GAME_WON;
            //}
            //if (gr == 0 && gc == 8)
            //{
            //    lastMoveState = MoveState.SUCCESS_BOARD_DRAW_GAME_LOST;
            //}
            //if (gr == 8 && gc == 8)
            //{
            //    lastMoveState = MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW;
            //}
            //GameOver();

            int br = gr / 3;
            int bc = gc / 3;

            int cr = gr % 3;
            int cc = gc % 3;

            PlayMove(gc, gr, bc, br, cc, cr, true);
        }

        async void PlayMove(int gc, int gr, int bc, int br, int cc, int cr, Boolean isHuman)
        {
            // show last turn icon if this is the very first move
            if (lastMoveCol == -1 || lastMoveRow == -1)
            {
                LastTurnImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            lastMoveState = game.PlayMove(currentPlayer, bc, br, cc, cr);

            if (game.IsMoveSuccess(lastMoveState))
            {
                // save this as the last move
                lastMoveRow = gr;
                lastMoveCol = gc;

                // stop any flash animations
                ClearFlashing();

                // display turn tip for the *next* player
                //AnimateYourTurn();
                TurnImg.Source = new BitmapImage(new Uri(this.BaseUri, currentPlayer == 1 ? oturnSrc : xturnSrc));
                Animations.PopUIElement(
                    TurnImg,
                    0,
                    1,
                    0,
                    1,
                    0.5 * turnImgWidth,
                    0,
                    0.5 * turnImgHeight,
                    0,
                    Animations.popInEasing);

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

            StatusText.Text = lastMoveState.ToString();

            if (game.IsMoveSuccess(lastMoveState))
            {
                // put an X/O
                // get original cell coords first
                FillCell(gr, gc, currentPlayer == 1 ? "X" : "O");

                if (soundEffectsEnabled)
                {
                    // play the corresponding sound effect
                    if (currentPlayer == 1)
                    {
                        wavePlayer.PlayWave("playerX");
                    }
                    else
                    {
                        wavePlayer.PlayWave("playerO");
                    }
                }
            }

            switch (lastMoveState)
            {
                case MoveState.SUCCESS_GAME_ON:

                    // nothing further for now
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_ON:

                    // if the winner is not AI, color the board then greet the board winner
                    if (!(currentPlayer == -1 && gameMode > GameMode.TwoPlayer))
                    {
                        ShowRandomGreeting();
                        ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));
                        await Task.Delay(TimeSpan.FromSeconds(greetingDurationSec));
                    }

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_DRAW:

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));

                    // then show the game over (draw) popup
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_WON:

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));

                    // then show the game over popup
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_WON:

                    // color the won board
                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));

                    // then show the game over popup
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_LOST:

                    // color the won board
                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));

                    // then show the game over popup
                    currentPlayer = -1 * currentPlayer;
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_WON_GAME_LOST:

                    // color the won board
                    ColorBoard(bc, br, (currentPlayer == 1) ? GetColor(GameColor.BoardWonX) : GetColor(GameColor.BoardWonO));

                    // then show the game over popup
                    currentPlayer = -1 * currentPlayer;
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW:

                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));

                    // then show the game over (draw) popup
                    GameOver();
                    break;

                case MoveState.SUCCESS_BOARD_DRAW_GAME_ON:

                    // color the draw board
                    ColorBoard(bc, br, GetColor(GameColor.BoardDraw));
                    break;
            }

            if (game.IsSuccessAndGameON(lastMoveState))
            {
                currentPlayer *= -1;

                if (isHuman && gameMode > GameMode.TwoPlayer)
                {
                    int Cc, Cr;

                    // sleep pretend we're thinking
                    UIEnabled = false;
                    turnProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    // Wait for AI to finish rendering
                    await Task.Delay(TimeSpan.FromSeconds(1));

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
                    turnProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

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

        private void GameOver()
        {

            // update playing stats if match was against AI
            if (IsMatchAgainstAI())
            {
                game.UpdateStats(lastMoveState);
            }

            if (soundEffectsEnabled)
            {
                // play game over music
                wavePlayer.PlayWave("gameWon");
            }

            // show game over popup
            bool draw = game.IsGameDraw(lastMoveState);
            Image gameOverImg = new Image()
            {
                Source = new BitmapImage(new Uri(
                this.BaseUri, "Assets/" + (draw ? "gameoverDraw.png" : currentPlayer == 1 ? "gameoverX.png" : "gameoverO.png"))),
                Height = gameOverImgHeight,
                Width = gameOverImgWidth
            };

            Canvas.SetZIndex(gameOverImg, 100);
            CanvasGrid.Children.Add(gameOverImg);
            Grid.SetRow(gameOverImg, 1);
            Grid.SetColumnSpan(gameOverImg, 2);
            Animations.PopUIElement(
                gameOverImg,
                0,
                1,
                0,
                1,
                0.5 * gameOverImgWidth,
                0,
                0.5 * gameOverImgHeight,
                0,
                Animations.popInEasing);
            gameOverImg.Tapped += gameOverImg_Tapped;

            // show new game popup
            Image newGameImg = new Image()
            {
                Source = new BitmapImage(new Uri(this.BaseUri, "Assets/newGame.png")),
                Height = newGameImgHeight,
                Width = newGameImgWidth,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom
            };

            Canvas.SetZIndex(newGameImg, 100);
            CanvasGrid.Children.Add(newGameImg);
            Grid.SetRow(newGameImg, 2);
            Grid.SetColumnSpan(newGameImg, 2);
            Animations.PopUIElement(
                newGameImg,
                0,
                1,
                0,
                1,
                0.5 * newGameImgWidth,
                0,
                0.5 * newGameImgHeight,
                0,
                Animations.popInEasing);
            newGameImg.Tapped += newGameImg_Tapped;

            Animations.SlideUIElement(newGameImg);

            // hide next turn popup
            TurnImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // hide last turn icon
            LastTurnImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // stop all flashing cells
            ClearFlashing();

            lastMoveState = MoveState.GAME_OVER;

        }

        private bool IsMatchAgainstAI()
        {
            return gameMode > GameMode.TwoPlayer;
        }

        void gameOverImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Animations.PopUIElement(
                sender as Image,
                1,
                0,
                1,
                0,
                0,
                0.5 * gameOverImgWidth,
                0,
                0.5 * gameOverImgHeight,
                Animations.popOutEasing);
        }

        void newGameImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationParams parameters = new NavigationParams();
            parameters.mode = gameMode;
            this.Frame.Navigate(typeof(GamePage), parameters);
        }

        private async void ShowRandomGreeting()
        {
            if (soundEffectsEnabled)
            {
                // play board won sound
                wavePlayer.PlayWave("boardWon");
            }

            Image greetingImg = new Image();
            greetingImg.Source = new BitmapImage(new Uri(
                this.BaseUri, "Assets/greeting_" + randomGreetings[new Random().Next() % randomGreetings.Length] + ".png"));
            greetingImg.Height = greetingImgHeight;
            greetingImg.Width = greetingImgWidth;
            CanvasGrid.Children.Add(greetingImg);
            Grid.SetRow(greetingImg, 0);
            Grid.SetColumnSpan(greetingImg, 2);
            Canvas.SetZIndex(greetingImg, 10);

            Animations.PopUIElement(greetingImg, 0, 1, 0, 1, 0.5 * greetingImgWidth, 0, 0.5 * greetingImgHeight, 0, Animations.popInEasing);
            await Task.Delay(TimeSpan.FromSeconds(greetingDurationSec));
            Animations.PopUIElement(greetingImg, 1, 0, 1, 0, 0, 0.5 * greetingImgWidth, 0, 0.5 * greetingImgHeight, Animations.popOutEasing);
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
            Animations.PopUIElement(
                TurnImg,
                0.7,
                1,
                0.7,
                1,
                0.5 * turnImgWidth,
                0,
                0.5 * turnImgHeight,
                0, Animations.popInEasing);
        }

        private async void TurnImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!UIEnabled || lastMoveCol == -1 || lastMoveRow == -1)
                return;

            // disable UI so that user can't interrupt the pop back animation
            UIEnabled = false;

            Animations.PopUIElement(imgs[lastMoveRow, lastMoveCol],
                imgToCellPerc,
                1.5,
                imgToCellPerc,
                1.5,
                (1 - imgToCellPerc) / 2 * cellWidth,
                -0.25 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellHeight,
                -0.25 * cellHeight,
                Animations.popInEasing);

            await Task.Delay(TimeSpan.FromSeconds(1));

            Animations.PopUIElement(imgs[lastMoveRow, lastMoveCol],
                1.5,
                imgToCellPerc,
                1.5,
                imgToCellPerc,
                -0.25 * cellWidth,
                (1 - imgToCellPerc) / 2 * cellWidth,
                -0.25 * cellHeight,
                (1 - imgToCellPerc) / 2 * cellHeight,
                Animations.popInEasing);

            UIEnabled = true;
        }

        private void AudioIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (soundEffectsEnabled)
            {
                AudioIcon.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/audioOff.png"));
            }
            else
            {
                AudioIcon.Source = new BitmapImage(new Uri(this.BaseUri, "Assets/audioOn.png"));
            }
            soundEffectsEnabled = !soundEffectsEnabled;
        }
    }
}
