using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections;
using System.Threading;


namespace GameLibrary
{
    /// <summary>
    /// Summary description for Profession
    /// </summary>
    public class Profession : GameLibrary.GameObject, IDisposable
    {
        object lockobj;
        /// <summary>
        /// Represents a group of skills, spells, and/or effects that are collectively called a profession.
        /// </summary>
        public Profession()
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
        ~Profession()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }



        ArrayList skills;
        ArrayList spells;
        ArrayList effects;


    }
}