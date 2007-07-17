using System;
using GameLibrary;
using System.Text;

/// <summary>
/// Test command
/// </summary>
public class Map : GameLibrary.Command
{
    public Map()
    {
        this.Name = "map";
        this.Words.Add("map");
        this.Words.Add("m");
        this.Syntax = "map";
        this.Category = "navigation";
    }

    public override bool DoCommand(PlayerCharacter pc, GameContext gamecontext, string command, string arguments)
    {
        
        Planet planet=pc.Planet;
        MapResource[,] mapresource = planet.MapTerrain;
        int mapwidth = 21;
        int mapheight = 11;
        GameLibrary.Coordinates coordinates=new Coordinates();
        pc.SendLine("Planet {0}", planet.NameDisplay);
        pc.SendLine("planet width {0}, height {1}", planet.Width, planet.Height);
        pc.SendLine("current X {0}, current Y {1}", pc.X, pc.Y);
        StringBuilder maptext = new StringBuilder(mapwidth * mapheight + (mapheight *3));
        // Show terrain map for planet
        for (int y = pc.Y - mapheight; y < pc.Y + mapheight; y++)
        {
            for (int x = pc.X - mapwidth; x < pc.X + mapwidth; x++)
            {
                coordinates.X = x;
                coordinates.Y = y;
                coordinates=GameLibrary.Coordinates.Wrap(planet, coordinates);

                if (x == pc.X && y == pc.Y)
                {
                    // center of map where player is
                    maptext.Append("&WX&n");
                }
                else if (planet.MapTerrain[coordinates.X, coordinates.Y].Symbol == " " || planet.MapTerrain[coordinates.X, coordinates.Y].Symbol == "")
                    maptext.Append(".");
                else maptext.Append(planet.MapTerrain[coordinates.X, coordinates.Y].Symbol);
                
            }
            maptext.Append("\r\n");
            // This is how to properly handle the GetResponse method.
            // If a null is returned, then the client disconnected at the response prompt.
            // Return true in this case to tell the server to properly mark the client as disconnected.
            //if (pc.GetResponse()==null) return true;
        }
        pc.SendLine(maptext.ToString());

        return false;
    }

    private static void CoordinateConverter(Planet planet, ref int displayx, ref int displayy, int y, int x)
    {
        if (x < 0)
        {
            displayx = (planet.Width) + x;
        }
        if (x > planet.Width)
        {
            displayx = x - planet.Width;
        }
        if (y < 0)
        {
            displayy = (planet.Height) + y;
        }
        if (y > planet.Height)
        {
            displayy = y - planet.Height;
        }
    }
}