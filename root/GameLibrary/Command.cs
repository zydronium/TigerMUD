using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{

    /// <summary>
    /// Represents a command that a player would issue at the prompt.
    /// Game objects can associate commands with themselves to provide custom responses.
    /// </summary>
    public class Command
    {
        public virtual bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext,string command, string arguments)
        {
            // implement this in the real Command.
            return false;
        }

        // Overload used for queueing a command outside of ClientHandler itself
        public virtual void DoCommand(object stateinfoin)
        {

        }


        public Command()
        {

        }

        private bool repeatforever;

        public bool RepeatForever
        {
            get { return repeatforever; }
            set { repeatforever = value; }
        }
	

        private DateTime start;

        public DateTime Start
        {
            get { return start; }
            set { start = value; }
        }

        private DateTime end;

        public DateTime End
        {
            get { return end; }
            set { end = value; }
        }
        	

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //private string[] words;

        //public string[] Words
        //{
        //    get { return words; }
        //    set { words = value; }
        //}

        private List<string> words=new List<string>();

        public List<string> Words
        {
            get { return words; }
            set { words = value; }
        }
	
        
        private string syntax;

        public string Syntax
        {
            get { return syntax; }
            set { syntax = value; }
        }

        private string category;

        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        private TimeSpan duration=TimeSpan.Zero;

        public TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; }
        }
	
     
        private int count;

        /// <summary>
        /// When Repeatable=true, how often to run a repeatable command. -1 means forever.
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        private DateTime nexttrigger=DateTime.MinValue;

        public DateTime NextTrigger
        {
            get { return nexttrigger; }
            set { nexttrigger = value; }
        }

        private TimeSpan delay = TimeSpan.MinValue;

        /// <summary>
        /// Time between instances of triggering the command.
        /// </summary>
        public TimeSpan Delay
        {
            get { return delay; }
            set { delay = value; }
        }
	
	
    }
}
