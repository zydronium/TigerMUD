using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public struct CharacterList
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string nameFirst;

        public string NameFirst
        {
            get { return nameFirst; }
            set { nameFirst = value; }
        }

        private string nameLast;

        public string NameLast
        {
            get { return nameLast; }
            set { nameLast = value; }
        }

        private int level;

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        private string characterClass;

        public string CharacterClass
        {
            get { return characterClass; }
            set { characterClass = value; }
        }
    }

}
