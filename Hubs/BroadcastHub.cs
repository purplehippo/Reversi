using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Reversi.Enums;
using Reversi.Models;
using System.Threading.Tasks;
using Reversi.Code;

namespace Reversi.Hubs
{
    public class BroadcastHub : Hub
    {
        private static object _syncRoot = new object();
        private static readonly Random rand = new Random();

        /// <summary>
        /// list of connected players, both playing and looking for a game
        /// </summary>
        private static readonly List<PlayerModel> players = new List<PlayerModel>();
        /// <summary>
        /// list of available games
        /// </summary>
        private static readonly List<GameModel> boards = new List<GameModel>();

        /// <summary>
        /// Updates player stats when a player connects to the Hub
        /// (new player creation is a separate funciton)
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected() 
        {
            return UpdatePlayerStats();
        }
        /// <summary>
        /// Removes player / board if a player disconnects from the Hub.
        /// Allows opponent to win.
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            // find the game being played
            GameModel game = CurrentBoard();
            // if no game found, player did not have one, remove them
            if (game == null)
            {
                PlayerModel player = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    players.Remove(player);
                    UpdatePlayerStats();
                }
            }
            else
            {
                // game found, but a player has left
                PlayerModel p = CurrentPlayer();
                boards.Remove(game);
                if (p != null)
                {
                    players.Remove(p);
                    if (p.Opponent != null)
                    {
                        UpdatePlayerStats();
                        // update the opponent's status, so they can look for new game
                        p.Opponent.IsLookingForOpponent = true;
                        p.Opponent.IsPlaying = false;
                        // let the remaining player know they're all alone
                        return Clients.Client(p.Opponent.ConnectionId).OpponentDisconnected(new { name = p.Name });
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Sends update call to UI for player stats
        /// </summary>
        /// <returns></returns>
        public Task UpdatePlayerStats()
        {
            return Clients.All.UpdatePlayers(new { noGamesPlaying = boards.Count, noPlayers = players.Count });
        }
        /// <summary>
        /// Creates a new Player and adds them to the available to play list
        /// </summary>
        /// <param name="name">Name of player from UI</param>
        public void NewPlayer(string name)
        {
            lock (_syncRoot)
            {
                PlayerModel player = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player == null)
                {
                    player = new PlayerModel { ConnectionId = Context.ConnectionId, Name = name };
                    players.Add(player);
                }
                player.IsPlaying = false;
            }

            UpdatePlayerStats();
            // register of this player is complete...  TODO: add message to usr
        }
        /// <summary>
        /// Allows a player to connect with another player and creates a new board game.
        /// Notifies the caller and their new opponent
        /// </summary>
        public void FindOpponent()
        {
            // get Player data for requesting user
            PlayerModel player = players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            player.IsLookingForOpponent = true;

            // pick an opponent
            PlayerModel opponent = players.FirstOrDefault(o => o.ConnectionId != Context.ConnectionId && o.IsLookingForOpponent && !o.IsPlaying);
            if (opponent == null)
            {
                Clients.Client(player.ConnectionId).NoOpponents();
                return;
            }

            player.IsPlaying = true;
            opponent.IsPlaying = true;
            player.IsLookingForOpponent = false;
            opponent.IsLookingForOpponent = false;

            player.Opponent = opponent;
            opponent.Opponent = player;

            // randomly set state
            if (rand.NextDouble() > 0.5)
            {
                player.State = TileStateEnum.BLACK;
                opponent.State = TileStateEnum.WHITE;
            }
            else
            {
                player.State = TileStateEnum.WHITE;
                opponent.State = TileStateEnum.BLACK;
            }

            // notify both players there is a new game and set random start colour
            Clients.Client(player.ConnectionId).FoundOpponent(new { name = player.Name, opponentName = opponent.Name, state = player.State.ToString() });
            Clients.Client(opponent.ConnectionId).FoundOpponent(new { name = opponent.Name, opponentName = player.Name, state = opponent.State.ToString() });
            Clients.Client(player.ConnectionId).ShowValidMoves(new List<int>() { 19, 26, 37, 44 }, player.State == TileStateEnum.BLACK);
            Clients.Client(opponent.ConnectionId).ShowValidMoves(new List<int>() { 19, 26, 37, 44 }, opponent.State == TileStateEnum.BLACK);

            lock (_syncRoot)
            {
                boards.Add(new GameModel { Player1 = player, Player2 = opponent });
            }

            UpdatePlayerStats();
        }
        /// <summary>
        /// Update score and next state message
        /// </summary>
        public void UpdateToolbox()
        {
            GameModel board = CurrentBoard();
            if (board == null) return;
            //Guid boardId = new Guid(boardClickedId);
            //BoardViewModel board = BoardViewModel.GetById(boardId);
            Clients.Client(Context.ConnectionId).UpdateToolbox(board.Tiles.Count(b => b.State == TileStateEnum.BLACK), board.Tiles.Count(b => b.State == TileStateEnum.WHITE), board.PlayerMessage);
        }
        /// <summary>
        /// Enables play of a move.
        /// Uses GameModel to define list of tiles to flip (or notify of invalid move) 
        /// </summary>
        /// <param name="changes"></param>
        public void Play(string tileClickedId)
        {
            int tileId = Int32.Parse(tileClickedId);
            // check we are connected to a valid board
            GameModel board = CurrentBoard();
            if (board == null)
            {
                NotConnected();
                return;
            }

            // list the flipped tiles
            Dictionary<String, String> changedTileIds = board.PlayMove(tileId, CurrentPlayer());

            // update required messages and board for player and opponent
            Clients.Client(board.Player1.ConnectionId).UpdateGrid(changedTileIds);
            Clients.Client(board.Player2.ConnectionId).UpdateGrid(changedTileIds);

            Clients.Client(board.Player1.ConnectionId).UpdateToolbox(board.Tiles.Count(b => b.State == TileStateEnum.BLACK), board.Tiles.Count(b => b.State == TileStateEnum.WHITE), board.PlayerMessage);
            Clients.Client(board.Player2.ConnectionId).UpdateToolbox(board.Tiles.Count(b => b.State == TileStateEnum.BLACK), board.Tiles.Count(b => b.State == TileStateEnum.WHITE), board.PlayerMessage);

            Clients.Client(board.Player1.ConnectionId).UpdateGameMessages(board.MessageLog.Logs, board.Player1.State);
            Clients.Client(board.Player2.ConnectionId).UpdateGameMessages(board.MessageLog.Logs, board.Player2.State);

            Clients.Client(board.Player1.ConnectionId).ShowValidMoves(board.ValidTiles, board.NextMove == board.Player1.State);
            Clients.Client(board.Player2.ConnectionId).ShowValidMoves(board.ValidTiles, board.NextMove == board.Player2.State);
        }
        /// <summary>
        /// Set up a dummy board in order to obtain initial score and start point
        /// </summary>
        public void Initialise()
        {
            // presume a new board, as we're just initialising initial text
            GameModel board = new GameModel();
            Clients.Client(Context.ConnectionId).UpdateToolbox(board.Tiles.Count(b => b.State == TileStateEnum.BLACK), board.Tiles.Count(b => b.State == TileStateEnum.WHITE), board.PlayerMessage);
        }

        /// <summary>
        /// Send a chat message to the board's players
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        public void SendChatMessage(string from, string message)
        {
            // check we are connected to a valid board
            GameModel board = CurrentBoard();
            if (board == null)
            {
                NotConnected();
                return;
            }
            string time = DateTime.Now.ToString("HH:mm:ss");
            Clients.Client(board.Player1.ConnectionId).ReceiveMessage(time, from, message);
            Clients.Client(board.Player2.ConnectionId).ReceiveMessage(time, from, message);
        }
        /// <summary>
        /// Find the current board for this player's connection
        /// </summary>
        /// <returns></returns>
        private GameModel CurrentBoard()
        {
            return boards.FirstOrDefault(b => b.Player1.ConnectionId == Context.ConnectionId || b.Player2.ConnectionId == Context.ConnectionId);
        }
        /// <summary>
        /// Find the current player for this connection
        /// </summary>
        /// <returns></returns>
        private PlayerModel CurrentPlayer()
        {
            GameModel board = CurrentBoard();
            if (board == null) return null;

            return board.Player1.ConnectionId == Context.ConnectionId ? board.Player1 : board.Player2;
        }

        /// <summary>
        /// Add a not connected message to the Log, and broadcast to requesting client
        /// Note: Log no longer used since port to Angular (still tidying up).  Now stored $scope.message
        /// </summary>
        private void NotConnected()
        {
            MessageLog log = new MessageLog();
            log.Add(MessageLogEnum.PLAYER, "You are not connected to a game. Please join a game.", TileStateEnum.EMPTY);
//            Clients.Client(Context.ConnectionId).UpdateGameMessages(log.Logs, TileStateEnum.EMPTY);
            Clients.Client(Context.ConnectionId).UpdatePlayerMessages("You are not connected to a game. Please join a game.", TileStateEnum.EMPTY);
        }

    }

}