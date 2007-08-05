using System;
using GameLibrary;

/// <summary>
/// Test command
/// </summary>
public class Movement : GameLibrary.Command
{
    public Movement()
    {
        this.Name = "movement";
        this.Words.Add("n");
        this.Words.Add("north");
        this.Words.Add("s");
        this.Words.Add("south");
        this.Words.Add("e");
        this.Words.Add("east");
        this.Words.Add("w");
        this.Words.Add("west");
        this.Words.Add("ne");
        this.Words.Add("northeast");
        this.Words.Add("se");
        this.Words.Add("southeast");
        this.Words.Add("sw");
        this.Words.Add("southwest");
        this.Words.Add("nw");
        this.Words.Add("northwest");
        this.Words.Add("u");
        this.Words.Add("up");
        this.Words.Add("d");
        this.Words.Add("down");
        this.Words.Add("i");
        this.Words.Add("in");
        this.Words.Add("o");
        this.Words.Add("out");
        this.Words.Add("enter");
        this.Words.Add("en");
        this.Words.Add("exit");
        this.Words.Add("ex");
        this.Syntax = "north";
        this.Category = "movement";
    }

    public override bool DoCommand(PlayerCharacter pc, GameContext gamecontext, string command, string arguments)
    {

        if (pc.InWilderness)
        {
            MoveInWilderness(pc, command, gamecontext);
        }
        else
        {

            // TODO scan exits to find target room
            MoveInRooms(pc, command, gamecontext);

        }

        return false;
    }

    private void MoveInWilderness(PlayerCharacter pc, string command, GameContext gamecontext)
    {
        switch (command)
        {

                // TODO check terrain to ensure that target coords are available for movement
            case "n":
                pc.Y -= 1;
                break;
            case "north":
                pc.Y -= 1;
                break;
            case "s":
                pc.Y += 1;
                break;
            case "south":
                pc.Y += 1;
                break;
            case "e":
                pc.X += 1;
                break;
            case "east":
                pc.X += 1;
                break;
            case "w":
                pc.X -= 1;
                break;
            case "west":
                pc.X -= 1;
                break;
            case "ne":
                pc.Y -= 1;
                pc.X += 1;
                break;
            case "northeast":
                pc.Y -= 1;
                pc.X += 1;
                break;
            case "se":
                pc.Y += 1;
                pc.X += 1;
                break;
            case "southeast":
                pc.Y += 1;
                pc.X += 1;
                break;
            case "sw":
                pc.Y += 1;
                pc.X -= 1;
                break;
            case "southwest":
                pc.Y += 1;
                pc.X -= 1;
                break;
            case "nw":
                pc.Y -= 1;
                pc.X -= 1;
                break;
            case "northwest":
                pc.Y -= 1;
                pc.X -= 1;
                break;
            case "enter":
                HandlePortal(pc, gamecontext);
                return;
            case "en":
                HandlePortal(pc, gamecontext);
                return;
            case "in":
                HandlePortal(pc,gamecontext);
                return;
        }

       


        Coordinates coordinates = new Coordinates();
        coordinates.X = pc.X;
        coordinates.Y = pc.Y;
        // wrap player around planet
        coordinates = GameLibrary.Coordinates.Wrap(pc.Planet, coordinates);
        pc.X = coordinates.X;
        pc.Y = coordinates.Y;

        // TODO Check for portals at these coords and display them

        if (pc.Planet.MapTerrain[pc.X, pc.Y].PortalLink != " ")
        {
            pc.SendLine("There is a portal to {0} here.", pc.Planet.MapTerrain[pc.X, pc.Y].LocationMessage);
        }


    }

    public bool HandlePortal(PlayerCharacter pc, GameContext gc)
    {
        if (pc.Planet.MapTerrain[pc.X, pc.Y].PortalLink != " ")
        {
            try
            {
                
                pc.MoveToRoom(pc.Planet.MapTerrain[pc.X, pc.Y].PortalLink, gc);
            }
            catch
            {
                pc.SendLine("&RFailed to retrieve new room");
                return false;
            }

        }
        else
        {
            pc.SendLine("&RError, nothing to enter.");

        }

        return false;

    }

    public void MoveInRooms(PlayerCharacter pc, string command, GameContext gamecontext)
    {
        Exit exit;
        try
        {
            exit = pc.Room.GetExit(command);
        }
        catch 
        {
            pc.SendLine("&RNo such exit.");
            return;
        }
        pc.Room = exit.DestinationRoom;
        Command tempcommand = gamecontext.GetCommand("look");
        tempcommand.DoCommand(pc, gamecontext, "look", null);

    }

}
