using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty_Tick_Tac_Toe
{
    enum MoveState
    {
        ERROR_CELL_FULL,
        ERROR_WRONG_TURN,
        ERROR_WRONG_BOARD,

        SUCCESS,
        SUCCESS_BOARD_WON,
        SUCCESS_BOARD_DRAW,
        SUCCESS_GAME_WON,
        SUCCESS_GAME_DRAW
    }

    public class GameEngine
    {
        //MoveState PlayMove(int player, int bx, int by, int cx, int cy);
    }
}
