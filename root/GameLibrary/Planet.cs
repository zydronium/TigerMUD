using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;


namespace GameLibrary
{
    

    public class Planet : GameObject
    {
        private string[,] map = new string[500, 500];
        private string[,] tempmap = new string[50, 50];
        int counterx;
        int countery;
        string mapstring = string.Empty;

        public Planet()
        {
            NameDisplay = String.Empty;
            Description = String.Empty;
            

        }
        private MapResource[,] mapterrain;

        public MapResource[,] MapTerrain
        {
            get { return mapterrain; }
            set { mapterrain = value; }
        }


        private MapResource[,] mapminerals;

        public MapResource[,] MapMinerals
        {
            get { return mapminerals; }
            set { mapminerals = value; }
        }

        private MapResource[,] mapflora;

        public MapResource[,] MapFlora
        {
            get { return mapflora; }
            set { mapflora = value; }
        }

        private MapResource[,] mapspawn;

        public MapResource[,] MapSpawn
        {
            get { return mapspawn; }
            set { mapspawn = value; }
        }

        private MapResource[,] mapweather;

        public MapResource[,] MapWeather
        {
            get { return mapweather; }
            set { mapweather = value; }
        }

        public static void WritePlanetToFile(Planet planet,string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                //writer.Write(textBox1.Text);
                writer.Write(planet.Id);
                writer.Write(planet.Width);
                writer.Write(planet.Height);
                writer.Write(planet.NameDisplay);
                writer.Write(planet.Description);
                for (int i = 0; i < planet.Height; i++)
                {
                    for (int h = 0; h < planet.Width; h++)
                    {
                        writer.Write(planet.MapTerrain[h, i].Type);
                        writer.Write(planet.MapTerrain[h, i].Concentration);
                        writer.Write(planet.MapTerrain[h, i].Quality);

                        writer.Write(planet.MapFlora[h, i].Type);
                        writer.Write(planet.MapFlora[h, i].Concentration);
                        writer.Write(planet.MapFlora[h, i].Quality);

                        writer.Write(planet.MapSpawn[h, i].Type);
                        writer.Write(planet.MapSpawn[h, i].Concentration);
                        writer.Write(planet.MapSpawn[h, i].Quality);

                        writer.Write(planet.MapMinerals[h, i].Type);
                        writer.Write(planet.MapMinerals[h, i].Concentration);
                        writer.Write(planet.MapMinerals[h, i].Quality);

                        writer.Write(planet.MapWeather[h, i].Type);
                        writer.Write(planet.MapWeather[h, i].Concentration);
                        writer.Write(planet.MapWeather[h, i].Quality);

                        // BUG these three strings get an extra space in front for some reason
                        if (planet.MapTerrain[h, i].Symbol == null) writer.Write(" ");
                        else writer.Write(planet.MapTerrain[h, i].Symbol);

                        if (planet.MapTerrain[h, i].LocationMessage == null) writer.Write(" ");
                        else writer.Write(planet.MapTerrain[h, i].LocationMessage);

                        if (planet.MapTerrain[h, i].PortalLink == null) writer.Write(" ");
                        else writer.Write(planet.MapTerrain[h, i].PortalLink);
                    }

                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                stream.Close();
                return;
            }
            stream.Close();
            return;


        }


        public static GameLibrary.Planet ReadPlanetFromFile(string filename)
        {
            FileStream stream;
            Planet planet = new GameLibrary.Planet();
            try
            {
                stream = new FileStream(filename, FileMode.Open);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Cannot open the file. " + ex.Message);
                return null;
            }
          
            BinaryReader reader = new BinaryReader(stream);

            try
            {
                planet.Id = reader.ReadString();
                planet.Width = reader.ReadInt32();
                planet.Height = reader.ReadInt32();
                planet.NameDisplay = reader.ReadString();
                planet.Description = reader.ReadString();

                planet.MapTerrain = new GameLibrary.MapResource[planet.Width, planet.Height];
                planet.MapFlora = new GameLibrary.MapResource[planet.Width, planet.Height];
                planet.MapSpawn = new GameLibrary.MapResource[planet.Width, planet.Height];
                planet.MapMinerals = new GameLibrary.MapResource[planet.Width, planet.Height];
                planet.MapWeather = new GameLibrary.MapResource[planet.Width, planet.Height];

                for (int heightcounter = 0; heightcounter < planet.Height; heightcounter++)
                {
                    for (int widthcounter = 0; widthcounter < planet.Width; widthcounter++)
                    {
                        planet.MapTerrain[widthcounter, heightcounter].Type = reader.ReadByte();
                        planet.MapTerrain[widthcounter, heightcounter].Concentration = reader.ReadByte();
                        planet.MapTerrain[widthcounter, heightcounter].Quality = reader.ReadByte();

                        planet.MapFlora[widthcounter, heightcounter].Type = reader.ReadByte();
                        planet.MapFlora[widthcounter, heightcounter].Concentration = reader.ReadByte();
                        planet.MapFlora[widthcounter, heightcounter].Quality = reader.ReadByte();

                        planet.MapSpawn[widthcounter, heightcounter].Type = reader.ReadByte();
                        planet.MapSpawn[widthcounter, heightcounter].Concentration = reader.ReadByte();
                        planet.MapSpawn[widthcounter, heightcounter].Quality = reader.ReadByte();

                        planet.MapMinerals[widthcounter, heightcounter].Type = reader.ReadByte();
                        planet.MapMinerals[widthcounter, heightcounter].Concentration = reader.ReadByte();
                        planet.MapMinerals[widthcounter, heightcounter].Quality = reader.ReadByte();

                        planet.MapWeather[widthcounter, heightcounter].Type = reader.ReadByte();
                        planet.MapWeather[widthcounter, heightcounter].Concentration = reader.ReadByte();
                        planet.MapWeather[widthcounter, heightcounter].Quality = reader.ReadByte();

                        
                        planet.MapTerrain[widthcounter, heightcounter].Symbol = reader.ReadString();
                        planet.MapTerrain[widthcounter, heightcounter].LocationMessage = reader.ReadString();
                        planet.MapTerrain[widthcounter, heightcounter].PortalLink = reader.ReadString();

                        if (widthcounter == 10 && heightcounter == 10) Console.WriteLine("'" + planet.MapTerrain[widthcounter, heightcounter].LocationMessage + "'");
                        if (widthcounter == 10 && heightcounter == 10) Console.WriteLine("'" + planet.MapTerrain[widthcounter, heightcounter].PortalLink + "'");
                    }

                }
            }
            catch 
            {
                stream.Close();
                return null;
            }



            stream.Close();

            return planet;

        }



        public bool Load(string planetpath)
        {
            LoadMap(planetpath);

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool LoadMap(string planetpath)
        {
            Bitmap bitmap;
            try
            {
                bitmap = LoadBMPfromFile(planetpath);
                this.Width = bitmap.Width;
                this.Height = bitmap.Height;
                this.MapTerrain = ConvertBMPtoByteArray(bitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Cannot load map for planet " + planetpath + ". Error was: " + ex.StackTrace);
                return false;
            }
            return true;
            
        }


        /// <summary>
        /// Populates a map with the given resources.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="xindex"></param>
        /// <param name="xcount"></param>
        /// <param name="yindex"></param>
        /// <param name="ycount"></param>
        /// <param name="mapresource"></param>
        public void InitMap(MapResource[,] array, int xindex, int xcount, int yindex, int ycount, MapResource mapresource)
        {

            for (int y = yindex; y < yindex + ycount; y++)
            {
                for (int x = xindex; x < xindex + xcount; x++) array[x,y] = mapresource;
            }

        }


        /// <summary>
        /// Returns the symbol map that displays to the client.
        /// </summary>
        /// <param name="locationx"></param>
        /// <param name="locationy"></param>
        /// <param name="rangewidth"></param>
        /// <param name="rangeheight"></param>
        /// <returns></returns>
        public string GetMap(int locationx, int locationy, int rangewidth, int rangeheight)
        {

            for (int y = locationy - rangeheight; y < locationy + rangeheight; y++)
            {
                countery++;
                for (int x = locationx - rangewidth; x < locationx + rangewidth; x++)
                {
                    tempmap[counterx, countery] = map[x, y];
                    if (x < 0 || x > 500 || y < 0 || y > 500)
                        mapstring += " ";
                    else
                        mapstring += map[x, y];
                    
                    counterx++;
                }
                counterx = 0;
            }
            return mapstring;
        }

        /// <summary>
        /// Convert a BMP to a 2D array of MapResources
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Array of MapResource structures</returns>
        public MapResource[,] ConvertBMPtoByteArray(System.Drawing.Bitmap bitmap)
        {
            if (bitmap==null) throw new ArgumentNullException("bitmap");

            MapResource[,] mapResource=new MapResource[bitmap.Width,bitmap.Height];
           
            Color color;
                                 
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    color = bitmap.GetPixel(x, y);
                    
                    mapResource[x,y].Type = color.R;
                    mapResource[x, y].Concentration = color.G;
                    mapResource[x, y].Quality = color.B;
                }

            }

            return mapResource;
                       
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public Bitmap LoadBMPfromFile(string filepath)
        {
            try
            {
                FileStream file = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return new System.Drawing.Bitmap(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:Could not load file. Error was: " + ex.StackTrace);
                return null;
            }

        }
   
        private int width;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        private string desc;

        public string Description
        {
            get { return desc; }
            set { desc = value; }
        }
	

      



    }
}
