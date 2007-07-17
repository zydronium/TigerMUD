using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class ChooseCharacterMenu : Menu3
    {
        ArrayList characters;

        public ChooseCharacterMenu()
        {
            gamecontext = GameContext.GetInstance();
        }

        public ChooseCharacterMenu(PlayerCharacter pcin)
        {
            gamecontext = GameContext.GetInstance();
            pc = pcin;

            characters = gamecontext.Database.GetCharacters(pc.AccountId);
            this.Title = "Character Selection";
            this.Description = "Select the character to load.";
            this.Prompt = "Choice: ";
            int i = 1;
            foreach (GameLibrary.CharacterList cl in characters)
            {
                this.AddMenuItem(new MenuItem(cl, cl.NameFirst + " " + cl.NameLast + " (" + cl.Level + " " + cl.CharacterClass + ")\r\n", string.Empty, i.ToString()));
                i++;
            }
            this.AddMenuItem(new MenuItem(new PlayerCharacter(), "Create New Character\r\n", "", "n"));
            pc.Menu = this;
        }

        public override void Exit()
        {

            pc.ConnectionState = GameLibrary.ConnectionState.Disconnected;
        }

        public override void ProcessChoices(object stateinfo)
        {
            ArrayList characterids = this.MenuChoices;
            
            // disco during menu choice
            if (characterids == null ? true : characterids.Count == 0)
            {
                return;
            }
            // If choosing new player, create placeholder character
            if (characterids[0] is PlayerCharacter)
            {
                gamecontext.Database.CreateCharacter(pc.Account, "New", "Character");
                pc.SendLine("New character successfully created.");

                // TODO Go on to new character definition. Name, class, etc.
            }
            else
            {
                // Otherwise, populate current player with the rest of the character data from the db
                pc = gamecontext.Database.LoadCharacter(pc, ((GameLibrary.CharacterList)characterids[0]).Id);
                // Put character on planet
                pc.Planet = gamecontext.GetPlanet(pc.PlanetID);
                Console.WriteLine("Put player {0} on planet {1}", pc.NameDisplay, pc.Planet.NameDisplay);
            }

            // Welcome the character
            pc.SendLine("Welcome {0} {1}!", pc.NameFirst, pc.NameLast);
            pc.ConnectionState = ConnectionState.NotWaitingForClientResponse;
        }

    }
}
