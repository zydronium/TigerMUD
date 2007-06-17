using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public struct MapResource
    {
        private byte type;

        public byte Type
        {
            get { return type; }
            set { type = value; }
        }
        private byte concentration;

        public byte Concentration
        {
            get { return concentration; }
            set { concentration = value; }
        }
        private byte quality;

        public byte Quality
        {
            get { return quality; }
            set { quality = value; }
        }

        private string symbol;

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        private string locationmessage;

        public string LocationMessage
        {
            get { return locationmessage; }
            set { locationmessage = value; }
        }

        private string locationlink;

        public string LocationLink
        {
            get { return locationlink; }
            set { locationlink = value; }
        }
	



    }
}
