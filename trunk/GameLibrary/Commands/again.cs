using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Again : GameLibrary.Command
{
    public Again()
    {
        // Code expects this to be command.Name="again". Consider it a special command and don't change or you get infinite loops.
        this.Name = "again";
        this.Words.Add("again");
        this.Words.Add("!");
        this.Syntax = "again";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        if (pc == null) throw new ArgumentNullException("pc");
        if (gamecontext == null) throw new ArgumentNullException("gamecontext");


        try
        {
            arguments = String.Empty;
            string commandword = gamecontext.GetCommandAndParams(pc.MessageLast, ref arguments);
            Command repeatcommand = gamecontext.GetCommand(commandword);
            repeatcommand.DoCommand(pc, gamecontext, commandword, arguments);
        }
        catch
        {}
        return false;
    }
}
