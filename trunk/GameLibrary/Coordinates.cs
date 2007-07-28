using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class Coordinates
    {
        public Coordinates()
        {
            X = 0;
            Y = 0;
        }


        public Coordinates(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }


        /// <summary>
        /// Convert coordinates from flat map to a sphere that wraps edges.
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Coordinates Wrap(Planet planet, Coordinates coordinates)
        {
            int displayx = coordinates.X;
            int displayy = coordinates.Y;

            if (coordinates.X < 0)
            {
                displayx = (planet.Width) + coordinates.X;
            }
            if (coordinates.X >= planet.Width)
            {
                displayx = coordinates.X - planet.Width;
            }
            if (coordinates.Y < 0)
            {
                displayy = (planet.Height) + coordinates.Y;
            }
            if (coordinates.Y >= planet.Height)
            {
                displayy = coordinates.Y - planet.Height;
            }

            coordinates.X = displayx;
            coordinates.Y = displayy;
            return coordinates;

        }


       

        private int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        private int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        
    }
}
