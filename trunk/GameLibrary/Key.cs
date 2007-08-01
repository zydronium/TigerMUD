using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GameLibrary
{
   
    
    
    public class Key : GameLibrary.GameObject, IDisposable
    {

        /// <summary>
        /// Rooms that this key can open.
        /// </summary>
        Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        object lockobj;

         /// <summary>
        /// Represents a room that players and npcs can move through.
        /// </summary>
        public Key()
        {
            Id = Guid.NewGuid().ToString();
            lockobj = new object();


            //
            // TODO: Add constructor logic here
            //
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
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
        ~Key()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

    }
}
