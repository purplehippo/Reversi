using Reversi.Enums;
using System;

namespace Reversi.Models
{
    public class PlayerModel
    {
        public string Name { get; set; }
        public PlayerModel Opponent { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsWaitingForTurn { get; set; }
        public bool IsLookingForOpponent { get; set; }
        public TileStateEnum State { get; set; }
        public string ConnectionId { get; set; }
    }
}