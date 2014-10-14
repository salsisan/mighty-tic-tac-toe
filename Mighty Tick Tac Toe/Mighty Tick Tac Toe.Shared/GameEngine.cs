using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty_Tick_Tac_Toe
{
    public enum MoveState
    {
        ERROR_CELL_FULL,
        ERROR_WRONG_TURN,
        ERROR_WRONG_BOARD,
        ERROR_BAD_COORDS,

        SUCCESS_GAME_ON,
        SUCCESS_BOARD_WON_GAME_ON,
        SUCCESS_BOARD_WON_GAME_WON,
        SUCCESS_BOARD_WON_GAME_DRAW,
        SUCCESS_BOARD_DRAW_GAME_ON,
        SUCCESS_BOARD_DRAW_GAME_DRAW
    }

    public class GameEngine
    {
        int[,] Boards = new int[3, 3];
        int[, , ,] Cells = new int[3, 3, 3, 3];
        int NextPlayer = 1;
        int NextBoardCol = -1;
        int NextBoardRow = -1;

        public bool IsSuccess(MoveState state)
        {
            return state >= MoveState.SUCCESS_GAME_ON;
        }

        public MoveState PlayMove(int player, int Bc, int Br, int Cc, int Cr)
        {
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
            NextBoardCol = Cc;
            NextBoardRow = Cr;

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
                int boardResult = CheckBoards();
                if (boardResult == player)
                {
                    return MoveState.SUCCESS_BOARD_WON_GAME_WON;
                }
                else if (boardResult == 2)
                {
                    return MoveState.SUCCESS_BOARD_WON_GAME_DRAW;
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
                int boardResult = CheckBoards();

                if (boardResult == 2)
                {
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_DRAW;
                }
                else
                {
                    return MoveState.SUCCESS_BOARD_DRAW_GAME_ON;
                }
            }

            return MoveState.SUCCESS_GAME_ON;
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

            bool isDraw = true;

            foreach (int i in Boards)
            {
                if (i == 0)
                {
                    isDraw = false;
                    break;
                }
            }

            if (isDraw)
                return 2;
            else
                return 0;
        }
    }
}
