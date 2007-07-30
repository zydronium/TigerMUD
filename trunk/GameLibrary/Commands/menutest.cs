using System;
using GameLibrary;


using System.Collections;

/// <summary>
/// Test command
/// </summary>
public class Menutest : GameLibrary.Command
{
    PlayerCharacter pc;
    public Menutest()
    {
        this.Name = "menu";
        this.Words.Add("menu");
        this.Words.Add("menutest");
        this.Syntax = "menu";
    }

    public override bool DoCommand(PlayerCharacter pcin, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        pc = pcin;
        Test(pc);

        return false;
    }

    public void Test(PlayerCharacter pc)
    {

        Menu3 m = new Menu3(pc, "This is the menu Title.", "Here is a description for the menu, perhaps instructions or more details.", "Here is a footer to put more information on.", "Choice: ", "x");
        m.Type = MenuType.MultiChoice;

        m.AddMenuItem(new MenuItem("label", "itemdesc", "a"));
        m.AddMenuItem(new MenuItem("label", "itemdesc", "b"));
        m.AddMenuItem(new MenuItem("label", "itemdesc", "c"));
        m.MenuHandler = new Menu3.MenuChoiceProcessor(ProcessChoices);
        m.Show();


    }



    public void ProcessChoices(object stateinfoin)
    {

        //GameLibrary.StateInfo stateinfo = (GameLibrary.StateInfo)stateinfoin;
        pc.SendLine("You chose:");
        foreach (string choice in pc.Menu.MenuChoices)
        {
            pc.SendLine(choice);
        }
        pc.ConnectionState = ConnectionState.NotWaitingForClientResponse;
        
    }





}
