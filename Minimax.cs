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

        private static List<Tuple<int, int>> possibleMoves(int[,] position)
        {
            var possibleMoves = new List<Tuple<int, int>>();
            for(int i = 0; i < size; i++) {
                for(int j = 0; j < size; j++) {
                    if(position[i, j] > 0) continue;

                    Tuple<int, int> move;
                    if(i > 0) {
                        if(j > 0) {
                            if(position[i-1, j-1] > 0 || position[i, j-1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(j < size-1) {
                            if(position[i-1, j+1] > 0 || position[i, j+1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(position[i-1, j] > 0) {
                            move = new Tuple<int, int> (i, j);
                            possibleMoves.Add(move);
                            continue;
                        }
                    }

                    if(i < size-1) {
                        if(j > 0) {
                            if(position[i+1, j-1] > 0 || position[i, j-1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(j < size-1) {
                            if(position[i+1, j+1] > 0 || position[i, j+1] > 0) {
                                move = new Tuple<int, int> (i, j);
                                possibleMoves.Add(move);
                                continue;
                            }
                        }
                        if(position[i+1, j] > 0) {
                            move = new Tuple<int, int> (i, j);
                            possibleMoves.Add(move);
                        }
                    }
                }
            }
            return possibleMoves;
        }
        
        private static double heuristic(int[,] position, bool player1Turn) {
            
            double playerScore = Heuristic.getScore(position, true, player1Turn);
            double botScore = Heuristic.getScore(position, false, player1Turn);

            if(playerScore == 0) playerScore = 1.0;

            return botScore / playerScore;
        }

        private static double minimax(int[,] position, int depth, double alpha, double beta, bool player1Turn)
        {
            var possibleMoves = Minimax.possibleMoves(position);
            if(depth == 0 || possibleMoves.Count == 0) {
                return heuristic(position, !player1Turn);
            }
            if(player1Turn) {
                double maxValue = -1;
                for(int i = 0; i < possibleMoves.Count; i++) {
                    int[,] current_board = new int[size, size];
                    for(int j = 0; j < size; j++)
                        for(int k = 0; k < size; k++)
                            current_board[j, k] = position[j, k];

                    current_board[possibleMoves[i].Item1, possibleMoves[i].Item2] = 1;
                    double value = minimax(current_board, depth-1, alpha, beta, false);
                    alpha = Math.Max(alpha, value);
                    if(value >= beta) return value;
                    maxValue = Math.Max(value, maxValue);
                    
                }
                return maxValue;
            }
            else {
                double minValue = 10000000;
                for(int i = 0; i < possibleMoves.Count; i++) {
                    int[,] current_board = new int[size, size];
                    for(int j = 0; j < size; j++)
                        for(int k = 0; k < size; k++)
                            current_board[j, k] = position[j, k];

                    current_board[possibleMoves[i].Item1, possibleMoves[i].Item2] = 2;
                    double value = minimax(current_board, depth-1, alpha, beta, true);
                    beta = Math.Min(beta, value);
                    if(value <= alpha) return value;
                    minValue = Math.Min(value, minValue);
        
                }
                return minValue;
            }
        }

        private static Tuple<int, int> winning_move(int[,] position, int currentTurn)
        {
            var possibleMoves = Minimax.possibleMoves(position);
            for(int i = 0; i < possibleMoves.Count; i++) {
                int[,] current_board = new int[size, size];
                for(int j = 0; j < size; j++)
                    for(int k = 0; k < size; k++)
                        current_board[j, k] = position[j, k];
                
                current_board[possibleMoves[i].Item1, possibleMoves[i].Item2] = (currentTurn == WHITE ? 1 : 2);
                if(Heuristic.getScore(current_board, false, currentTurn == WHITE) >= Heuristic.winScore)
                {
                    var location = possibleMoves[i];
                    return location;
                }
            }
            return null;
        }

        public Tuple<int, int> best_move(int[,] position, int currentTurn) {
            Tuple<int, int> location = null;
            var possibleMoves = Minimax.possibleMoves(position);
            double value = -1;

            if(winning_move(position, currentTurn) != null) {
                return winning_move(position, currentTurn);
            }

            for(int i = 0; i < possibleMoves.Count; i++) {
                int[,] current_board = new int[size, size];
                for(int j = 0; j < size; j++)
                    for(int k = 0; k < size; k++)
                        current_board[j, k] = position[j, k];
                        
                current_board[possibleMoves[i].Item1, possibleMoves[i].Item2] = BLACK;
                
                if(minimax(current_board, depth, -1, Heuristic.winScore, false) >= value) {
                    value = minimax(current_board, depth, -1, Heuristic.winScore, false);
                    location = possibleMoves[i];
                }
            }
            return location;
        }
    }
}