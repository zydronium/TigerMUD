using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a player account that may contain one or more player characters.
    /// </summary>
    public class PlayerAccount : GameObject
    {
        public PlayerAccount()
        {
            this.Id = Guid.NewGuid().ToString();
        }
               
        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private DateTime datecreated;

        public DateTime DateCreated
        {
            get { return datecreated; }
            set { datecreated = value; }
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }

        private bool banned;

        public bool Banned
        {
            get { return banned; }
            set { banned = value; }
        }

        private DateTime datelastlogin;

        public DateTime DateLastLogin
        {
            get { return datelastlogin; }
            set { datelastlogin = value; }
        }


        private string email;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

     
        ArrayList playercharacters;

        private bool connected;

        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }
	
	
    }
}
