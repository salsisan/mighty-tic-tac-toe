using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty_Tick_Tac_Toe
{
    class GreedyAI1
    {
        public static void Play(GameEngine ge,int player, ref int Bc, ref int Br, out int Cc, out int Cr)
        {
            int winner;
            int age;

            Play(ge.Cells, player, ref Bc, ref Br, out Cc, out Cr, out winner, out age);

        }
        public static void Play(int[, , ,] Cells, int player, ref int Bc, ref int Br, out int Cc, out int Cr, out int winner, out int age)
        {
            if (Bc == -1)
            {
                ChooseBoardToPlay(Cells, player, ref Bc, ref Br);
            }

            if (Bc == -1)
            {
                Cr = -1;
                Cc = -1;
                winner = 0;
                age = 0;
            }

            PlayRandomMove(Cells, Bc, Br, out Cc, out Cr);
            if (Cc == -1)
            {
                ChooseBoardToPlay(Cells, player, ref Bc, ref Br);
                PlayRandomMove(Cells, Bc, Br, out Cc, out Cr);
            }

            winner = 0;
            age = 1;
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
