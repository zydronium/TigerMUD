using System;
using System.Collections;
using System.Text;
using System.Xml;

namespace TigerMUD
{
    public class LevelReader
    {
        XmlTextReader mReader; 

        public LevelReader(string Filename)
        {
            mReader = new XmlTextReader(Filename);
        }

        public LevelReader()
        {
            mReader = new XmlTextReader("");
        }

        public Boolean LoadLevel(string filename)
        {
            Boolean endRoom = false;
            Actor newRoom = new Actor();
           
            mReader = new XmlTextReader(filename);

            while (mReader.Read())
            {

                if (mReader.NodeType == XmlNodeType.Element)
                {
                    if (mReader.Name.ToUpper().Equals("ROOM"))
                    {
                        mReader.Read();
                        while(!endRoom)
                        {
                            //read room data
                            switch(mReader.NodeType)
                            {
                                case XmlNodeType.Element: // The node is an element.
                                    String stateName = mReader.Name;
                                    mReader.Read(); //read text inside tags
                                    newRoom[stateName] = mReader.Value;
                                    break;
                                case XmlNodeType.EndElement:
                                    
                                    if(mReader.Name.ToUpper().Equals("ROOM"))
                                        endRoom = true;
                                    mReader.Read(); // read elements
                                    break;
                                        
                            }
                            mReader.Read();     
                            
                            
                        }

                        //TODO: Verify minimum contents
                        // Add default fields if not existing
                        newRoom["type"] = "room";
                        newRoom["equippable"] = false;
                        newRoom["shortnameupper"] = "";
                        newRoom["combatactive"] = false;


                        lock (Lib.actors.SyncRoot)
                        {
                            Lib.actors.Add(newRoom);
                        }

                        //prepare for next room
                        endRoom = false;
                        newRoom = new Actor();
                    }
                }
            }

            return true;
        }
    }
}
