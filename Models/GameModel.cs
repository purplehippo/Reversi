using Reversi.Code;
using Reversi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reversi.Models
{
    public class GameModel
    {
        public int WhiteScore { get; set; }
        public int BlackScore { get; set; }
        public List<TileViewModel> Tiles { get; set; }
        public TileStateEnum NextMove { get; set; }
        public String PlayerMessage { get; set; }
        public MessageLog MessageLog { get; set; }
        public PlayerModel Player1 { get; set; }
        public PlayerModel Player2 { get; set; }
        public List<int> ValidTiles { get; set; }

        private Dictionary<String, String> _changedTiles = new Dictionary<string, string>();

        // set up a delegate function, so we can re-use the nav functions with different implementations
        private delegate void NavigateFunction(ref int row, ref int col);
        private static List<NavigateFunction> _navFunctions = new List<NavigateFunction>();
        static GameModel()
        {
            _navFunctions.Add(delegate(ref int row, ref int col) { row++; });
            _navFunctions.Add(delegate(ref int row, ref int col) { col++; });
            _navFunctions.Add(delegate(ref int row, ref int col) { row--; });
            _navFunctions.Add(delegate(ref int row, ref int col) { col--; });
            _navFunctions.Add(delegate(ref int row, ref int col) { row++; col++; });
            _navFunctions.Add(delegate(ref int row, ref int col) { row--; col--; });
            _navFunctions.Add(delegate(ref int row, ref int col) { row++; col--; });
            _navFunctions.Add(delegate(ref int row, ref int col) { row--; col++; });
        }

        public GameModel()        
        {
            InitialiseGame();
        }

        private void InitialiseGame()
        {
            Tiles = new BoardViewModel().Tiles;
            NextMove = TileStateEnum.BLACK;
            MessageLog = new MessageLog();
            ValidTiles = new List<int>();
            PlayerMessage = NextMove.ToString() + " to play next.";
//            GameOver();
        }

        /// <summary>
        /// Play the move!
        /// Checks for validity and game over.  Lists the tiles that require flipping.
        /// </summary>
        /// <param name="tileId"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public Dictionary<String, String> PlayMove(int tileId, PlayerModel player)
        {
            TileViewModel tile = GetTile(tileId);
            // clear previous changes
            _changedTiles.Clear();

            if (player.State != NextMove)
            {
                LogMessage(MessageLogEnum.PLAYER, "It is not your turn", player.State);
                return null;
            }
            // check valid move
            if (!IsValidMove(tile, NextMove))
            {
                LogMessage(MessageLogEnum.PLAYER, String.Format("{0} invalid move", tile.Location), player.State);
                return null;
            }
            
            // set square to new state and add clicked square to list
            tile.State = NextMove;
            LogMessage(MessageLogEnum.GAME, String.Format("{0} played {1}", tile.State, tile.Location), NextMove);
            _changedTiles.Add(tile.TileId.ToString(), tile.State.ToString());

            // flip opponent's counters
            FlipOpponentTiles(tile, NextMove);

            NextMove = NextState(NextMove);
            PlayerMessage = NextMove.ToString() + " to play next.";

            // swap moves back if next player's move is not possible
            if (!HasValidMove(NextMove))
            {
                NextMove = NextState(NextMove);
                GameOver();
            }

            return _changedTiles;
        }

#region "private methods"
        /// <summary>
        /// LINQ hunts through tiles to return tile with given row/col
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>Single tile with given row / col</returns>
        private TileViewModel GetTile(int tileId)
        {
            return Tiles.Single(t => t.TileId == tileId);
        }
        private TileViewModel GetTile(int row, int col)
        {
            return Tiles.Single(s => s.Row == row && s.Column == col);
        }
        private TileStateEnum NextState(TileStateEnum state)
        {
            return state == TileStateEnum.BLACK ? TileStateEnum.WHITE : TileStateEnum.BLACK;
        }
        /// <summary>
        /// Create a list of tiles, and their state, to flip -  in each direction
        /// given by delegates, _navFunctions
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="state"></param>
        private void FlipOpponentTiles(TileViewModel tile, TileStateEnum state)
        {
            foreach (var nav in _navFunctions)
            {
                // check there are occupied tiles around in this direction
                if (!MoveSurroundingCounters(tile, nav, state)) continue;

                TileStateEnum opponentState = NextState(state);
                var tiles = NavigateBoard(nav, tile.Row, tile.Column);
                foreach (var t in tiles)
                {
                    if (t.State == state) break;
                    t.State = state;
                    _changedTiles.Add(t.TileId.ToString(), t.State.ToString());
                }
            }
        }
        /// <summary>
        /// Check for valid tiles in each direction given by delegate funcitons
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="nav"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool MoveSurroundingCounters(TileViewModel tile, NavigateFunction nav, TileStateEnum state)
        {
            int idx = 1;
            
            var surroundingTiles = NavigateBoard(nav, tile.Row, tile.Column);
            foreach (var t in surroundingTiles)
            {
                TileStateEnum currentTileState = t.State;
                // check immediate neighbour is of other colour
                if (idx == 1)
                {
                    if (currentTileState != NextState(NextMove))
                    {
                        //LogMessage(String.Format("Square {0} has no adjacent opponent tiles to turn.", t.Location));
                        return false;
                    }
                }
                else
                {
                    // if we reach a tile of the same colour, this is a valid move
                    if (currentTileState == state) return true;

                    if (currentTileState == TileStateEnum.EMPTY)
                    {
                        //LogMessage(String.Format("Square {0} has empty square before next tile", t.Location));
                        return false;
                    }
                }
                idx++;
            }
            return false;
        }
        /// <summary>
        /// A list of board tiles that are yielded via the given delegate function
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private IEnumerable<TileViewModel> NavigateBoard(NavigateFunction nav, int row, int col)
        {
            nav(ref col, ref row);
            while (col >= 0 && col < Constants.COLS && row >= 0 && row < Constants.ROWS)
            {
                yield return GetTile(row, col);
                nav(ref col, ref row);
            }
        }
        /// <summary>
        /// Check the entire board to see if player has a move.
        /// Store the tile to enable display of valid moves.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool HasValidMove(TileStateEnum state)
        {
            bool bValid = false;
            ValidTiles.Clear();

            // iterate the entire board for available moves
            for (int r = 0; r < Constants.ROWS; r++) {
                for (int c = 0; c < Constants.COLS; c++)
                {
                    if (IsValidMove(r, c, state))
                    {
                        ValidTiles.Add(GetTile(r, c).TileId);
                        bValid = true;
                        //return true;
                    }
                }
            }
            return bValid;
        }
        private void GameOver()
        {
            if (!IsGameOver)
            {
                PlayerMessage = NextMove.ToString() + " to play next.";
            }
            else
            {
                int black = this.Tiles.Count(b => b.State == TileStateEnum.BLACK);
                int white = this.Tiles.Count(w => w.State == TileStateEnum.WHITE);
                string overMsg = String.Format("<b>Game Over!  {0} wins, {1}-{2}.</b>", NextMove.ToString(), (black > white) ? black : white, (black > white) ? white : black);
                PlayerMessage = overMsg + "<br />You may join another game.";
                LogMessage(MessageLogEnum.GAME, overMsg, TileStateEnum.EMPTY);

                // reset player states for re-join
                this.Player1.IsLookingForOpponent = true;
                this.Player2.IsLookingForOpponent = true;
                this.Player1.IsPlaying = false;
                this.Player2.IsPlaying = false;
            }
        }
#endregion
#region properties
        public bool IsGameOver
        {
            get
            {
                return !HasValidMove(TileStateEnum.BLACK) && !HasValidMove(TileStateEnum.WHITE);
            }
        }
#endregion

        private bool IsValidMove(int row, int col, TileStateEnum state)
        {
            return IsValidMove(GetTile(row, col), state);
        }
        public bool IsValidMove(TileViewModel tile, TileStateEnum state)
        {
            // check the cell is currently empty
            if (tile.State != TileStateEnum.EMPTY)
            {
                //LogMessage(String.Format("Square {0} is not empty", tile.Location));
                return false;
            }

            // apply movesurroundingcounters check to all directions on delegate functions
            return _navFunctions.Any(n => MoveSurroundingCounters(tile, n, state));
        }
    
        public void LogMessage(MessageLogEnum log, string msg, TileStateEnum state)
        {
            MessageLog.Add(log, msg + "<br />", state);
        }

    }
}