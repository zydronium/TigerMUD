using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TigerMUD
{
    public class SpellReader : TigerXMLReader
    {
        public SpellReader() : base() { }
        public SpellReader(string Filename) : base(Filename) { }

        public override Boolean Load(string filename)
        {
            Boolean endSpell = false;
            Spell newSpell= new Spell();

            mReader = new XmlTextReader(filename);
            
            mReader.Read();
            while (!mReader.EOF)
            {

                if (mReader.NodeType == XmlNodeType.Element)
                {
                    if (mReader.Name.ToUpper().Equals("SPELL"))
                    {
                        mReader.Read();
                        while (!endSpell)
                        {
                            //read room data
                            switch (mReader.NodeType)
                            {
                                case XmlNodeType.Element: // The node is an element.
                                    String stateName = mReader.Name;
                                    mReader.Read(); //read text inside tags
                                    newSpell[stateName] = mReader.Value;
                                    break;
                                case XmlNodeType.EndElement:

                                    if (mReader.Name.ToUpper().Equals("SPELL"))
                                        endSpell = true;
                                    mReader.Read(); // read elements
                                    break;

                            }
                            mReader.Read();


                        }

                        //TODO: Verify minimum contents
                        // Add default fields if not existing
                        //newRoom["type"] = "room";
                        //newRoom["equippable"] = false;
                        //newRoom["shortnameupper"] = "";
                        //newRoom["combatactive"] = false;


                        lock (Lib.spells.SyncRoot)
                        {
                            Lib.spells.Add(newSpell);
                        }

                        //prepare for next room
                        endSpell = false;
                        newSpell = new Spell();
                    }
                    else
                        mReader.Read();
                }
                else
                    mReader.Read();
            }

            return true;
        }
    }
}
