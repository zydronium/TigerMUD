using System;
using System.Collections;
using System.Text;
using System.Data.OleDb;

namespace TigerMUD
{
    enum ObjectiveType
    {
        NONE = 0,
        TAKE,
        GIVE,
        KILL
    }

    enum ObjectType
    { 
        TREASURE = 0,
    }

    enum ObjectiveState
    {
        PENDING = 0,
        DONE
    }

    public class Objective
    {
        Actor mOwner;
        String mName;
        ObjectiveType mType;
        ObjectiveState mState;
    }

    public abstract class StoryFragment
    {
        public char FragmentCode;

        public abstract void TellStory();
        

    }

    /// <summary>
    ///  Temporal-spatial determination
    /// </summary>
    public class StoryInitOne:StoryFragment
    {        
        //String Component;
        ArrayList Components;

        StoryInitOne()
        {
            this.FragmentCode = 'a'; 
        }

        override public void TellStory()
        { }

    }

    /// <summary>
    /// Contains components that belong to a story
    /// </summary>
    public class Theme
    {
        ArrayList Components;
    }

    public class Story
    {
        public Actor ActorOwner;
        public ArrayList StoryFragments;
        public String StorySummary;
        


    }

    public class StoryManager
    {

        Story[] StoryList;

        public StoryManager()
        {
            //create the database connection
            string filename = Lib.PathtoRoot + "story.mdb";
            OleDbConnection aConnection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename);

            //create the command object and store the sql query
            OleDbCommand aCommand = new OleDbCommand("select * from story", aConnection);
            try
            {
                aConnection.Open();

                //create the datareader object to connect to table
                OleDbDataReader aReader = aCommand.ExecuteReader();
                Console.WriteLine("This is the returned data from story table");

                //Iterate throuth the database
                while (aReader.Read())
                {
                    
                    Console.WriteLine(aReader.GetString(1).ToString());
                }

                //close the reader 
                aReader.Close();

                //close the connection Its important.
                aConnection.Close();
            }

            //Some usual exception handling
            catch (OleDbException e)
            {
                Console.WriteLine("Error: {0}", e.Errors[0].Message);
            }
            
        }

        // evaluate current story and detrmine the step to be taken
        void RetrieveNextStep(Story story) { 
        
        
        }

        public void StoreStory()
        { 

        
        }

        void RetrieveStory()
        { 
            
        }

        Story AddStoryFragment(StoryFragment fragment, Story story)
        {
            story.StoryFragments.Add(fragment);
            story.StorySummary = story.StorySummary + fragment.FragmentCode;

            return story;
        }

        Story GetStoryByActor(string actorName)
        { 
            foreach (Story s in StoryList)
            {
                if (s.ActorOwner["name"].Equals(actorName))
                    return s;
            }

            return null;
        }
       
    }
}
