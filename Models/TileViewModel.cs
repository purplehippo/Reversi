using Reversi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reversi.Models
{
    public class TileViewModel
    {
        public TileStateEnum State { get; set; }
        public int Column { get; private set; }
        public int Row { get; private set; }
        public int TileId { get; private set; }
        public String Location { get; private set; }

        public TileViewModel(int row, int col, BoardViewModel parent)
        {
            Row = row;
            Column = col;
            TileId = parent.Tiles.Count;
            Location = String.Format("{0}{1}", (char)(this.Row + 65), this.Column + 1);
        }
    }
}