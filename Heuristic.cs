using System;
using static Gomoku.Game;

namespace Gomoku
{
    public static class Heuristic {
        public const int winScore = 100000000;

        public static int getScore(int[,] board, bool countingPlayer, bool currentTurn) {
       
            return evaluateHorizontal(board, countingPlayer, currentTurn) +
                    evaluateVertical(board, countingPlayer, currentTurn) +
                    evaluateDiagonal(board, countingPlayer, currentTurn);
        }

        private static int evaluateHorizontal(int[,] board, bool countingPlayer, bool currentTurn) {
            int length = 0;
            int blocks = 2;
            int score = 0;

            for(int i = 0; i < size; i++) {
                for(int j = 0; j < size; j++) {
                    if(board[i, j] == (countingPlayer ? 1 : 2)) {
                        length++;
                    }
                    else if(board[i, j] == 0) {
                        if(length > 0) {
                            blocks--;
                            score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                            length = 0;
                            blocks = 1;
                        }
                        else blocks = 1;
                    }
                    else if(length > 0) {
                        score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                        length = 0;
                        blocks = 2;
                    }
                    else blocks = 2;
                }
                if(length > 0) {
                    score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                }
                length = 0;
                blocks = 2;
            }
            return score;
        }

        private static int evaluateVertical(int[,] board, bool countingPlayer, bool currentTurn) {
            int length = 0;
            int blocks = 2;
            int score = 0;

            for(int j = 0; j < size; j++) {
                for(int i = 0; i < size; i++) {
                    if(board[i, j] == (countingPlayer ? 1 : 2)) {
                        length++;
                    }
                    else if(board[i, j] == 0) {
                        if(length > 0) {
                            blocks--;
                            score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                            length = 0;
                            blocks = 1;
                        }
                        else blocks = 1;
                    }
                    else if(length > 0) {
                        score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                        length = 0;
                        blocks = 2;
                    }
                    else blocks = 2;
                }
                if(length > 0) {
                    score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                }
                length = 0;
                blocks = 2;
            }
            return score;
        }    

        private static int evaluateDiagonal(int[,] board, bool countingPlayer, bool currentTurn) {
            int length = 0;
            int blocks = 2;
            int score = 0;

            for(int k = 0; k <= 2 * (size-1); k++) {
                int iStart = Math.Max(0, k-size+1);
                int iEnd = Math.Min(k, size-1);
                for(int i = iStart; i <= iEnd; i++) {
                    int j = k-i;

                    if(board[i, j] == (countingPlayer ? 1 : 2)) {
                        length++;
                    }
                    else if(board[i, j] == 0) {
                        if(length > 0) {
                            blocks--;
                            score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                            length = 0;
                            blocks = 1;
                        }
                        else blocks = 1;
                    }
                    else if(length > 0) {
                        score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                        length = 0;
                        blocks = 2;
                    }
                    else blocks = 2;
                }
                if(length > 0) {
                    score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                }
                length = 0;
                blocks = 2;
            }

            for(int k = 1-size; k < size; k++) {
                int iStart = Math.Max(0, k);
                int iEnd = Math.Min(size+k-1, size-1);
                for(int i = iStart; i <= iEnd; i++) {
                    int j = i-k;

                    if(board[i, j] == (countingPlayer ? 1 : 2)) {
                        length++;
                    }
                    else if(board[i, j] == 0) {
                        if(length > 0) {
                            blocks--;
                            score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                            length = 0;
                            blocks = 1;
                        }
                        else blocks = 1;
                    }
                    else if(length > 0) {
                        score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                        length = 0;
                        blocks = 2;
                    }
                    else blocks = 2;
                }
                if(length > 0) {
                    score += scoreExchange(length, blocks, countingPlayer == currentTurn);
                }
                length = 0;
                blocks = 2;
            }
            return score;
        }

        private static int scoreExchange(int count, int blocks, bool currentTurn) {
            const int winGuarantee = 1000000;
            if(blocks == 2 && count < 5) return 0;

            switch(count) {
                case 5: {
                    return winScore;
                }
                case 4: {
                    if(currentTurn) return winGuarantee;
                    else {
                        if(blocks == 0) return winGuarantee / 4;
                        else return 200;
                    }
                }
                case 3: {
                    if(blocks == 0) {
                        if(currentTurn) return 50000;
                        else return 200;
                    }
                    else {
                        if(currentTurn) return 10;
                        else return 5;
                    }
                }
                case 2: {
                    if(blocks == 0) {
                        if(currentTurn) return 7;
                        else return 5;
                    }
                    else return 3;
                }
                case 1: {
                    return 1;
                }
            }

            return winScore * 2;
        }
    }
}