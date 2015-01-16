//using Reversi.Enums;
//using Reversi.Hubs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Reversi.Models
//{
//    public class BoardViewModel
//    {
//        public Guid BoardId { get; set; }
//        public String Name { get; set; }
//        public int WhiteScore { get; set; }
//        public int BlackScore { get; set; }
//        public List<TileViewModel> Tiles { get; set; }
//        public TileStateEnum NextMove { get; set; }
//        public String PlayerMessage { get; set; }
//        public List<String> LogMessages { get; set; }
//        public PlayerModel Player1 { get; set; }
//        public PlayerModel Player2 { get; set; }

//        private Dictionary<String, String> _changedTiles = new Dictionary<string, string>();

//        private static Dictionary<Guid, BoardViewModel> Boards = new Dictionary<Guid,BoardViewModel>();
//        public static BoardViewModel GetById(Guid id)
//        {
//            return Boards.Single(b => b.Key == id).Value;
//        }

//        // set up a delegate function, so we can re-use the nav functions with different implementations
//        private delegate void NavigateFunction(ref int row, ref int col);
//        private static List<NavigateFunction> _navFunctions = new List<NavigateFunction>();
//        static BoardViewModel()
//        {
//            _navFunctions.Add(delegate(ref int row, ref int col) { row++; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { col++; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { row--; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { col--; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { row++; col++; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { row--; col--; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { row++; col--; });
//            _navFunctions.Add(delegate(ref int row, ref int col) { row--; col++; });
//        }

//        public BoardViewModel()
//        {
//            Tiles = new List<TileViewModel>();
//            LogMessages = new List<string>();
//            for (int row = 0; row < Constants.ROWS; row++)
//            {
//                for (int col = 0; col < Constants.COLS; col++)
//                {
//                    Tiles.Add(new TileViewModel(row, col, this));
//                }
//            }

//            InitialiseGame();
//        }

//        private void InitialiseGame()
//        {
//            // initialise all tiles on the board
//            foreach (var tile in Tiles)
//            {
//                tile.State = TileStateEnum.EMPTY;
//            }

//            // set the centre ones to black/white
//            GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2 - 1).State = TileStateEnum.WHITE;
//            GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2).State = TileStateEnum.BLACK;
//            GetTile(Constants.ROWS / 2, Constants.COLS / 2 - 1).State = TileStateEnum.BLACK;
//            GetTile(Constants.ROWS / 2, Constants.COLS / 2).State = TileStateEnum.WHITE;

//            NextMove = TileStateEnum.BLACK;
//            PlayerMessage = NextMove.ToString() + " to play next.";

//            BoardId = Guid.NewGuid();
//            Boards.Add(BoardId, this);
//        }

//        public Dictionary<String, String> PlayMove(Guid tileId)
//        {
//            TileViewModel tile = GetTile(tileId);
//            // clear previous changes
//            _changedTiles.Clear();

//            Console.Write(tile.Row.ToString() + " : " + tile.Column.ToString());

//            // check valid move
//            if (!IsValidMove(tile, NextMove)) return null;
            
//            // set square to new state and add clicked square to list
//            tile.State = NextMove;
//            LogMessage(String.Format("{0} played {1}", tile.State, tile.Location));
//            _changedTiles.Add(tile.TileId.ToString(), tile.State.ToString());

//            // flip opponent's counters
//            FlipOpponentTiles(tile, NextMove);

//            GameOver();

//            NextMove = NextState(NextMove);
//            // swap moves back if next player's move is not possible
//            if (!HasValidMove(NextMove)) NextMove = NextState(NextMove);

//            return _changedTiles;
//        }

//#region "private methods"
//        /// <summary>
//        /// LINQ hunts through tiles to return tile with given row/col
//        /// </summary>
//        /// <param name="row"></param>
//        /// <param name="col"></param>
//        /// <returns>Single tile with given row / col</returns>
//        private TileViewModel GetTile(Guid tileId)
//        {
//            return Tiles.Single(s => s.TileId == tileId);
//        }
//        private TileViewModel GetTile(int row, int col)
//        {
//            return Tiles.Single(s => s.Row == row && s.Column == col);
//        }
//        private TileStateEnum NextState(TileStateEnum state)
//        {
//            return state == TileStateEnum.BLACK ? TileStateEnum.WHITE : TileStateEnum.BLACK;
//        }
//        private void FlipOpponentTiles(TileViewModel tile, TileStateEnum state)
//        {
//            foreach (var nav in _navFunctions)
//            {
//                // check there are occupied tiles around in this direction
//                if (!MoveSurroundingCounters(tile, nav, state)) continue;

//                TileStateEnum opponentState = NextState(state);
//                var tiles = NavigateBoard(nav, tile.Row, tile.Column);
//                foreach (var t in tiles)
//                {
//                    if (t.State == state) break;
//                    t.State = state;
//                    _changedTiles.Add(t.TileId.ToString(), t.State.ToString());
//                }
//            }
//        }
//        private bool MoveSurroundingCounters(TileViewModel tile, NavigateFunction nav, TileStateEnum state)
//        {
//            int idx = 1;
            
//            var surroundingTiles = NavigateBoard(nav, tile.Row, tile.Column);
//            foreach (var t in surroundingTiles)
//            {
//                TileStateEnum currentTileState = t.State;
//                // check immediate neighbour is of other colour
//                if (idx == 1)
//                {
//                    if (currentTileState != NextState(NextMove))
//                    {
//                        //LogMessage(String.Format("Square {0} has no adjacent opponent tiles to turn.", t.Location));
//                        return false;
//                    }
//                }
//                else
//                {
//                    // if we reach a tile of the same colour, this is a valid move
//                    if (currentTileState == state) return true;

//                    if (currentTileState == TileStateEnum.EMPTY)
//                    {
//                        //LogMessage(String.Format("Square {0} has empty square before next tile", t.Location));
//                        return false;
//                    }
//                }
//                idx++;
//            }
//            return false;
//        }
//        /// <summary>
//        /// A list of board tiles that are yielded via the given navigation function
//        /// </summary>
//        /// <param name="nav"></param>
//        /// <param name="row"></param>
//        /// <param name="col"></param>
//        /// <returns></returns>
//        private IEnumerable<TileViewModel> NavigateBoard(NavigateFunction nav, int row, int col)
//        {
//            nav(ref col, ref row); // ????
//            while (col >= 0 && col < Constants.COLS && row >= 0 && row < Constants.ROWS)
//            {
//                yield return GetTile(row, col);
//                nav(ref col, ref row);
//            }
//        }
//        private bool HasValidMove(TileStateEnum state)
//        {
//            // iterate the entire board for available moves
//            for (int r = 0; r < Constants.ROWS; r++) {
//                for (int c = 0; c < Constants.COLS; c++)
//                {
//                    if (IsValidMove(r, c, state)) return true;
//                }
//            }
//            return false;
//        }
//        private void GameOver()
//        {
//            if (!IsGameOver)
//            {
//                PlayerMessage = NextMove.ToString() + " to play next.";
//            }
//            else
//            {
//                PlayerMessage = "Game Over";
//                int black = this.Tiles.Count(b => b.State == TileStateEnum.BLACK);
//                int white = this.Tiles.Count(w => w.State == TileStateEnum.WHITE);
//                LogMessage(String.Format("<b>Game Over!  {0} wins, {1}-{2}.</b>", NextMove.ToString(), (black > white) ? black : white, (black > white) ? white : black));
//            }
//        }
//#endregion
//#region properties
//        public bool IsGameOver
//        {
//            get
//            {
//                return !HasValidMove(TileStateEnum.BLACK) && !HasValidMove(TileStateEnum.WHITE);
//            }
//        }
//#endregion

//        private bool IsValidMove(int row, int col, TileStateEnum state)
//        {
//            return IsValidMove(GetTile(row, col), state);
//        }
//        public bool IsValidMove(TileViewModel tile, TileStateEnum state)
//        {
//            // check the cell is currently empty
//            if (tile.State != TileStateEnum.EMPTY)
//            {
//                //LogMessage(String.Format("Square {0} is not empty", tile.Location));
//                return false;
//            }

//            return _navFunctions.Any(n => MoveSurroundingCounters(tile, n, state));
//        }
    
//        public void LogMessage(String msg)
//        {
//            LogMessages.Add(msg+"<br />");
//        }
    
//    }

//}