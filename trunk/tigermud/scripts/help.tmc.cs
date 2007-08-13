using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web.Mail;
using System.Text;
using System.IO;
using TigerMUD;

namespace TigerMUD
{
   public class Command_help : Command
	{
		public Command_help()
		{
			name = "command_help";
			words = new string[1] { "help" };
			help.Command = "help";
			help.Summary = "Displays a list of commands.";
			help.Syntax = "help";
			help.Examples = new string[1];
			help.Examples[0] = "help";
		}

		public override bool DoCommand(Actor actor, string command, string arguments)
		{
			if (arguments.Length == 0)
			{
				int counter = 0;
                actor.Send("\r\nSystem Commands:\r\n    ");
				foreach (string callableCommand in Lib.SystemCommandWords.Keys)
				{
					counter++;
					actor.Send(callableCommand + " ");
					if (counter % 6 == 0)
					{
                        actor.Send("\r\n    ");
					}
				}
				if (counter % 6 != 0)
				{
					actor.Send("\r\n");
				}
                actor.Send("\r\nPlayer Commands:\r\n    ");
				counter = 0;
				foreach (string callableCommand in Lib.PlayerCommandWords.Keys)
				{
					counter++;
					actor.Send(callableCommand + " ");
					if (counter % 6 == 0)
					{
                        actor.Send("\r\n    ");
					}
				}
				if (counter % 6 != 0)
				{
					actor.Send("\r\n");
				}

				#region access level based commands

				if (actor["accesslevel"] != null)
				{
					int access = (int)actor["accesslevel"];
					if (access >= (int)AccessLevel.Builder)
					{
                        actor.Send("\r\nBuilder Commands:\r\n    ");
						counter = 0;
						foreach (string callableCommand in Lib.BuilderCommandWords.Keys)
						{
							counter++;
							actor.Send(callableCommand + " ");
							if (counter % 6 == 0)
							{
                                actor.Send("\r\n    ");
							}
						}
					}
					if (counter % 6 != 0)
					{
						actor.Send("\r\n");
					}
					if (access >= (int)AccessLevel.Admin)
					{
                        actor.Send("\r\nAdmin Commands:\r\n    ");
						counter = 0;
						foreach (string callableCommand in Lib.AdminCommandWords.Keys)
						{
							counter++;
							actor.Send(callableCommand + " ");
							if (counter % 6 == 0)
							{
                                actor.Send("\r\n    ");
							}
						}
					}
					if (counter % 6 != 0)
					{
						actor.Send("\r\n");
					}
					if (access >= (int)AccessLevel.UberAdmin)
					{
                        actor.Send("\r\nUber Commands:\r\n    ");
						counter = 0;
						foreach (string callableCommand in Lib.UberAdminCommandWords.Keys)
						{
							counter++;
							actor.Send(callableCommand + " ");
							if (counter % 6 == 0)
							{
                                actor.Send("\r\n    ");
							}
						}
					}
				}

				#endregion

                actor.Send("\r\n");
			}
			else
			{
				Command helpCommand = actor.GetCommandByWord(arguments);
                if (helpCommand != null)
                {
                    actor.Send("Command: " + helpCommand.Help.Command + "\r\n");
                    actor.Send("Description: " + helpCommand.Help.Summary + "\r\n");
                    actor.Send("Syntax: " + helpCommand.Help.Syntax + "\r\n");
                    actor.Send("Examples:\r\n");
                    foreach (string example in helpCommand.Help.Examples)
                    {
                        actor.Send("    " + example + "\r\n");
                    }
                }
                else
                {
                    actor.Send("Unrecognized command: " + arguments);
                }
			}

			return true;
		}
	}
}
