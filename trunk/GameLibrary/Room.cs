using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Threading;
using System.Collections;
using System.Collections.Generic;


namespace GameLibrary
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class Room : GameLibrary.GameObject, IDisposable
    {
        /// <summary>
        /// Exits that lead out of this room.
        /// </summary>
        Dictionary<string, Exit> exits = new Dictionary<string, Exit>();
        
        /// <summary>
        /// Players in this room.
        /// </summary>
        List<PlayerCharacter> players = new List<PlayerCharacter>();
        
        /// <summary>
        /// Keys that can open this room.
        /// </summary>
        Dictionary<string, Key> keys = new Dictionary<string,Key>();

        List<Item> items = new List<Item>();
        object lockobj;

        /// <summary>
        /// Represents a room that players and npcs can move through.
        /// </summary>
        public Room()
        {
            Id = Guid.NewGuid().ToString();
            lockobj = new object();


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
        ~Room()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

      


        /// <summary>
        /// Get the list of exists for the room.
        /// </summary>
        /// <returns>A list of room exits.</returns>
        public List<string> GetExitList()
        {
            List<string> tempexits = new List<string>();

            foreach (string key in exits.Keys)
            {
                tempexits.Add(key);
            }
            return tempexits;

        }

        /// <summary>
        /// Gets an exit based on the name of the exit.
        /// </summary>
        /// <param name="exitname">Name of the exit to get.</param>
        /// <returns>A room exit.</returns>
        public Exit GetExit(string exitname)
        {
            return exits[exitname];
        }

        /// <summary>
        /// Check if an exit with a certain name exists in the room
        /// </summary>
        /// <param name="exitname"></param>
        /// <returns></returns>
        public bool DoesExitExist(string exitname)
        {
            if (exits.ContainsKey(exitname)) return true;
            else return false;
        }

        

        /// <summary>
        /// Add an exit to the room.
        /// </summary>
        /// <param name="exitname"></param>
        /// <param name="exit"></param>
        /// <returns></returns>
        public Exception AddExit(string exitname, Exit exit)
        {
            try
            {
                exits.Add(exitname, exit);
                return null;

            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Remove an exit from the room.
        /// </summary>
        /// <param name="exitname"></param>
        /// <returns></returns>
        public Exception RemoveExit(string exitname)
        {
            try
            {
                exits.Remove(exitname);
                return null;
            }
            catch (Exception ex) { return ex; }

        }

        public string desc;

        /// <summary>
        /// The room's description.
        /// </summary>
        public string Description
        {
            get { return desc; }
            set { desc = value; }
        }



        private bool indoor;

        /// <summary>
        /// Is the room considered indoors or outdoors?
        /// </summary>
        public bool Indoor
        {
            get { return indoor; }
            set { indoor = value; }
        }

        private bool skyvisible;

        /// <summary>
        /// Is the sky visible from the room?
        /// </summary>
        public bool SkyVisible
        {
            get { return skyvisible; }
            set { skyvisible = value; }
        }


        private GameLibrary.Area area;

        /// <summary>
        /// Area that the room is a member of
        /// </summary>
        public GameLibrary.Area Area
        {
            get { return area; }
            set { area = value; }
        }



        /// <summary>
        /// Gets a player that is in the room.
        /// </summary>
        /// <param name="playername"> The name of the player to get.</param>
        /// <returns>A player.</returns>
        public PlayerCharacter GetPlayerInRoom(string playername)
        {
            foreach (PlayerCharacter player in players)
            {
                if (String.Compare(playername, player.NameFirst, true) == 0)
                {
                    return player;
                }

                if (String.Compare(playername, player.NameShort, true) == 0)
                {
                    return player;
                }

                if (String.Compare(playername, player.NameDisplay, true) == 0)
                {
                    return player;
                }

                if (String.Compare(playername, player.NameLast, true) == 0)
                {
                    return player;
                }
            }
            return null;
        }


        /// <summary>
        /// Gets a list of players in the room.
        /// </summary>
        /// <returns></returns>
        public List<PlayerCharacter> GetPlayers()
        {
            List<PlayerCharacter> tempplayers = new List<PlayerCharacter>();
            tempplayers = players;
            return tempplayers;
        }

        /// <summary>
        /// Adds a player to the room.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Exception AddPlayer(PlayerCharacter player)
        {
            try
            {
                players.Add(player);
                return null;
            }
            catch (Exception ex) { return ex; }

        }

        /// <summary>
        /// Removes a player from the room.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Exception RemovePlayer(PlayerCharacter player)
        {
            try
            {
                players.Remove(player);
                return null;
            }
            catch (Exception ex) { return ex; }

        }

       
     

        public delegate void EnterRoomEventHandler(object room, RoomEventArgs eventargs);

        /// <summary>
        /// Triggered when a player enters a room.
        /// </summary>
        public event EnterRoomEventHandler EnterRoom;

        protected void OnEnterRoom(object room, RoomEventArgs eventargs)
        {
            if (EnterRoom != null) EnterRoom(room, eventargs);
        }

        public delegate void LeaveRoomEventHandler(object room, RoomEventArgs eventargs);

        /// <summary>
        /// Triggered when a player leaves a room.
        /// </summary>
        public event LeaveRoomEventHandler LeaveRoom;

        protected void OnLeaveRoom(object room, RoomEventArgs eventargs)
        {
            if (LeaveRoom != null) LeaveRoom(room, eventargs);
        }






    }
}