using System;
using System.Collections;
using System.Text;
using System.Xml;
using TigerMUD;

namespace XMLLevelEditors
{
    class LevelReader
    {
        XmlTextReader mReader; 

        public LevelReader(string Filename)
        {
            mReader = new XmlTextReader(Filename);
        }

        public void LoadLevel()
        {
            while (mReader.Read())
            {
                switch (mReader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        Console.Write("<" + mReader.Name);
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(mReader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        Console.Write("</" + mReader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
        }
    }
}
