using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;

namespace GameLibrary
{
    public class Exit : GameLibrary.GameObject, IDisposable
    {
        private object lockobj;

        List<Key> keys = new List<Key>();
       
        
        /// <summary>
        /// Represents an exit from a room or area.
        /// </summary>
        public Exit()
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
        ~Exit()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        /// <summary>
        /// Adds a key that can unlock this exit.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Exception AddKey(Key key)
        {
            try
            {
                keys.Add(key);
                return null;

            }
            catch (Exception ex)
            {
                return ex;
            }
        }


        private Room destinationroom;

        public Room DestinationRoom
        {
            get { return destinationroom; }
            set { destinationroom = value; }
        }

      

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }




        private bool hidden;

        public bool Hidden
        {
            get { return hidden; }
            set { hidden = value; }
        }

        private bool magic;

        public bool Magic
        {
            get { return magic; }
            set { magic = value; }
        }

    	

    }
}
