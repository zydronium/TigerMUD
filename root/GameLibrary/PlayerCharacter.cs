using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents the player entity when playing the game.
    /// </summary>
    public class PlayerCharacter : GameLibrary.GameObject, IDisposable
    {
        byte[] senddata;

        int data;
        int lastdata;
        char[] receivedchar;
        byte[] receivedbyte;
        ASCIIEncoding ae = new ASCIIEncoding();
        Hashtable colors = new Hashtable();
        char escapecode = '&';
        Queue<PlayerDelegate> commandqueue = new Queue<PlayerDelegate>();
        object lockobj;

        public delegate void PlayerDelegate(object stateinfo);

        private PlayerDelegate commanddelegate;

        private PlayerDelegate CommandDelegate
        {
            get { return commanddelegate; }
            set
            {

                commanddelegate = value;

            }
        }




        public PlayerDelegate GetNextCommand()
        {
            return commandqueue.Dequeue();

        }

        public void SetNextCommand(PlayerDelegate delegatein)
        {
            lock (lockobj)
            {
                commandqueue.Enqueue(delegatein);
            }

        }

        public bool IsCommandQueued
        {
            get
            {

                if (commandqueue.Count > 0) return true;
                else return false;
            }

        }







        public PlayerCharacter()
        {
            Id = Guid.NewGuid().ToString();
            lockobj = new object();


            /*
              &n - normal
              &k - black      &K - gray           &0 - background black
              &b - blue       &B - bright blue    &1 - background blue
              &g - green      &G - bright green   &2 - background green
              &c - cyan       &C - bright cyan    &3 - background cyan
              &r - red        &R - bright red     &4 - background red
              &m - magneta    &M - bright magneta &5 - background magneta
              &y - yellow     &Y - bright yellow  &6 - background yellow
              &w - white      &W - bright white   &7 - background white &9 - reset background
            */
            colors.Add("n", "\x1B[0m");
            colors.Add("I", "\x1B[7m"); // inverse on
            colors.Add("i", "\x1B[27m"); // inverse off
            colors.Add("k", "\x1B[0;30m");
            colors.Add("r", "\x1B[0;31m");
            colors.Add("g", "\x1B[0;32m");
            colors.Add("y", "\x1B[0;33m");
            colors.Add("b", "\x1B[0;34m");
            colors.Add("m", "\x1B[0;35m");
            colors.Add("c", "\x1B[0;36m");
            colors.Add("w", "\x1B[0;37m");
            colors.Add("R", "\x1B[1;31m");
            colors.Add("G", "\x1B[1;32m");
            colors.Add("Y", "\x1B[1;33m");
            colors.Add("B", "\x1B[1;34m");
            colors.Add("M", "\x1B[1;35m");
            colors.Add("C", "\x1B[1;36m");
            colors.Add("W", "\x1B[1;37m");
            colors.Add("0", "\x1B[40m");
            colors.Add("1", "\x1B[41m");
            colors.Add("2", "\x1B[42m");
            colors.Add("3", "\x1B[43m");
            colors.Add("4", "\x1B[44m");
            colors.Add("5", "\x1B[45m");
            colors.Add("6", "\x1B[46m");
            colors.Add("7", "\x1B[47m");
            colors.Add("9", "\x1B[49m");

            //
            // TODO: Add constructor logic here
            //
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~PlayerCharacter()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Send(string text)
        {
            text = Colorize(text);
            // add reset colors to end
            text = text + "\x1B[0m";
            lock (lockobj)
            {
                try
                {
                    senddata = Encoding.ASCII.GetBytes(text);
                    this.Socket.Send(senddata);
                }
                catch { }

            }
        }

        public string Colorize(string text)
        {

            bool foundcode = false;
            if (text == null) return null;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != escapecode && !foundcode) continue;
                if (text[i] == escapecode)
                {
                    // make sure not just an isolated escape character
                    if (text.Length >= i + 1)
                    {
                        string test = text.Substring(i + 1, 1);
                        if (colors.ContainsKey(test))
                        {
                            text = text.Replace(text.Substring(i, 2), Convert.ToString(colors[test]));
                        }
                    }
                }
            }
            return text;

        }


        public void SendLine(string text)
        {
            this.Send(text + "\r\n");
        }

        public void SendLine()
        {
            this.Send("\r\n");
        }

        public void Send(string text, params object[] parameters)
        {
            if (text == null) throw new ArgumentNullException("text");

            for (int i = 0; i < parameters.Length; i++)
            {
                text = text.Replace("{" + i + "}", Convert.ToString(parameters[i]));
            }
            Send(text);
        }

        public void SendLine(string text, params object[] parameters)
        {
            Send(text + "\r\n", parameters);
        }

        private bool connected;

        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        private string ipaddress;

        public string IPAddress
        {
            get { return ipaddress; }
            set { ipaddress = value; }
        }

        private NetworkStream networkstream;

        public NetworkStream NetworkStream
        {
            get { return networkstream; }
            set { networkstream = value; }
        }

        private string usernameresponse;

        public string UsernameResponse
        {
            get { return usernameresponse; }
            set { usernameresponse = value; }
        }

        private string passwordresponse;

        public string PasswordResponse
        {
            get { return passwordresponse; }
            set { passwordresponse = value; }
        }


        private bool messagewaiting;

        public bool MessageWaiting
        {
            get { return messagewaiting; }
            set { messagewaiting = value; }
        }


        private Socket socket;

        public Socket Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        private int loginattempts;

        public int LoginAttempts
        {
            get { return loginattempts; }
            set { loginattempts = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string messageunderconstruction = string.Empty;

        public string MessageUnderConstruction
        {
            get { return messageunderconstruction; }
            set { messageunderconstruction = value; }
        }


        private string messagelast;

        public string MessageLast
        {
            get { return messagelast; }
            set { messagelast = value; }
        }

        private Menu3 menu;

        public Menu3 Menu
        {
            get { return menu; }
            set { menu = value; }
        }


        private string namefirst;

        public string NameFirst
        {
            get { return namefirst; }
            set
            {
                lock (lockobj)
                {
                    namefirst = value;
                }
            }
        }


        private string namelast;

        public string NameLast
        {
            get { return namelast; }
            set
            {
                lock (lockobj)
                {
                    namelast = value;
                }
            }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                lock (lockobj)
                {
                    title = value;
                }

            }
        }

        private int level;

        public int Level
        {
            get { return level; }
            set
            {
                lock (lockobj)
                {
                    level = value;
                }
            }
        }

        private string accountid;

        public string AccountId
        {
            get { return accountid; }
            set { accountid = value; }
        }


        private string characterclass;

        public string CharacterClass
        {
            get { return characterclass; }
            set
            {
                lock (lockobj)
                {
                    characterclass = value;
                }
            }
        }

        ArrayList professions;
        ArrayList skills;
        ArrayList spells;
        ArrayList effects;
        ArrayList statmodifiers;

        private int reputationreceived;

        public int ReputationReceived
        {
            get { return reputationreceived; }
            set
            {
                lock (lockobj)
                {
                    reputationreceived = value;
                }
            }
        }

        private int reputationtogive;

        public int ReputationToGive
        {
            get { return reputationtogive; }
            set
            {
                lock (lockobj)
                {
                    reputationtogive = value;
                }
            }
        }

        ArrayList factions;

        private bool active;

        public bool Active
        {
            get { return active; }
            set
            {
                lock (lockobj)
                {
                    active = value;
                }
            }
        }


        private int health;

        public int Health
        {
            get { return health; }
            set
            {
                lock (lockobj)
                {
                    health = value;
                }
            }
        }

        private int action;

        public int Action
        {
            get { return action; }
            set
            {
                lock (lockobj)
                {
                    action = value;
                }
            }
        }

        private int mental;

        public int Mental
        {
            get { return mental; }
            set
            {
                lock (lockobj)
                {
                    mental = value;
                }
            }
        }

        private Object tradepartner;

        public Object TradePartner
        {
            get { return tradepartner; }
            set
            {
                lock (lockobj)
                {
                    tradepartner = value;
                }
            }
        }

        ArrayList tradelist;

        private GameLibrary.ConnectionState connectionstate;

        public GameLibrary.ConnectionState ConnectionState
        {
            get { return connectionstate; }
            set
            {
                lock (lockobj)
                {
                    connectionstate = value;
                }
            }

        }

        private int strength;

        public int Strength
        {
            get { return strength; }
            set
            {
                lock (lockobj)
                {
                    strength = value;
                }
            }
        }

        private int stamina;

        public int Stamina
        {
            get { return stamina; }
            set
            {
                lock (lockobj)
                {
                    stamina = value;
                }
            }
        }

        private int agility;

        public int Agility
        {
            get { return agility; }
            set
            {
                lock (lockobj)
                {
                    agility = value;
                }
            }
        }


        private int intellect;

        public int Intellect
        {
            get { return intellect; }
            set
            {
                lock (lockobj)
                {
                    intellect = value;
                }
            }
        }

        private int wisdom;

        public int Wisdom
        {
            get
            {
                return wisdom;
            }
            set
            {
                lock (lockobj)
                {
                    wisdom = value;
                }
            }
        }

        private int spirit;

        public int Spirit
        {

            get
            { return spirit; }
            set
            {
                lock (lockobj)
                {
                    spirit = value;
                }
            }
        }

        private bool authenticated;

        public bool Authenticated
        {
            get { return authenticated; }
            set
            {
                lock (lockobj)
                {
                    authenticated = value;
                }
            }
        }

        private int failedlogins;

        public int FailedLogins
        {
            get { return failedlogins; }
            set
            {
                lock (lockobj)
                {
                    failedlogins = value;
                }
            }
        }

        private string room;

        public string Room
        {
            get { return room; }
            set
            {
                lock (lockobj)
                {
                    room = value;
                }
            }
        }

        private int x;

        /// <summary>
        /// X coordinate location on current planet.
        /// </summary>
        public int X
        {
            get { return x; }
            set
            {
                lock (lockobj)
                {
                    x = value;
                }
            }
        }

        private int y;

        /// <summary>
        /// Y coordinate location on current planet.
        /// </summary>
        public int Y
        {
            get { return y; }
            set
            {
                lock (lockobj)
                {
                    y = value;
                }
            }
        }


        private GameLibrary.PlayerAccount account;

        /// <summary>
        /// Account that this character is associated with.
        /// </summary>
        public GameLibrary.PlayerAccount Account
        {
            get { return account; }
            set
            {
                lock (lockobj)
                {
                    account = value;
                }
            }
        }

        private Question question;

        public Question Question
        {
            get { return question; }
            set
            {
                lock (lockobj)
                {
                    question = value;
                }
            }
        }
      
        public GameLibrary.CommunicationResult HandleKeystrokes()
        {
            //this.Message = string.Empty;
            lastdata = data;
            try
            {
                // Handle ungraceful client disconnects
                if (this.Socket.Poll(1, SelectMode.SelectRead) && !this.NetworkStream.DataAvailable)
                {
                    return CommunicationResult.Error;
                }

                if (!this.NetworkStream.DataAvailable) return CommunicationResult.NoMessageReceived;

                // Get available data
                if (this.NetworkStream.DataAvailable)
                {
                    data = this.NetworkStream.ReadByte();

                }
            }
            catch (System.IO.IOException)
            {
                return CommunicationResult.Error;
            }

            // -1 is a disconnect
            if (data == -1)
            {
                return CommunicationResult.Error;
            }
            if (data == 13)
            {
                // Upon carriage return, go process the message string
                this.Message = this.MessageUnderConstruction;
                this.MessageUnderConstruction = string.Empty;
                return CommunicationResult.MessageReceived;
            }
            else if (data == 10 && lastdata != 13)
            {
                this.Message = this.MessageUnderConstruction;
                this.MessageUnderConstruction = string.Empty;
                return CommunicationResult.MessageReceived;
            }
            else if (data == 10 && lastdata == 13)
            {
                return CommunicationResult.NoMessageReceived;
                // do nothing
            }
            else if (data == 8)
            {
                // protect against going backwards past beginning
                if (this.MessageUnderConstruction.Length > 0)
                    this.MessageUnderConstruction = this.MessageUnderConstruction.Substring(0, this.MessageUnderConstruction.Length - 1);
                return CommunicationResult.NoMessageReceived;
            }
            else
            {
                receivedbyte = BitConverter.GetBytes(data);
                receivedchar = ae.GetChars(receivedbyte);
                this.MessageUnderConstruction += receivedchar[0];
                return CommunicationResult.NoMessageReceived;
            }
        }

        public string GetResponse(string prompt)
        {
            this.Send(prompt);
            return this.GetResponse();
        }

        public string GetResponse()
        {
            this.Message = "";
            while (true)
            {
                lastdata = data;
                try
                {
                    data = this.NetworkStream.ReadByte();
                }
                catch (System.IO.IOException)
                {
                    return null;
                }

                // -1 is a disconnect
                if (data == -1)
                {
                    return null;
                }
                if (data == 13)
                {
                    // Upon carriage return, go process the message string
                    return this.Message;
                }
                else if (data == 10 && lastdata != 13)
                {
                    return this.Message;
                }
                else if (data == 10 && lastdata == 13)
                {
                    // do nothing
                }
                else if (data == 8)
                {
                    // protect against going backwards past beginning
                    if (this.Message.Length > 0)
                        this.Message = this.Message.Substring(0, this.Message.Length - 1);
                }
                else
                {
                    receivedbyte = BitConverter.GetBytes(data);
                    receivedchar = ae.GetChars(receivedbyte);
                    this.Message += receivedchar[0];
                }
            }
        }



        private string planetid;

        public string PlanetID
        {
            get { return planetid; }
            set { planetid = value; }
        }

        private Planet planet;

        public Planet Planet
        {
            get { return planet; }
            set { planet = value; }
        }



    }
}