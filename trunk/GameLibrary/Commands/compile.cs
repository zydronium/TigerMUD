using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class compile : GameLibrary.Command
{
    public compile()
    {
        this.Name = "compile";
        this.Words.Add("compile");
        this.Syntax = "compile";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        string errors = null;
        if (String.IsNullOrEmpty(arguments))
        {
            pc.SendLine("&RSpecify a filename to compile");
            return false;
        }

        try
        {
            errors = gamecontext.Compiler.InitCommands(gamecontext, gamecontext.CommandsFolder + "\\" + arguments);
            if (!String.IsNullOrEmpty(errors)) pc.SendLine("&R" + errors);
        }
        catch (Exception ex)
        {
            pc.SendLine("&R" + ex.Message);
            return false;
        }
        if (String.IsNullOrEmpty(errors))
            pc.SendLine("&GCompiled Successfully.");

        return false;
    }
}
