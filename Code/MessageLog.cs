using Reversi.Enums;
using System;
using System.Collections.Generic;

namespace Reversi.Code
{
    public enum MessageLogEnum
    {
        PLAYER,
        GAME
    }

    public class MessageLog
    {
        public class Log
        {
            public string log;
            public string msg;
            public TileStateEnum state;
        }

        public List<Log> Logs {get; set; }

        public MessageLog()
        {
            Logs = new List<Log>();
        }
        public void Add(MessageLogEnum log, string msg, TileStateEnum state) {
            Logs.Add(new Log() {log = log.ToString(), msg = msg, state = state});
        }

    }
}