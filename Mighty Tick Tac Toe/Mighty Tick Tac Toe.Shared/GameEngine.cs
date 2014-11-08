using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mighty_Tick_Tac_Toe
{
    public enum MoveState
    {
        ERROR_CELL_FULL,
        ERROR_WRONG_TURN,
        ERROR_WRONG_BOARD,
        ERROR_BAD_COORDS,
        ERROR_GAME_FINISHED,

        SUCCESS_GAME_ON,
        SUCCESS_BOARD_WON_GAME_ON,
        SUCCESS_BOARD_WON_GAME_WON,
        SUCCESS_BOARD_WON_GAME_DRAW,
        SUCCESS_BOARD_WON_GAME_LOST,
        SUCCESS_BOARD_DRAW_GAME_ON,
        SUCCESS_BOARD_DRAW_GAME_WON,
        SUCCESS_BOARD_DRAW_GAME_DRAW,
        SUCCESS_BOARD_DRAW_GAME_LOST,

        GAME_OVER
    }

    public class GameEngine
    {
        int[,] Boards = new int[3, 3];
        public int[, , ,] Cells = new int[3, 3, 3, 3];
        public int NextPlayer = 1;
        public int NextBoardCol = -1;
        public int NextBoardRow = -1;
        Boolean gameFinished = false;

        public bool IsMoveSuccess(MoveState state)
        {
            return state >= MoveState.SUCCESS_GAME_ON;
        }

        public bool IsGameWon(MoveState state)
        {
            return state == MoveState.SUCCESS_BOARD_DRAW_GAME_WON | state == MoveState.SUCCESS_BOARD_WON_GAME_WON;
        }

        public bool IsGameLost(MoveState state)
        {
            return state == MoveState.SUCCESS_BOARD_DRAW_GAME_LOST | state == MoveState.SUCCESS_BOARD_WON_GAME_LOST;
        }

        public bool IsGameDraw(MoveState state)
        {
            return state == MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW | state == MoveState.SUCCESS_BOARD_WON_GAME_DRAW;
        }

        public bool IsSuccessAndGameON(MoveState state)
        {
            return (state == MoveState.SUCCESS_GAME_ON || state == MoveState.SUCCESS_BOARD_DRAW_GAME_ON || state == MoveState.SUCCESS_BOARD_WON_GAME_ON);
        }

        public MoveState PlayMove(int player, int Bc, int Br, int Cc, int Cr)
        {
            if (gameFinished)
                return MoveState.ERROR_GAME_FINISHED;

            if (player != NextPlayer)
                return MoveState.ERROR_WRONG_TURN;

            if ((NextBoardCol != -1) && (NextBoardCol != Bc || NextBoardRow != Br))
                return MoveState.ERROR_WRONG_BOARD;

            if (Cells[Bc, Br, Cc, Cr] != 0)
                return MoveState.ERROR_CELL_FULL;

            if (Bc < 0 || Bc > 2 || Br < 0 || Br > 2 || Cc < 0 || Cc > 2 || Cr < 0 || Cr > 2)
                return MoveState.ERROR_BAD_COORDS;

            Cells[Bc, Br, Cc, Cr] = player;
            NextPlayer *= -1;

            if (Boards[Cc, Cr] == 0)
            {
                NextBoardCol = Cc;
                NextBoardRow = Cr;
            }
            else
            {
                NextBoardCol = -1;
                NextBoardRow = -1;
            }

            if (Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 0, 2] == player * 3 ||
                Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 1, 2] == player * 3 ||
                Cells[Bc, Br, 2, 0] + Cells[Bc, Br, 2, 1] + Cells[Bc, Br, 2, 2] == player * 3 ||
                Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 2, 0] == player * 3 ||
                Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 1] == player * 3 ||
                Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 2] + Cells[Bc, Br, 2, 2] == player * 3 ||
                Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 2] == player * 3 ||
                Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 0] == player * 3
                )
            {
                Boards[Bc, Br] = player;

                if (Boards[Cc, Cr] == 0)
                {
                    NextBoardCol = Cc;
                    NextBoardRow = Cr;
                }
                else
                {
                    NextBoardCol = -1;
                    NextBoardRow = -1;
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Cells[Bc, Br, i, j] = player;
                    }
                }

                int boardResult = CheckBoards();
                if (boardResult == player)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_WON_GAME_WON;
                }
                else if (boardResult == 20)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_WON_GAME_DRAW;
                }
                else if (boardResult == -1 * player)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_WON_GAME_LOST;
                }
                else
                {
                    return MoveState.SUCCESS_BOARD_WON_GAME_ON;
                }
            }

            bool drawBoard = true;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Cells[Bc, Br, i, j] == 0)
                    {
                        drawBoard = false;
                    }
                }
            }

            if (drawBoard)
            {
                Boards[Bc, Br] = 20;

                if (Boards[Cc, Cr] == 0)
                {
                    NextBoardCol = Cc;
                    NextBoardRow = Cr;
                }
                else
                {
                    NextBoardCol = -1;
                    NextBoardRow = -1;
                }

                int boardResult = CheckBoards();

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Cells[Bc, Br, i, j] = 20;
                    }
                }

                if (boardResult == player)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_WON;
                }
                else if (boardResult == 20)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW;
                }
                else if (boardResult == -1 * player)
                {
                    gameFinished = true;
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_LOST;
                }
                else
                {
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_ON;
                }
            }

            return MoveState.SUCCESS_GAME_ON;
        }

        public void recalculateBoards()
        {
            for (int Bc = 0; Bc < 3; Bc++)
            {
                for (int Br = 0; Br < 3; Br++)
                {
                    if (Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 0, 2] == 3 ||
                        Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 1, 2] == 3 ||
                        Cells[Bc, Br, 2, 0] + Cells[Bc, Br, 2, 1] + Cells[Bc, Br, 2, 2] == 3 ||
                        Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 2, 0] == 3 ||
                        Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 1] == 3 ||
                        Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 2] + Cells[Bc, Br, 2, 2] == 3 ||
                        Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 2] == 3 ||
                        Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 0] == 3
                        )
                    {
                        Boards[Bc, Br] = 1;
                    }
                    else if (Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 0, 2] == -3 ||
                        Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 1, 2] == -3 ||
                        Cells[Bc, Br, 2, 0] + Cells[Bc, Br, 2, 1] + Cells[Bc, Br, 2, 2] == -3 ||
                        Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 0] + Cells[Bc, Br, 2, 0] == -3 ||
                        Cells[Bc, Br, 0, 1] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 1] == -3 ||
                        Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 2] + Cells[Bc, Br, 2, 2] == -3 ||
                        Cells[Bc, Br, 0, 0] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 2] == -3 ||
                        Cells[Bc, Br, 0, 2] + Cells[Bc, Br, 1, 1] + Cells[Bc, Br, 2, 0] == -3
                        )
                    {
                        Boards[Bc, Br] = -1;
                    }
                    else if (Cells[Bc, Br, 0, 0] == 0 || Cells[Bc, Br, 0, 1] == 0 || Cells[Bc, Br, 0, 2] == 0 ||
                        Cells[Bc, Br, 1, 0] == 0 || Cells[Bc, Br, 1, 1] == 0 || Cells[Bc, Br, 1, 2] == 0 ||
                        Cells[Bc, Br, 2, 0] == 0 || Cells[Bc, Br, 2, 1] == 0 || Cells[Bc, Br, 2, 2] == 0)
                    {
                        Boards[Bc, Br] = 0;
                    }
                    else
                    {
                        Boards[Bc, Br] = 20;
                    }
                }
            }
        }

        private int CheckBoards()
        {
            if (Boards[0, 0] + Boards[0, 1] + Boards[0, 2] == 3 ||
                Boards[1, 0] + Boards[1, 1] + Boards[1, 2] == 3 ||
                Boards[2, 0] + Boards[2, 1] + Boards[2, 2] == 3 ||
                Boards[0, 0] + Boards[1, 0] + Boards[2, 0] == 3 ||
                Boards[0, 1] + Boards[1, 1] + Boards[2, 1] == 3 ||
                Boards[0, 2] + Boards[1, 2] + Boards[2, 2] == 3 ||
                Boards[0, 0] + Boards[1, 1] + Boards[2, 2] == 3 ||
                Boards[0, 2] + Boards[1, 1] + Boards[2, 0] == 3
                )
                return 1;

            if (Boards[0, 0] + Boards[0, 1] + Boards[0, 2] == -3 ||
                Boards[1, 0] + Boards[1, 1] + Boards[1, 2] == -3 ||
                Boards[2, 0] + Boards[2, 1] + Boards[2, 2] == -3 ||
                Boards[0, 0] + Boards[1, 0] + Boards[2, 0] == -3 ||
                Boards[0, 1] + Boards[1, 1] + Boards[2, 1] == -3 ||
                Boards[0, 2] + Boards[1, 2] + Boards[2, 2] == -3 ||
                Boards[0, 0] + Boards[1, 1] + Boards[2, 2] == -3 ||
                Boards[0, 2] + Boards[1, 1] + Boards[2, 0] == -3
                )
                return -1;

            bool isFull = true;
            int wins = 0;
            int loses = 0;
            foreach (int i in Boards)
            {
                if (i == 0)
                {
                    isFull = false;
                }
                else if (i == 1)
                {
                    wins++;
                }
                else if (i == -1)
                {
                    loses++;
                }
            }

            if (isFull)
            {
                if (wins > loses) return 1;
                else if (loses > wins) return -1;
                else return 20;
            }
            else
                return 0;
        }

        public static void GetStats(ref int wins, ref int losses, ref int draws, ref int winStreak, ref int longestWinStreak)
        {
            var data = ApplicationData.Current.LocalSettings;

            // initialize values if it's the first run
            if (data.Values["wins"] == null)
            {
                data.Values["wins"] = 0;
            }
            if (data.Values["losses"] == null)
            {
                data.Values["losses"] = 0;
            }
            if (data.Values["draws"] == null)
            {
                data.Values["draws"] = 0;
            }
            if (data.Values["winStreak"] == null)
            {
                data.Values["winStreak"] = 0;
            }
            if (data.Values["longestWinStreak"] == null)
            {
                data.Values["longestWinStreak"] = 0;
            }

            wins = int.Parse(data.Values["wins"].ToString());
            losses = int.Parse(data.Values["losses"].ToString());
            draws = int.Parse(data.Values["draws"].ToString());
            winStreak = int.Parse(data.Values["winStreak"].ToString());
            longestWinStreak = int.Parse(data.Values["longestWinStreak"].ToString());
        }

        public static bool GetStatsPanelVisibility()
        {
            var data = ApplicationData.Current.LocalSettings;
            if (data.Values["statsPanelVisibility"] == null)
            {
                data.Values["statsPanelVisibility"] = true.ToString();
            }

            return bool.Parse(data.Values["statsPanelVisibility"].ToString());
        }

        public static void SetStatsPanelVisibility(bool state)
        {
            var data = ApplicationData.Current.LocalSettings;
            data.Values["statsPanelVisibility"] = state.ToString();
        }

        public void UpdateStats(MoveState lastMoveState)
        {
            var data = ApplicationData.Current.LocalSettings;
            bool won = (NextPlayer == -1 && IsGameWon(lastMoveState))
                || (NextPlayer == 1 && IsGameLost(lastMoveState));
            bool lost = (NextPlayer == -1 && IsGameLost(lastMoveState))
                || (NextPlayer == 1 && IsGameWon(lastMoveState));
            if (won)
            {
                data.Values["wins"] = int.Parse(data.Values["wins"].ToString()) + 1;
                int winStreak = int.Parse(data.Values["winStreak"].ToString()) + 1;
                data.Values["winStreak"] = winStreak;
                int longestWinStreak = int.Parse(data.Values["longestWinStreak"].ToString());
                if (longestWinStreak < winStreak)
                {
                    longestWinStreak = winStreak;
                }
                data.Values["longestWinStreak"] = longestWinStreak;
            }
            else if (lost)
            {
                data.Values["losses"] = int.Parse(data.Values["losses"].ToString()) + 1;
                data.Values["winStreak"] = 0;
            }
            else
            {
                data.Values["draws"] = int.Parse(data.Values["draws"].ToString()) + 1;
                data.Values["winStreak"] = 0;
            }
        }

        public static void ResetStats()
        {
            var data = ApplicationData.Current.LocalSettings;
            data.Values["wins"] = 0;
            data.Values["losses"] = 0;
            data.Values["draws"] = 0;
            data.Values["winStreak"] = 0;
            data.Values["longestWinStreak"] = 0;
        }
    }
}