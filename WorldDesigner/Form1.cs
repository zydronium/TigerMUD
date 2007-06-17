using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.XPath;

namespace WorldDesigner
{
    public partial class Form1 : Form
    {
        GameLibrary.Planet planet;
        Graphics offscreenbuffer;
        Color color;
        SolidBrush brush;
        int startmousex;
        int startmousey;
        int startrectanglex;
        int startrectangley;
        GameLibrary.MapResource resource;
        Pen pen;
        bool rectangleinprogress;
        DataTable resourcetable;
        DataRow row;
        Dictionary<byte, Color> colors = new Dictionary<byte, Color>();
        Color undefinedcolor = Color.Yellow;// set any unknown color to bright pink
        int scrolloffsetx;
        int scrolloffsety;


        Bitmap offscreenbitmap;
        Graphics clientdisplay;

        Bitmap tempbitmap;

        public enum DrawMode { None, Point, FatPoint, GiantBlock, Rectangle };

        public DrawMode drawmode = DrawMode.None;

        public Form1()
        {
            InitializeComponent();

            color = new Color();
            brush = new SolidBrush(color);
            tempbitmap = new Bitmap(1, 1);
            resource = new GameLibrary.MapResource();
            pen = new Pen(new Color());
            resourcetable = new DataTable();

            resourcetable.Columns.Add("id", typeof(System.Int32));
            resourcetable.Columns.Add("displayname", typeof(System.String));
            resourcetable.Columns.Add("symbol", typeof(System.String));

            ReadConfig();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            planet.NameDisplay = textBox4.Text;
            planet.Description = textBox5.Text;
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Filter = "Planet Files (*.planet)|*.planet";
            DialogResult result = savedialog.ShowDialog();
            if (result.ToString().Equals("Cancel")) return;
            WriteFile(savedialog.FileName);

        }

        private void button1_Click(object sender, EventArgs e)
        {

            // Load planet file
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Planet Files (*.planet)|*.planet";
            DialogResult result = dialog.ShowDialog();
            if (result.ToString().Equals("Cancel")) return;

            planet = GameLibrary.Planet.LoadPlanetFromFile(dialog.FileName);

            textBox1.Text = planet.Id;
            textBox6.Text = planet.Width.ToString();
            textBox7.Text = planet.Height.ToString();
            textBox4.Text = planet.NameDisplay;
            textBox5.Text = planet.Description;

            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
            textBox5.Enabled = true;
            textBox6.Enabled = true;
            textBox7.Enabled = true;
            textBox8.Enabled = true;

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;

            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
            radioButton4.Enabled = true;
            radioButton5.Enabled = true;


            comboBox2.Enabled = true;

            offscreenbitmap = new Bitmap(planet.Width, planet.Height);
            offscreenbuffer = Graphics.FromImage(offscreenbitmap);
            clientdisplay = panel1.CreateGraphics();

            brush.Color = Color.Gray;
            clientdisplay.FillRectangle(brush, 0, 0, panel1.Width - 1, panel1.Height - 1);
            DrawMap(planet, scrolloffsetx, scrolloffsety);
        }

        private void DrawMap(GameLibrary.Planet planet, int scrolloffsetx, int scrolloffsety)
        {


            for (int heightcounter = 0; heightcounter < planet.Height; heightcounter++)
            {
                for (int widthcounter = 0; widthcounter < planet.Width; widthcounter++)
                {
                    if (radioButton1.Checked) color = colors[planet.MapTerrain[widthcounter, heightcounter].Type];

                    if (radioButton1.Checked)
                    {
                        if (colors.ContainsKey(planet.MapTerrain[widthcounter, heightcounter].Type))
                        {
                            color = colors[planet.MapTerrain[widthcounter, heightcounter].Type];
                        }
                        else
                        {
                            //add the color so we can paint it
                            colors.Add(planet.MapTerrain[widthcounter, heightcounter].Type, undefinedcolor);
                            color = undefinedcolor;

                        }

                    }

                    if (radioButton2.Checked)
                    {
                        if (colors.ContainsKey(planet.MapFlora[widthcounter, heightcounter].Type))
                        {
                            color = colors[planet.MapFlora[widthcounter, heightcounter].Type];
                        }
                        else
                        {
                            //add the color so we can paint it
                            colors.Add(planet.MapFlora[widthcounter, heightcounter].Type, undefinedcolor);
                            color = undefinedcolor;

                        }

                    }

                    if (radioButton3.Checked)
                    {
                        if (colors.ContainsKey(planet.MapSpawn[widthcounter, heightcounter].Type))
                        {
                            color = colors[planet.MapSpawn[widthcounter, heightcounter].Type];
                        }
                        else
                        {
                            //add the color so we can paint it
                            colors.Add(planet.MapSpawn[widthcounter, heightcounter].Type, undefinedcolor);
                            color = undefinedcolor;

                        }

                    }

                    if (radioButton4.Checked)
                    {
                        if (colors.ContainsKey(planet.MapMinerals[widthcounter, heightcounter].Type))
                        {
                            color = colors[planet.MapMinerals[widthcounter, heightcounter].Type];
                        }
                        else
                        {
                            //add the color so we can paint it
                            colors.Add(planet.MapMinerals[widthcounter, heightcounter].Type, undefinedcolor);
                            color = undefinedcolor;

                        }

                    }


                    if (radioButton5.Checked)
                    {
                        if (colors.ContainsKey(planet.MapWeather[widthcounter, heightcounter].Type))
                        {
                            color = colors[planet.MapWeather[widthcounter, heightcounter].Type];
                        }
                        else
                        {
                            //add the color so we can paint it
                            colors.Add(planet.MapWeather[widthcounter, heightcounter].Type, undefinedcolor);
                            color = undefinedcolor;

                        }

                    }


                    // Draw each every point one at a time with the color from the planet map

                    brush.Color = color;
                    offscreenbuffer.FillRectangle(brush, widthcounter, heightcounter, 1, 1);

                    // This was slower actually
                    //tempbitmap.SetPixel(0,0,color);
                    //offscreenbuffer.DrawImageUnscaled(tempbitmap, widthcounter, heightcounter);


                }

            }

            // do drawing in offScreenDC 
            clientdisplay.DrawImage(offscreenbitmap, scrolloffsetx, scrolloffsety);
        }


        private void DrawMap(GameLibrary.Planet planet)
        {
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }



        private void WriteFile(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                writer.Write(textBox1.Text);
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

                        if (planet.MapTerrain[h, i].Symbol == null) writer.Write(" ");
                        else writer.Write(planet.MapTerrain[h, i].Symbol);
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                stream.Close();
                return;
            }
            stream.Close();
            return;


        }



        private void WriteTestFile(string filename)
        {
            // file format

            //x
            //y
            //planet name
            //planet description

            //terrain map resource
            //mineral map resource
            //flora map resource
            //spawn map resource
            //weather map resource
            //map symbol

            // This example below should create a 2x5 map that looks like:
            //AB
            //CD
            //EF
            //GH
            //IJ

            int x = 2;
            int y = 5;
            string planetname = "Test Planet";
            string planetdesc = "This is the description for the planet.";

            // Each map resource represents a spot on the map
            // Each map resource is 16 bytes (five 3 byte chunks and 1 byte for map symbol)
            // The number of these resources is X times Y

            // RGB = Type, Concentration, Quality
            byte[] mapresource1 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource2 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource3 = { 255,0,0,
               122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource4 = { 255,0,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource5 = { 0,0,255,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource6 = { 0,0,255,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource7 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource8 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource9 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};
            byte[] mapresource10 = { 0,255,0,
                122,122,122,
                144,144,144,
                155,155,155,
                166,166,166};

            string symbol1 = "a";

            string symbol2 = "b";
            string symbol3 = "c";
            string symbol4 = "d";
            string symbol5 = "e";
            string symbol6 = "f";
            string symbol7 = "g";
            string symbol8 = "h";
            string symbol9 = "i";
            string symbol10 = "j";

            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                writer.Write(x);
                writer.Write(y);
                writer.Write(planetname);
                writer.Write(planetdesc);
                writer.Write(mapresource1);
                writer.Write(symbol1);
                writer.Write(mapresource2);
                writer.Write(symbol2);
                writer.Write(mapresource3);
                writer.Write(symbol3);
                writer.Write(mapresource4);
                writer.Write(symbol4);
                writer.Write(mapresource5);
                writer.Write(symbol5);
                writer.Write(mapresource6);
                writer.Write(symbol6);
                writer.Write(mapresource7);
                writer.Write(symbol7);
                writer.Write(mapresource8);
                writer.Write(symbol8);
                writer.Write(mapresource9);
                writer.Write(symbol9);
                writer.Write(mapresource10);
                writer.Write(symbol10);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                stream.Close();
                return;
            }
            stream.Close();
            return;
        }

        private GameLibrary.MapResource GetMapResource(int x, int y)
        {

            if (planet != null ? x < planet.Width && x >= 0 && y >= 0 && y < planet.Height : false)
            {
                if (!button2.Enabled) button2.Enabled = true;


                if (radioButton1.Checked) return planet.MapTerrain[x, y];
                if (radioButton2.Checked) return planet.MapFlora[x, y];
                if (radioButton3.Checked) return planet.MapSpawn[x, y];
                if (radioButton4.Checked) return planet.MapMinerals[x, y];
                if (radioButton5.Checked) return planet.MapWeather[x, y];
                return new GameLibrary.MapResource();

            }
            return new GameLibrary.MapResource();

        }

        private System.Array ResizeMap(int x, int y, System.Array oldmap)
        {
            // Check to ensure we are only growing the map, can't shrink arrays or we lost data
            // Get current dimension sizes
            int firstdimensionsize = oldmap.GetUpperBound(0);
            int seconddimensionsize = oldmap.GetUpperBound(1);

            // never shrink an array, only let it grow
            // is the array already big enough in both dimensions?
            if (firstdimensionsize > x && seconddimensionsize > y)
            {
                // if so, just return existing array and don't resize
                return oldmap;
            }

            // Get current map type
            System.Type elementType = oldmap.GetType().GetElementType();
            // Create the new array with the new size
            System.Array newmap = System.Array.CreateInstance(elementType, Math.Max(x, firstdimensionsize) + 1, Math.Max(y, seconddimensionsize) + 1);
            // Copy the old array into the new

            // Copy old array into new array with custom routine,
            // because Array.Copy doesn't preserve the true data positions after copy
            // Array.Copy would be good for empty arrays, but not here copying full arrays

            for (int b = 0; b <= seconddimensionsize; b++)
            {
                for (int a = 0; a <= firstdimensionsize; a++)
                {
                    newmap.SetValue(oldmap.GetValue(a, b), a, b);
                }

            }

            return newmap;


        }

        private void SetMapResource(int x, int y, GameLibrary.MapResource resource, GameLibrary.Planet planet)
        {
            // Correct for scrolling
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            // did user click outside current map size?
            if (x + 1 > planet.Width || y + 1 > planet.Height)
            {
                // Added a map resource outside the current bounds of the map.
                // So resize all the maps before we set the value.
                planet.MapTerrain = (GameLibrary.MapResource[,])ResizeMap(Math.Max(x + 1, planet.Width), Math.Max(y + 1, planet.Height), planet.MapTerrain);
                planet.MapFlora = (GameLibrary.MapResource[,])ResizeMap(Math.Max(x + 1, planet.Width), Math.Max(y + 1, planet.Height), planet.MapFlora);
                planet.MapSpawn = (GameLibrary.MapResource[,])ResizeMap(Math.Max(x + 1, planet.Width), Math.Max(y + 1, planet.Height), planet.MapSpawn);
                planet.MapMinerals = (GameLibrary.MapResource[,])ResizeMap(Math.Max(x + 1, planet.Width), Math.Max(y + 1, planet.Height), planet.MapMinerals);
                planet.MapWeather = (GameLibrary.MapResource[,])ResizeMap(Math.Max(x + 1, planet.Width), Math.Max(y + 1, planet.Height), planet.MapWeather);

                // also set new planet map size
                planet.Width = Math.Max(x + 1, planet.Width);
                planet.Height = Math.Max(y + 1, planet.Height);

                textBox6.Text = planet.Width.ToString();
                textBox7.Text = planet.Height.ToString();

                if (radioButton1.Checked) planet.MapTerrain[x, y] = resource;
                else if (radioButton2.Checked) planet.MapFlora[x, y] = resource;
                else if (radioButton3.Checked) planet.MapSpawn[x, y] = resource;
                else if (radioButton4.Checked) planet.MapMinerals[x, y] = resource;
                else if (radioButton5.Checked) planet.MapWeather[x, y] = resource;

                // resize the graphics backbuffer too

                offscreenbitmap = new Bitmap(planet.Width, planet.Height);
                offscreenbuffer = Graphics.FromImage(offscreenbitmap);

                // redraw the map

                DrawMap(planet, scrolloffsetx, scrolloffsety);

                return;

            }

            if (radioButton1.Checked) planet.MapTerrain[x, y] = resource;
            else if (radioButton2.Checked) planet.MapFlora[x, y] = resource;
            else if (radioButton3.Checked) planet.MapSpawn[x, y] = resource;
            else if (radioButton4.Checked) planet.MapMinerals[x, y] = resource;
            else if (radioButton5.Checked) planet.MapWeather[x, y] = resource;

        }

        private void ValidateByteInput(TextBox textbox)
        {
            int number;
            // is it an integer?
            if (Int32.TryParse(textbox.Text, out number))
            {
                // its a number, now check the range
                if (number >= 0 && number < 256)
                {
                    // all is well so bail out here
                    return;
                }
                // set to 255
                textbox.Text = "255";
                return;
            }
            // not even a number so clear it
            textbox.Text = String.Empty;

        }


        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {
            if (planet != null) DrawMap(planet, scrolloffsetx, scrolloffsety);

        }



        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (planet == null) return;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (planet == null) return;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (planet == null) return;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (planet == null) return;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (planet == null) return;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }

        private void panel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = (e.X - scrolloffsetx) + " , " + (e.Y - scrolloffsety);
            //groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;

            //if (comboBox1.SelectedItem.ToString() == "Point")
            if (drawmode == DrawMode.Point)
            {
                DrawPoint(e);
                return;

            }

            //if (comboBox1.SelectedItem.ToString() == "Fat Point")
            if (drawmode == DrawMode.FatPoint)
            {
                DrawFatPoint(e);
                return;

            }

            //if (comboBox1.SelectedItem.ToString() == "Giant Block")
            if (drawmode == DrawMode.GiantBlock)
            {
                DrawGiantBlock(e);
                return;

            }



            //if (comboBox1.SelectedItem.ToString() == "Rectangle")
            if (drawmode == DrawMode.Rectangle)
            {
                if (!rectangleinprogress)
                {
                    //startmousex = e.X;
                    //startmousey = e.Y;

                    startmousex = e.X;
                    startmousey = e.Y;
                }
                else
                {
                    // Either complete the rectangle on left click or abort on a right click.
                    if (e.Button != MouseButtons.Right)
                    {
                        brush.Color = pen.Color;
                        offscreenbuffer.FillRectangle(brush, startmousex, startmousey, Math.Max(1, e.X - startmousex), Math.Max(1, e.Y - startmousey));
                        for (int h = startmousey; h <= e.Y; h++)
                        {
                            for (int w = startmousex; w <= e.X; w++)
                            {
                                SetMapResource(w - scrolloffsetx, h - scrolloffsety, resource, planet);
                            }

                        }
                    }

                }


                // Right button click here, so clear rectangle work and abort
                rectangleinprogress = !rectangleinprogress;
                //clientdisplay.FillRectangle(brush, startmousex+ scrolloffsetx, startmousey + scrolloffsety, Math.Max(1, e.X - startmousex + scrolloffsetx), Math.Max(1, e.Y - startmousey + scrolloffsety));
                //clientdisplay.FillRectangle(brush, startmousex, startmousey, Math.Max(1, e.X- startmousex), Math.Max(1, e.Y - startmousey));
                return;
            }



            comboBox2.SelectedValue = GetMapResource(e.X - scrolloffsetx, e.Y - scrolloffsety).Type.ToString();
            textBox2.Text = GetMapResource(e.X - scrolloffsetx, e.Y - scrolloffsety).Concentration.ToString();
            textBox3.Text = GetMapResource(e.X - scrolloffsetx, e.Y - scrolloffsety).Quality.ToString();
            textBox8.Text = GetMapResource(e.X - scrolloffsetx, e.Y - scrolloffsety).Symbol;

        }



        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = (e.X - scrolloffsetx) + " , " + (e.Y - scrolloffsety);
            //groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;

            //if (comboBox1.SelectedItem.ToString() == "None" && !rectangleinprogress)
            if (drawmode == DrawMode.None && !rectangleinprogress)
            {
                return;
            }

            //if (comboBox1.SelectedItem.ToString() == "Point" && e.Button == MouseButtons.Left)
            if (drawmode == DrawMode.Point && e.Button == MouseButtons.Left)
            {

                DrawPoint(e);
                return;

            }

            //if (comboBox1.SelectedItem.ToString() == "Fat Point" && e.Button == MouseButtons.Left)
            if (drawmode == DrawMode.FatPoint && e.Button == MouseButtons.Left)
            {
                DrawFatPoint(e);
                return;
            }

            //if (comboBox1.SelectedItem.ToString() == "Giant Block" && e.Button == MouseButtons.Left)
            if (drawmode == DrawMode.GiantBlock && e.Button == MouseButtons.Left)
            {
                DrawGiantBlock(e);
                return;

            }

            //if (comboBox1.SelectedItem.ToString() == "Rectangle" && rectangleinprogress)
            if (drawmode == DrawMode.Rectangle && rectangleinprogress)
            {
                DrawMap(planet, scrolloffsetx, scrolloffsety);

                // now paint new rectangle
                resource.Type = Convert.ToByte(comboBox2.SelectedValue);
                resource.Concentration = Convert.ToByte(textBox2.Text);
                resource.Quality = Convert.ToByte(textBox3.Text);
                resource.Symbol = textBox8.Text;

                color = colors[resource.Type];

                brush.Color = color;
                pen.Color = color;
                clientdisplay.DrawRectangle(pen, startmousex, startmousey, Math.Max(1, e.X - startmousex), Math.Max(1, e.Y - startmousey));
                // remember new rectangle to erase later if needed
                startrectanglex = startmousex + scrolloffsetx;
                startrectangley = startmousey + scrolloffsety;

            }

        }

        private void DrawPoint(MouseEventArgs e)
        {


            resource.Type = Convert.ToByte(comboBox2.SelectedValue);
            resource.Concentration = Convert.ToByte(textBox2.Text);
            resource.Quality = Convert.ToByte(textBox3.Text);
            resource.Symbol = textBox8.Text;
            color = colors[resource.Type];
            brush.Color = color;
            offscreenbuffer.FillRectangle(brush, e.X, e.Y, 1, 1);
            clientdisplay.FillRectangle(brush, e.X, e.Y, 1, 1);
            SetMapResource(e.X - scrolloffsetx, e.Y - scrolloffsety, resource, planet);

        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            //if (comboBox1.SelectedItem.ToString() == "Rectangle")
            if (drawmode == DrawMode.Rectangle)
            {
                startrectanglex = 0;
                startrectangley = 0;

                //startrectanglex = e.X;
                //startrectangley = e.Y;

            }

            DrawMap(planet, scrolloffsetx, scrolloffsety);
        }



        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ValidateByteInput(textBox2);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ValidateByteInput(textBox3);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            WriteTestFile("test.planet");
        }

        private void ReadConfig()
        {
            int resourceid;
            string resourcesymbol = string.Empty;
            string resourcevalue = string.Empty;
            XPathDocument xdoc = new XPathDocument("resources.xml");
            XPathNavigator xnav = xdoc.CreateNavigator();
            XPathNodeIterator nodes = xnav.Select("/mapresources/terrain/type");



            while (nodes.MoveNext())
            {
                resourceid = Convert.ToInt32(nodes.Current.GetAttribute("id", string.Empty));
                resourcesymbol = nodes.Current.GetAttribute("symbol", string.Empty);

                // add map colors to the dictionary for fast access in the draw loop
                // id is byte for speed avoiding a conversion in the loop
                colors.Add(Convert.ToByte(resourceid), Color.FromArgb(Convert.ToByte(nodes.Current.GetAttribute("red", string.Empty)), Convert.ToByte(nodes.Current.GetAttribute("green", string.Empty)), Convert.ToByte(nodes.Current.GetAttribute("blue", string.Empty))));

                resourcevalue = nodes.Current.Value;
                row = resourcetable.NewRow();

                row["id"] = Convert.ToInt32(resourceid);
                row["displayname"] = resourcevalue;
                row["symbol"] = resourcesymbol;

                resourcetable.Rows.Add(row);

            }
            resourcetable.PrimaryKey = new DataColumn[] { resourcetable.Columns[0] };

            comboBox2.DataSource = resourcetable;
            comboBox2.DisplayMember = "displayname";
            comboBox2.ValueMember = "id";
            comboBox2.SelectedValue = 0;


        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBox2.Enabled) return;
            DataRow row = resourcetable.Rows.Find(comboBox2.SelectedValue);
            if (row != null) textBox8.Text = row["symbol"].ToString();


        }

        private void DrawFatPoint(MouseEventArgs e)
        {
            resource.Type = Convert.ToByte(comboBox2.SelectedValue);
            resource.Concentration = Convert.ToByte(textBox2.Text);
            resource.Quality = Convert.ToByte(textBox3.Text);
            resource.Symbol = textBox8.Text;
            color = colors[resource.Type];
            brush.Color = color;
            offscreenbuffer.FillRectangle(brush, Math.Max(0, e.X - 2), Math.Max(0, e.Y - 2), 5, 5);
            clientdisplay.FillRectangle(brush, Math.Max(0, e.X - 2), Math.Max(0, e.Y - 2), 5, 5);


            for (int h = Math.Max(0, e.Y - 2); h <= e.Y + 2; h++)
            {
                for (int w = Math.Max(0, e.X - 2); w <= e.X + 2; w++)
                {
                    SetMapResource(w - scrolloffsetx, h - scrolloffsety, resource, planet);

                }
            }
        }

        private void DrawGiantBlock(MouseEventArgs e)
        {
            resource.Type = Convert.ToByte(comboBox2.SelectedValue);
            resource.Concentration = Convert.ToByte(textBox2.Text);
            resource.Quality = Convert.ToByte(textBox3.Text);
            resource.Symbol = textBox8.Text;
            color = colors[resource.Type];
            brush.Color = color;

            offscreenbuffer.FillRectangle(brush, Math.Max(0, e.X - 5), Math.Max(0, e.Y - 5), 11, 11);
            clientdisplay.FillRectangle(brush, Math.Max(0, e.X - 5), Math.Max(0, e.Y - 5), 11, 11);

            for (int h = Math.Max(0, e.Y - 5); h <= e.Y + 5; h++)
            {
                for (int w = Math.Max(0, e.X - 5); w <= e.X + 5; w++)
                {
                    SetMapResource(w - scrolloffsetx, h - scrolloffsety, resource, planet);

                }
            }
            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            scrolloffsetx -= 15;
            //if (scrolloffsetx < 0) scrolloffsetx = 0;
            ClearPanel();
            groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;
            DrawMap(planet, scrolloffsetx, scrolloffsety);


        }

        private void button5_Click(object sender, EventArgs e)
        {

            scrolloffsety -= 15;
            //if (scrolloffsety < 0) scrolloffsety = 0;
            ClearPanel();
            groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;
            DrawMap(planet, scrolloffsetx, scrolloffsety);



        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (scrolloffsetx >= 0) return;

            scrolloffsetx += 15;
            if (scrolloffsetx > planet.Width) scrolloffsetx = planet.Width;
            ClearPanel();
            groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;
            DrawMap(planet, scrolloffsetx, scrolloffsety);

        }



        private void button3_Click(object sender, EventArgs e)
        {
            if (scrolloffsety >= 0) return;

            scrolloffsety += 15;
            if (scrolloffsety > planet.Height) scrolloffsety = planet.Height;
            ClearPanel();
            groupBox4.Text = scrolloffsetx + "   " + scrolloffsety;
            DrawMap(planet, scrolloffsetx, scrolloffsety);


        }

        private void ClearPanel()
        {

            brush.Color = Color.Gray;
            clientdisplay.FillRectangle(brush, 0, 0, panel1.Width - 1, panel1.Height - 1);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (comboBox1.SelectedItem.ToString() == "None")
            if (drawmode == DrawMode.None)
            {
                panel1.Cursor = System.Windows.Forms.Cursors.Default;

            }
            //if (comboBox1.SelectedItem.ToString() == "Point")
            if (drawmode == DrawMode.Point)
            {
                Bitmap b = new Bitmap(1, 1);
                Graphics g = Graphics.FromImage(b);
                Pen pen = new Pen(Color.Purple);
                // do whatever you wish
                //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
                g.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
                IntPtr ptr = b.GetHicon();
                panel1.Cursor = new Cursor(ptr);
            }

            //if (comboBox1.SelectedItem.ToString() == "Fat Point")
            if (drawmode == DrawMode.FatPoint)
            {
                Bitmap b = new Bitmap(5, 5);
                Graphics g = Graphics.FromImage(b);
                Brush brush = new SolidBrush(Color.Purple);
                // do whatever you wish
                //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
                g.FillRectangle(brush, new Rectangle(0, 0, 5, 5));
                IntPtr ptr = b.GetHicon();
                panel1.Cursor = new Cursor(ptr);

            }

            //if (comboBox1.SelectedItem.ToString() == "Giant Block")
            if (drawmode == DrawMode.GiantBlock)
            {
                Bitmap b = new Bitmap(11, 11);
                Graphics g = Graphics.FromImage(b);
                Brush brush = new SolidBrush(Color.Purple);
                // do whatever you wish
                //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
                g.FillRectangle(brush, new Rectangle(0, 0, 11, 11));
                IntPtr ptr = b.GetHicon();
                panel1.Cursor = new Cursor(ptr);

            }

            //if (comboBox1.SelectedItem.ToString() == "Rectangle")
            if (drawmode == DrawMode.Rectangle)
            {
                panel1.Cursor = System.Windows.Forms.Cursors.Cross;

            }

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            drawmode = DrawMode.None;
            panel1.Cursor = System.Windows.Forms.Cursors.Cross;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            drawmode = DrawMode.Point;
            Bitmap b = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(b);
            Pen pen = new Pen(Color.Purple);
            // do whatever you wish
            //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
            g.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
            IntPtr ptr = b.GetHicon();
            panel1.Cursor = new Cursor(ptr);
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            drawmode = DrawMode.FatPoint;

            Bitmap b = new Bitmap(5, 5);
            Graphics g = Graphics.FromImage(b);
            Brush brush = new SolidBrush(Color.Purple);
            // do whatever you wish
            //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
            g.FillRectangle(brush, new Rectangle(0, 0, 5, 5));
            IntPtr ptr = b.GetHicon();
            panel1.Cursor = new Cursor(ptr);



        }


        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            drawmode = DrawMode.GiantBlock;

            Bitmap b = new Bitmap(11, 11);
            Graphics g = Graphics.FromImage(b);
            Brush brush = new SolidBrush(Color.Purple);
            // do whatever you wish
            //g.DrawString("myText", this.Font, Brushes.Blue, 0, 0);
            g.FillRectangle(brush, new Rectangle(0, 0, 11, 11));
            IntPtr ptr = b.GetHicon();
            panel1.Cursor = new Cursor(ptr);

        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            drawmode = DrawMode.Rectangle;
            panel1.Cursor = System.Windows.Forms.Cursors.Cross;
        }

      
      

       






    }
}