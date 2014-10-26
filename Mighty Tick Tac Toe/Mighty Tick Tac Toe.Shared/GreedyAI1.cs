using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty_Tick_Tac_Toe
{
    class GreedyAI1
    {
        public static void Play(GameEngine ge, int player, ref int Bc, ref int Br, out int Cc, out int Cr, int level)
        {
            int winner;
            int age;

            Play(ge.Cells, player, ref Bc, ref Br, out Cc, out Cr, out winner, out age, level);

        }
        public static void Play(int[, , ,] Cells, int player, ref int Bc, ref int Br, out int Cc, out int Cr, out int winner, out int age, int level)
        {
            int playedMoves = 0;
            foreach (int i in Cells)
            {
                if (i != 0) playedMoves++;
            }

            Cr = -1;
            Cc = -1;
            winner = 0;
            age = 0;
            if (level == 1)
            {
                if (Bc == -1 || boardIsFull(Cells, Bc, Br))
                {
                    ChooseBoardToPlay(Cells, player, ref Bc, ref Br);
                }

                if (Bc == -1)
                {
                    Cr = -1;
                    Cc = -1;
                    winner = 0;
                    age = 0;
                    return;
                }

                if (level == 1)
                {
                    PlayRandomMove(Cells, Bc, Br, out Cc, out Cr);
                }
            }
            else if (level == 2 /* || (level==3 && playedMoves < 15)*/)
            {
                if (Bc == -1 || boardIsFull(Cells, Bc, Br))
                {
                    List<int> availableBc = new List<int>();
                    List<int> availableBr = new List<int>();
                    FindAvailableBoards(Cells, player, availableBc, availableBr);
                    if (availableBc.Count == 0)
                    {
                        Cr = -1;
                        Cc = -1;
                        winner = 0;
                        age = 0;
                        return;
                    }
                    int bestScore = -10;
                    for (int i = 0; i<availableBc.Count; i++)
                    {
                        int localAge;
                        int c, r;
                        int[,,,] cells = new int[3, 3,3 ,3];
                        Array.Copy(Cells, cells, Cells.Length);
                        int score = PlayBestMove(cells, player, availableBc[i], availableBr[i], out c, out r, out localAge);
                        if (score > bestScore || (bestScore != 1 && score == bestScore && localAge >= age) || (bestScore == 1 && score == bestScore && localAge <= age))
                        {
                            if (localAge == age && score == bestScore)
                            {
                                if (new Random().Next() % 2 == 0)
                                {
                                    Bc = availableBc[i];
                                    Br = availableBr[i];
                                    bestScore = score;
                                    Cc = c;
                                    Cr = r;
                                    age = localAge;
                                }
                            }
                            else
                            {
                                Bc = availableBc[i];
                                Br = availableBr[i];
                                bestScore = score;
                                Cc = c;
                                Cr = r;
                                age = localAge;
                            }
                        }
                    }
                }
                else
                {
                    PlayBestMove(Cells, player, Bc, Br, out Cc, out Cr, out age);
                }
            }
            else if (level == 3)
            {
                PlayBestGlobalMove(Cells, player, ref Bc, ref Br, out Cc, out Cr, ref age, 4);
                /*
                if (playedMoves < 50)
                {
                    PlayBestGlobalMove(Cells, player, ref Bc, ref Br, out Cc, out Cr, ref age, 5);
                }
                //else if (playedMoves < 50)
                //{
                //    PlayBestGlobalMove(Cells, player, ref Bc, ref Br, out Cc, out Cr, ref age, 6);
                //}
                else
                {
                    PlayBestGlobalMove(Cells, player, ref Bc, ref Br, out Cc, out Cr, ref age, 10);
                }
                 */
            }
            else
            {
                Cc = -1;
                Cr = -1;
            }

            winner = 0;
            age = 1;
        }

        private static void FindAvailableBoards(int[, , ,] Cells, int player, List<int> availableBc, List<int> availableBr)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    bool isAv = false;
                    for (int ci = 0; ci < 3; ci++)
                    {
                        for (int cj = 0; cj < 3; cj++)
                        {
                            if (Cells[i, j, ci, cj] == 0)
                            {
                                isAv = true;
                                break;
                            }
                        }
                        if (isAv) break;
                    }

                    if (isAv)
                    {
                        availableBc.Add(i);
                        availableBr.Add(j);
                    }
                }
            }
        }

        private static Boolean boardIsFull(int[, , ,] Cells, int Bc, int Br)
        {
            for (int ci = 0; ci < 3; ci++)
            {
                for (int cj = 0; cj < 3; cj++)
                {
                    if (Cells[Bc, Br, ci, cj] == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int PlayBestMove(int[, , ,] Cells, int player, int Bc, int Br, out int Cc, out int Cr, out int age)
        {
            int[,] localBoard = new int[3, 3];
            for (int ci = 0; ci < 3; ci++)
            {
                for (int cj = 0; cj < 3; cj++)
                {
                    localBoard[ci, cj] = Cells[Bc, Br, ci, cj];
                }
            }

            return PlayBestMove(localBoard, player, out Cc, out Cr, out age);
        }

        private static float PlayBestGlobalMove(int[, , ,] Cells, int player, ref int Bc, ref int Br, out int Cc, out int Cr, ref int age, int maxAge)
        {
            List<int> availableBc = new List<int>();
            List<int> availableBr = new List<int>();
            List<int> availableCc = new List<int>();
            List<int> availableCr = new List<int>();
            Cc = -1;
            Cr = -1;

            if (age > maxAge)
            {
                float wins = 0;
                float loses = 0;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (
                            Cells[i, j, 0, 0] + Cells[i, j, 0, 1] + Cells[i, j, 0, 2] == player * 3 ||
                            Cells[i, j, 1, 0] + Cells[i, j, 1, 1] + Cells[i, j, 1, 2] == player * 3 ||
                            Cells[i, j, 2, 0] + Cells[i, j, 2, 1] + Cells[i, j, 2, 2] == player * 3 ||
                            Cells[i, j, 0, 0] + Cells[i, j, 1, 0] + Cells[i, j, 2, 0] == player * 3 ||
                            Cells[i, j, 0, 1] + Cells[i, j, 1, 1] + Cells[i, j, 2, 1] == player * 3 ||
                            Cells[i, j, 0, 2] + Cells[i, j, 1, 2] + Cells[i, j, 2, 2] == player * 3 ||
                            Cells[i, j, 0, 0] + Cells[i, j, 1, 1] + Cells[i, j, 2, 2] == player * 3 ||
                            Cells[i, j, 0, 2] + Cells[i, j, 1, 1] + Cells[i, j, 2, 0] == player * 3
                            )
                        {
                            wins++;
                        }
                        else if (
                            Cells[i, j, 0, 0] + Cells[i, j, 0, 1] + Cells[i, j, 0, 2] == -1 * player * 3 ||
                            Cells[i, j, 1, 0] + Cells[i, j, 1, 1] + Cells[i, j, 1, 2] == -1 * player * 3 ||
                            Cells[i, j, 2, 0] + Cells[i, j, 2, 1] + Cells[i, j, 2, 2] == -1 * player * 3 ||
                            Cells[i, j, 0, 0] + Cells[i, j, 1, 0] + Cells[i, j, 2, 0] == -1 * player * 3 ||
                            Cells[i, j, 0, 1] + Cells[i, j, 1, 1] + Cells[i, j, 2, 1] == -1 * player * 3 ||
                            Cells[i, j, 0, 2] + Cells[i, j, 1, 2] + Cells[i, j, 2, 2] == -1 * player * 3 ||
                            Cells[i, j, 0, 0] + Cells[i, j, 1, 1] + Cells[i, j, 2, 2] == -1 * player * 3 ||
                            Cells[i, j, 0, 2] + Cells[i, j, 1, 1] + Cells[i, j, 2, 0] == -1 * player * 3
                            )
                        {
                            loses++;
                        }

                    }
                }
                return (wins + loses) / 9;
                // instead retun a franction between -1 and 1 based on finished boards
            }
             

            FindAvailableMoves(Cells, Bc, Br, availableBc, availableBr, availableCc, availableCr);

            if (availableBc.Count == 0)
            {
                return 0;
                // instead retun a franction between -1 and 1 based on finished boards
            }

            float bestScore = -10;
            int bestAge = 0;
            for (int i = 0; i < availableBc.Count; i++)
            {
                int localAge = age;
                float score = -10;
                //int[,,,] cells = new int[3, 3, 3, 3];
                GameEngine localEngine = new GameEngine();
                localEngine.NextPlayer = player;
                int cc, cr;
                Array.Copy(Cells, localEngine.Cells, Cells.Length);
                MoveState res = localEngine.PlayMove(player, availableBc[i], availableBr[i], availableCc[i], availableCr[i]);
                localAge++;

                if (res == MoveState.SUCCESS_BOARD_WON_GAME_WON)
                {
                    score = 1;
                }
                else if (!localEngine.IsSuccess(res))
                {
                    break;
                }
                else
                {
                    score = -1 * PlayBestGlobalMove(localEngine.Cells, -1 * player, ref localEngine.NextBoardCol, ref localEngine.NextBoardRow, out cc, out cr, ref localAge, maxAge);
                }

                if (score > bestScore || (bestScore != 1 && score == bestScore && localAge >= bestAge) || (bestScore == 1 && score == bestScore && localAge <= bestAge))
                {
                    if (localAge == bestAge && score == bestScore)
                    {
                        if (new Random().Next() % 2 == 0)
                        {
                            bestScore = score;
                            Bc = availableBc[i];
                            Br = availableBr[i];
                            Cc = availableCc[i];
                            Cr = availableCr[i];
                            bestAge = localAge;
                        }
                    }
                    else
                    {
                            bestScore = score;
                            Bc = availableBc[i];
                            Br = availableBr[i];
                            Cc = availableCc[i];
                            Cr = availableCr[i];
                            bestAge = localAge;
                    }
                }
            }

            age = bestAge;
            return bestScore;
        }

        private static void FindAvailableMoves(int[, , ,] Cells, int Bc, int Br, List<int> availableBc, List<int> availableBr, List<int> availableCc, List<int> availableCr)
        {
            if (Bc == -1 || boardIsFull(Cells, Bc, Br))
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                if (Cells[i, j, k, l] == 0)
                                {
                                    availableBc.Add(i);
                                    availableBr.Add(j);
                                    availableCc.Add(k);
                                    availableCr.Add(l);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        if (Cells[Bc, Br, k, l] == 0)
                        {
                            availableBc.Add(Bc);
                            availableBr.Add(Br);
                            availableCc.Add(k);
                            availableCr.Add(l);
                        }
                    }
                }
            }
        }

        private static int PlayBestMove(int[,] localBoard, int player, out int Cc, out int Cr, out int age)
        {
            List<int> CellCol = new List<int>();
            List<int> CellRow = new List<int>();
            for (int ci = 0; ci < 3; ci++)
            {
                for (int cj = 0; cj < 3; cj++)
                {
                    if (localBoard[ci, cj] == 0)
                    {
                        CellCol.Add(ci);
                        CellRow.Add(cj);
                    }
                }
            }

            Cc = -1;
            Cr = -1;
            age = 0;
            if (CellCol.Count == 0)
            {
                return 0;
            }
            if (CellCol.Count == 9)
            {
                Cc = new Random().Next() % 3;
                Cr = new Random().Next() % 3;
                return 0;
            }

            int bestScore = -10;
            for (int i = 0; i < CellCol.Count; i++)
            {
                int localAge = 0;
                int score = -10;
                int[,] b = new int[3, 3];
                int c, r;
                Array.Copy(localBoard, b, localBoard.Length);

                b[CellCol[i], CellRow[i]] = player;

                var res = CheckBoard(b, player);

                if (res == 1)
                {
                    score = 1;
                    localAge = 1;
                }
                else if (res == -1)
                {
                    score = -1;
                    localAge = 1;
                }
                else if (res == 20)
                {
                    score = 0;
                    localAge = 1;
                }
                else
                {
                    score = -1 * PlayBestMove(b, -1 * player, out c, out r, out localAge);
                    localAge++;
                }

                if (score > bestScore || (bestScore != 1 && score == bestScore && localAge >= age) || (bestScore == 1 && score == bestScore && localAge <= age))
                {
                    if (localAge == age && score==bestScore)
                    {
                        if (new Random().Next() % 2 ==0)
                        { 
                        bestScore = score;
                        Cc = CellCol[i];
                        Cr = CellRow[i];
                        age = localAge;
                            }
                    }
                    else
                    {
                        bestScore = score;
                        Cc = CellCol[i];
                        Cr = CellRow[i];
                        age = localAge; 
                    }
                }
            }

            return bestScore;
        }

        private static int CheckBoard(int[,] b, int player)
        {
            if (b[0, 0] + b[0, 1] + b[0, 2] == 3 ||
                b[1, 0] + b[1, 1] + b[1, 2] == 3 ||
                b[2, 0] + b[2, 1] + b[2, 2] == 3 ||
                b[0, 0] + b[1, 0] + b[2, 0] == 3 ||
                b[0, 1] + b[1, 1] + b[2, 1] == 3 ||
                b[0, 2] + b[1, 2] + b[2, 2] == 3 ||
                b[0, 0] + b[1, 1] + b[2, 2] == 3 ||
                b[0, 2] + b[1, 1] + b[2, 0] == 3
                )
                return player;

            if (b[0, 0] + b[0, 1] + b[0, 2] == -3 ||
                b[1, 0] + b[1, 1] + b[1, 2] == -3 ||
                b[2, 0] + b[2, 1] + b[2, 2] == -3 ||
                b[0, 0] + b[1, 0] + b[2, 0] == -3 ||
                b[0, 1] + b[1, 1] + b[2, 1] == -3 ||
                b[0, 2] + b[1, 2] + b[2, 2] == -3 ||
                b[0, 0] + b[1, 1] + b[2, 2] == -3 ||
                b[0, 2] + b[1, 1] + b[2, 0] == -3
                )
                return -1 * player;

            bool isDraw = true;

            foreach (int i in b)
            {
                if (i == 0)
                {
                    isDraw = false;
                    break;
                }
            }

            if (isDraw)
                return 20;
            else
                return 0;
        }
        private static void PlayRandomMove(int[, , ,] Cells, int Bc, int Br, out int Cc, out int Cr)
        {
            List<int> CellCol = new List<int>();
            List<int> CellRow = new List<int>();

            for (int ci = 0; ci < 3; ci++)
            {
                for (int cj = 0; cj < 3; cj++)
                {
                    if (Cells[Bc, Br, ci, cj] == 0)
                    {
                        CellCol.Add(ci);
                        CellRow.Add(cj);
                    }
                }
            }

            if (CellCol.Count == 0)
            {
                Cc = -1;
                Cr = -1;
                return;
            }

            Random rnd = new Random();
            int index = rnd.Next(0, CellCol.Count);
            Cc = CellCol[index];
            Cr = CellRow[index];
        }

        private static void ChooseBoardToPlay(int[, , ,] Cells, int player, ref int Bc, ref int Br)
        {
            Bc = -1;
            Br = -1;
            int maxCellsMine = -1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int cellsMine = 0;
                    int emptyCells = 0;

                    for (int ci = 0; ci < 3; ci++)
                    {
                        for (int cj = 0; cj < 3; cj++)
                        {
                            if (Cells[i, j, ci, cj] == player)
                                cellsMine++;
                            if (Cells[i, j, ci, cj] == 0)
                                emptyCells++;
                        }
                    }

                    if (cellsMine > maxCellsMine && emptyCells != 0)
                    {
                        maxCellsMine = cellsMine;
                        Bc = i;
                        Br = j;
                    }
                }
            }
        }
    }
}
