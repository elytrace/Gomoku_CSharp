using System;
using static Gomoku.Form1;

namespace Gomoku
{
    public static class GameOverChecking
    {
        public static bool IsWon()
        {
            int answer = Math.Max(CheckNgangDoc(), CheckCheo());

            //MessageBox.Show(CheckNgangDoc() + " " + CheckCheo() + " " + answer.ToString() + " " + currentTurn);

            return answer == 5;
        }

        private static int CheckNgangDoc()
        {
            int answer = 0;

            int[,] left = new int[size, size];
            int[,] right = new int[size, size];
            int[,] up = new int[size, size];
            int[,] down = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                left[i, size - 1] = (board[i, size - 1] == currentTurn ? 1 : 0);
                right[i, 0] = (board[i, 0] == currentTurn ? 1 : 0);
                up[0, i] = (board[0, i] == currentTurn ? 1 : 0);
                down[size - 1, i] = (board[size - 1, i] == currentTurn ? 1 : 0);
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (board[i, j] == currentTurn && j > 0) left[i, j] = left[i, j - 1] + 1;
                    else left[i, j] = 0;

                    if (board[j, i] == currentTurn && j > 0) up[j, i] = up[j - 1, i] + 1;
                    else up[j, i] = 0;

                    j = size - 1 - j;

                    if (board[i, j] == currentTurn && j < size-1) right[i, j] = right[i, j + 1] + 1;
                    else right[i, j] = 0;

                    if (board[j, i] == currentTurn && j < size-1) down[j, i] = down[j + 1, i] + 1;
                    else down[j, i] = 0;

                    j = size - 1 - j;
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int value = Math.Max(Math.Max(left[i, j], right[i, j]), Math.Max(up[i, j], down[i, j]));
                    if (value > answer)
                    {
                        answer = value;
                    }
                }
            }
            return answer;
        }

        private static int CheckCheo()
        {
            int answer = 0;

            int[,] top_left = new int[size, size];
            int[,] bot_left = new int[size, size];
            int[,] top_right = new int[size, size];
            int[,] bot_right = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                top_left[i, 0] = board[i, 0] == currentTurn ? 1 : 0;
                top_left[0, i] = board[0, i] == currentTurn ? 1 : 0;

                bot_left[i, 0] = board[i, 0] == currentTurn ? 1 : 0;
                bot_left[size - 1, i] = board[size - 1, i] == currentTurn ? 1 : 0;

                top_right[0, i] = board[0, i] == currentTurn ? 1 : 0;
                top_right[i, size - 1] = board[i, size - 1] == currentTurn ? 1 : 0;

                bot_right[i, size - 1] = board[i, size - 1] == currentTurn ? 1 : 0;
                bot_right[size - 1, i] = board[size - 1, i] == currentTurn ? 1 : 0;
            }


            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (board[i,j] == currentTurn && i > 0 && j > 0)
                    {
                        top_left[i,j] = top_left[i - 1,j - 1] + 1;
                    }
                    else top_left[i,j] = 0;

                    if (board[j,i] == currentTurn && j < size-1 && i > 0)
                    {
                        bot_left[j,i] = bot_left[j + 1,i - 1] + 1;
                    }
                    else bot_left[j,i] = 0;

                    j = size - 1 - j;

                    if (board[j,i] == currentTurn && j < size-1 && i < size-1)
                    {
                        bot_right[j,i] = bot_right[j + 1,i + 1] + 1;
                    }
                    else bot_right[j,i] = 0;

                    if (board[i,j] == currentTurn && i > 0 && j < size-1)
                    {
                        top_right[i,j] = top_right[i - 1,j + 1] + 1;
                    }
                    else top_right[i,j] = 0;

                    j = size - 1 - j;
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int value = Math.Max(Math.Max(top_left[i,j], bot_right[i,j]), Math.Max(top_right[i,j], bot_left[i,j]));
                    if (value > answer)
                    {
                        answer = value;
                    }
                }
            }
            return answer; 
        }
    }
}