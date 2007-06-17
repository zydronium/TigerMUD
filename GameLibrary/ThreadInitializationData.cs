using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace GameLibrary
{
    /// <summary>
    /// Represents a set of objects that are passed to a new client thread upon startup.
    /// </summary>
    public struct ThreadInitializationData
    {
        private PlayerCharacter playerCharacter;

        public PlayerCharacter PlayerCharacter
        {
            get { return playerCharacter; }
            set { playerCharacter = value; }
        }
        private GameContext gameContext;

        public GameContext GameContext
        {
            get { return gameContext; }
            set { gameContext = value; }
        }
        private TcpClient tcpClient;

        public TcpClient TcpClient
        {
            get { return tcpClient; }
            set { tcpClient = value; }
        }
        private TcpListener tcpListener;

        public TcpListener TcpListener
        {
            get { return tcpListener; }
            set { tcpListener = value; }
        }

    }

   
}
