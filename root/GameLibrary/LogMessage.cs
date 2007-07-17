using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class LogMessage
    {

        private DateTime datetime;

        public DateTime DateTime
        {
            get { return datetime; }
            set { datetime = value; }
        }
	

        private string ip;

        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }


        private string charactername;

        public string CharacterName
        {
            get { return charactername; }
            set { charactername = value; }
        }
	

        private string messagetype;

        public string Type
        {
            get { return messagetype; }
            set { messagetype = value; }
        }

        private string messagetext;

        public string Text
        {
            get { return messagetext; }
            set { messagetext = value; }
        }
	
	
	
    }
}
