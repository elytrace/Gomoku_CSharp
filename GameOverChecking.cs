using System;
using static Gomoku.Form1;

namespace Gomoku
{
    public class GameOverChecking
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

            int[,] left = new int[numOfRows, numOfRows];
            int[,] right = new int[numOfRows, numOfRows];
            int[,] up = new int[numOfRows, numOfRows];
            int[,] down = new int[numOfRows, numOfRows];

            for (int i = 0; i < numOfRows; i++)
            {
                left[i, numOfRows - 1] = (board[i, numOfRows - 1] == currentTurn ? 1 : 0);
                right[i, 0] = (board[i, 0] == currentTurn ? 1 : 0);
                up[0, i] = (board[0, i] == currentTurn ? 1 : 0);
                down[numOfRows - 1, i] = (board[numOfRows - 1, i] == currentTurn ? 1 : 0);
            }

            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
                {
                    if (board[i, j] == currentTurn && j > 0) left[i, j] = left[i, j - 1] + 1;
                    else left[i, j] = 0;

                    if (board[j, i] == currentTurn && j > 0) up[j, i] = up[j - 1, i] + 1;
                    else up[j, i] = 0;

                    j = numOfRows - 1 - j;

                    if (board[i, j] == currentTurn && j < numOfRows-1) right[i, j] = right[i, j + 1] + 1;
                    else right[i, j] = 0;

                    if (board[j, i] == currentTurn && j < numOfRows-1) down[j, i] = down[j + 1, i] + 1;
                    else down[j, i] = 0;

                    j = numOfRows - 1 - j;
                }
            }
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
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

            int[,] top_left = new int[numOfRows, numOfRows];
            int[,] bot_left = new int[numOfRows, numOfRows];
            int[,] top_right = new int[numOfRows, numOfRows];
            int[,] bot_right = new int[numOfRows, numOfRows];

            for (int i = 0; i < numOfRows; i++)
            {
                top_left[i, 0] = board[i, 0] == currentTurn ? 1 : 0;
                top_left[0, i] = board[0, i] == currentTurn ? 1 : 0;

                bot_left[i, 0] = board[i, 0] == currentTurn ? 1 : 0;
                bot_left[numOfRows - 1, i] = board[numOfRows - 1, i] == currentTurn ? 1 : 0;

                top_right[0, i] = board[0, i] == currentTurn ? 1 : 0;
                top_right[i, numOfRows - 1] = board[i, numOfRows - 1] == currentTurn ? 1 : 0;

                bot_right[i, numOfRows - 1] = board[i, numOfRows - 1] == currentTurn ? 1 : 0;
                bot_right[numOfRows - 1, i] = board[numOfRows - 1, i] == currentTurn ? 1 : 0;
            }


            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
                {
                    if (board[i,j] == currentTurn && i > 0 && j > 0)
                    {
                        top_left[i,j] = top_left[i - 1,j - 1] + 1;
                    }
                    else top_left[i,j] = 0;

                    if (board[j,i] == currentTurn && j < numOfRows-1 && i > 0)
                    {
                        bot_left[j,i] = bot_left[j + 1,i - 1] + 1;
                    }
                    else bot_left[j,i] = 0;

                    j = numOfRows - 1 - j;

                    if (board[j,i] == currentTurn && j < numOfRows-1 && i < numOfRows-1)
                    {
                        bot_right[j,i] = bot_right[j + 1,i + 1] + 1;
                    }
                    else bot_right[j,i] = 0;

                    if (board[i,j] == currentTurn && i > 0 && j < numOfRows-1)
                    {
                        top_right[i,j] = top_right[i - 1,j + 1] + 1;
                    }
                    else top_right[i,j] = 0;

                    j = numOfRows - 1 - j;
                }
            }
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
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