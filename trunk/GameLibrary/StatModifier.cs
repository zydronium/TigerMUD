using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a condition that positively or negatively affects the value of a player or npc's defining statistics.
    /// </summary>
    public struct StatModifier
    {
        private string statName;

        public string StatName
        {
            get { return statName; }
            set { statName = value; }
        }
        private int modifier;

        public int Modifier
        {
            get { return modifier; }
            set { modifier = value; }
        }
    }
}
