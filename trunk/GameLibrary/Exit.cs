using System;
using System.Web;
using System.Threading;

namespace GameLibrary
{
    public class Exit : GameLibrary.GameObject, IDisposable
    {
        private object lockobj;

        
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


        private Room destination;

        public Room Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        private bool lockable;

        public bool Lockable
        {
            get { return lockable; }
            set { lockable = value; }
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }

        private string namedirection;

        public string NameDirection
        {
            get { return namedirection; }
            set { namedirection = value; }
        }


        private Room room;

        public Room Room
        {
            get { return room; }
            set { room = value; }
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

        private bool wilderness;

        public bool Wilderness
        {
            get { return wilderness; }
            set { wilderness = value; }
        }
	

	

    }
}
