using Reversi.Enums;
using Reversi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reversi.Models
{
    public class BoardViewModel
    {
        public List<TileViewModel> Tiles { get; set; }

        public BoardViewModel()
        {
            Tiles = new List<TileViewModel>();
            for (int row = 0; row < Constants.ROWS; row++)
            {
                for (int col = 0; col < Constants.COLS; col++)
                {
                    Tiles.Add(new TileViewModel(row, col, this));
                }
            }

            InitialiseGame();
        }

        private void InitialiseGame()
        {
            // initialise all tiles on the board
            foreach (var tile in Tiles)
            {
                tile.State = TileStateEnum.EMPTY;
            }

            // set the centre ones to black/white
            GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2 - 1).State = TileStateEnum.WHITE;
            GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2).State = TileStateEnum.BLACK;
            GetTile(Constants.ROWS / 2, Constants.COLS / 2 - 1).State = TileStateEnum.BLACK;
            GetTile(Constants.ROWS / 2, Constants.COLS / 2).State = TileStateEnum.WHITE;
        }

        private TileViewModel GetTile(int row, int col)
        {
            return Tiles.Single(s => s.Row == row && s.Column == col);
        }

    }

}