using System;
using System.Collections.Generic;
using static Gomoku.Game;

namespace Gomoku
{
    public class Minimax
    {
        private readonly int depth;

        public Minimax(int depth)
        {
            this.depth = depth;
        }

        private static List<Tuple<int, int>> possibleMoves(int[,] board)
        {
            var possibleMoves = new List<Tuple<int, int>>();
            for(int i = 0; i < size; i++) {
                for(int j = 0; j < size; j++) {
                    if(board[i, j] > 0) continue;

                    Tuple<int, int> move;
                    if(i > 0) {
                        if(j > 0) {
                            if(board[i-1, j-1] > 0 || board[i, j-1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(j < size-1) {
                            if(board[i-1, j+1] > 0 || board[i, j+1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(board[i-1, j] > 0) {
                            move = new Tuple<int, int> (i, j);
                            possibleMoves.Add(move);
                            continue;
                        }
                    }

                    if(i < size-1) {
                        if(j > 0) {
                            if(board[i+1, j-1] > 0 || board[i, j-1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(j < size-1) {
                            if(board[i+1, j+1] > 0 || board[i, j+1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(board[i+1, j] > 0) {
                            move = new Tuple<int, int> (i, j);
                            possibleMoves.Add(move);
                        }
                    }
                }
            }
            return possibleMoves;
        }
        
        private static double heuristic(int[,] board, bool currentTurn) {
            
            double playerScore = Heuristic.getScore(board, true, currentTurn);
            double botScore = Heuristic.getScore(board, false, currentTurn);

            if(playerScore == 0) playerScore = 1.0;

            return botScore / playerScore;
        }

        private static double minimax(int[,] board, int depth, double alpha, double beta, bool currentTurn)
        {
            var possibleMoves = Minimax.possibleMoves(board);
            
            if(depth == 0 || possibleMoves.Count == 0) {
                return heuristic(board, !currentTurn);
            }
            if(currentTurn) {
                double maxValue = -1;
                foreach (var move in possibleMoves)
                {
                    board[move.Item1, move.Item2] = WHITE;
                    var value = minimax(board, depth-1, alpha, beta, false);
                    board[move.Item1, move.Item2] = NONE;
                    
                    alpha = Math.Max(alpha, value);
                    if(value >= beta) return value;
                    maxValue = Math.Max(value, maxValue);

                }
                return maxValue;
            }
            else {
                double minValue = 10000000;
                foreach (var move in possibleMoves)
                {
                    board[move.Item1, move.Item2] = BLACK;
                    var value = minimax(board, depth-1, alpha, beta, true);
                    board[move.Item1, move.Item2] = NONE;
                    
                    beta = Math.Min(beta, value);
                    if(value <= alpha) return value;
                    minValue = Math.Min(value, minValue);
                    
                }
                return minValue;
            }
        }

        private static Tuple<int, int> victoryMove(int[,] board, int currentTurn)
        {
            var possibleMoves = Minimax.possibleMoves(board);
            foreach (var move in possibleMoves)
            {
                board[move.Item1, move.Item2] = currentTurn;
                var score = Heuristic.getScore(board, false, currentTurn == WHITE);
                board[move.Item1, move.Item2] = NONE;
                if(score >= Heuristic.winScore)
                {
                    return move;
                }
            }
            return null;
        }

        public Tuple<int, int> best_move(int[,] board, int currentTurn) {
            Tuple<int, int> location = null;
            var possibleMoves = Minimax.possibleMoves(board);
            double value = -1;

            if(victoryMove(board, currentTurn) != null) {
                return victoryMove(board, currentTurn);
            }

            foreach (var move in possibleMoves)
            {
                board[move.Item1, move.Item2] = BLACK;
                var evaluation = minimax(board, depth, -1, Heuristic.winScore, false);
                board[move.Item1, move.Item2] = NONE;
                if(evaluation >= value) {
                    value = evaluation;
                    location = move;
                }
            }
            return location;
        }
    }
}