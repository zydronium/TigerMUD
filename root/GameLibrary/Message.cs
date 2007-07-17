using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents an in-game email sent to a player.
    /// </summary>
    public struct Message
    {
        private string to;

        public string To
        {
            get { return to; }
            set { to = value; }
        }

        private string from;

        public string From
        {
            get { return from; }
            set { from = value; }
        }

        private string cc;

        public string CC
        {
            get { return cc; }
            set { cc = value; }
        }

        private string subject;

        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        private string body;

        public string Body
        {
            get { return body; }
            set { body = value; }
        }


        private Item attachment;

        public Item Attachment
        {
            get { return attachment; }
            set { attachment = value; }
        }
	
	
	
       
        

    }
}
