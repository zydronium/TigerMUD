using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace GameLibrary
{
    /// <summary>
    /// Represents an object that listens to player keystrokes and passes commands along when encountering carriage returns.
    /// </summary>
    public class ClientHandler
    {
        Command c;
        ASCIIEncoding ae = new ASCIIEncoding();

        string commandword;
        string arguments;
        string response = string.Empty;
        PlayerCharacter pc;
        GameContext gamecontext;
        bool disconnect = false;

        DateTime timerstart;
        DateTime timerend;
        TimeSpan timerdifference;


        public ClientHandler()
        {
            gamecontext = GameContext.GetInstance();
            timerstart = new DateTime();
             timerend = new DateTime();
             timerdifference = new TimeSpan();


        }
        public void UsernamePrompt(object stateinfoin)
        {
            
            StateInfo stateinfo = (StateInfo)stateinfoin;

            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");
            string accountid;
            // check for bypass auth setting for dev environment
            if (gamecontext.BypassAuthentication)
            {
                accountid = "{DA151F69-48C5-4798-A870-A5ECA4A7B63A}";
                pcin.Account = gamecontext.Database.LoadAccount(accountid);
                pcin.AccountId = accountid;
                pcin.Authenticated = true;
                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(CharacterSelection));
                pcin.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse;
                return; 

            }

            try
            {
                pcin.SendLine();
                pcin.Send("Enter your username or 'new': ");
                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(HandleUsernameResponse));
                pcin.ConnectionState = GameLibrary.ConnectionState.WaitingforClientResponse;
            }
            catch
            {
                pcin.ConnectionState = ConnectionState.Disconnected;
           
            }

        }

        public void HandleUsernameResponse(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");

            try
            {
                pcin.UsernameResponse = pcin.Message;
                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(PasswordPrompt));
                pcin.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse;
            }
            catch
            {
                pcin.ConnectionState = ConnectionState.Disconnected;
 
            }

        }

        public void PasswordPrompt(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");

            try
            {
                pcin.Send("Enter your password: ");
                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(HandlePasswordResponse));
                pcin.ConnectionState = ConnectionState.WaitingforClientResponse;

            }
            catch
            {
                pcin.ConnectionState = ConnectionState.Disconnected;

            }

        }

        public void HandlePasswordResponse(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");
            pcin.PasswordResponse = pcin.Message;

            string accountid = gamecontext.Database.ValidatePassword(pcin.UsernameResponse, pcin.PasswordResponse);
            if (!String.IsNullOrEmpty(accountid))
            {
                pcin.Account = gamecontext.Database.LoadAccount(accountid);
                pcin.AccountId = accountid;
                pcin.Authenticated = true;
            }
            else
            {
                pcin.SendLine("&RInvalid Login");
                pcin.SetNextCommand(new PlayerCharacter.PlayerDelegate(UsernamePrompt));
                pcin.ConnectionState = ConnectionState.NotWaitingForClientResponse;
            }
            pcin.LoginAttempts++;
            if (pcin.Authenticated)
            {
                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(CharacterSelection));
                pcin.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse;
                return;
            }

            if (pcin.LoginAttempts >= gamecontext.MaxLoginAttempts)
            {
                pcin.SendLine("Too many bad login attempts. Goodbye!");
                Disconnect(pcin);
                pcin.ConnectionState = ConnectionState.Disconnected;
            }

            
        }



        public void CharacterSelection(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            try
            {
                GameLibrary.ChooseCharacterMenu choosecharactermenu = new ChooseCharacterMenu(pcin);
                //pcin.Menu = choosecharactermenu;
                choosecharactermenu.Show();
            }
            catch (Exception ex)
            {
                pcin.Send(ex.Message);
                pcin.ConnectionState = ConnectionState.Disconnected;
            }
            
        }

        public void CommandPrompt(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");
            
            try
            {
                (gamecontext.GetCommand("prompt")).DoCommand(pcin, gamecontext, "prompt", string.Empty);

                pcin.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(CommandResponse));
                pcin.ConnectionState = GameLibrary.ConnectionState.WaitingforClientResponse;
            }
            catch 
            { 
                Disconnect(pcin);
            }
            
        }

        public void CommandResponse(object stateinfoin)
        {
            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            response = pcin.Message;

            if (response == null)
            {
                Disconnect(pcin);
                pcin.ConnectionState = ConnectionState.Disconnected;
            }

            disconnect = ProcessClientMessage(pcin);
            if (disconnect)
            {
                Disconnect(pcin);
                pcin.ConnectionState = ConnectionState.Disconnected;
            }

            // Block command prompt if we're sitting at a menu prompt
            if (pcin.ConnectionState != ConnectionState.WaitingAtMenuPrompt)
            {
                pcin.SetNextCommand(new PlayerCharacter.PlayerDelegate(CommandPrompt));
                pcin.ConnectionState = ConnectionState.NotWaitingForClientResponse;
            }
            

        }

        //Creates random password for user to relogin to the system with...
        public string RandomPasswordGenerator(int length)
        {
            int[] randomPassword = new int[length];
            string generatedPassword = string.Empty;

            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                randomPassword[i] = random.Next(97, 122);
            }
            for (int i = 0; i < length; i++)
            {
                generatedPassword = generatedPassword + (((Char)randomPassword[i]).ToString());
            }
            return generatedPassword;
        }

        public bool ProcessClientMessage(PlayerCharacter pcin)
        {
            if (pcin == null) throw new ArgumentNullException("pcin");

            if (gamecontext.CommandTimer) timerstart = DateTime.Now;

            if (pcin.Message == null) return true;
            if (pcin.Message.Length < 1) return false;
            commandword = gamecontext.GetCommandAndParams(pcin.Message, ref arguments);
            if (commandword == null) return false;
            // Find command in the commands list
            c = gamecontext.GetCommand(commandword);
            if (c == null)
            {
                pcin.SendLine("&RInvalid command.");
                return false;
            }
            //try
            //{
            disconnect = c.DoCommand(pcin, gamecontext, commandword, arguments);
            //}
            //catch (Exception ex)
            //{
            //  Console.WriteLine(ex.Message + ":" + ex.StackTrace);
            // return false;
            //}

            if (disconnect)
            {
                return true;
            }

            if (gamecontext.CommandTimer)
            {
                timerend = DateTime.Now;
                timerdifference = timerend - timerstart;
                pcin.Send("Command processing time was " + timerdifference + "\r\n");
            }

            //pc.Send("Command word was " + commandword);
            //Console.WriteLine("Message from {0}: {1}", pc.Id, pc.Message);

            // remember last message
            // avoid infinite chain of again commands
            if (c.Name != "again")
                pcin.MessageLast = pcin.Message;
            pcin.Message = string.Empty;
            return false;
        }


        public string CreateNewAccount(PlayerCharacter pcin)
        {
            if (pcin == null) throw new ArgumentNullException("pcin");
            string name = string.Empty;
            string email = string.Empty;
            string emailconfirm = string.Empty;
            string password = string.Empty;
            int counter = 0;
            Regex emailregex = new Regex("(?<user>[^@]+)@(?<host>.+)");

            name = pcin.GetResponse("Enter a name for your account: ");
            if (name == null) return null;

            do
            {
                email = pcin.GetResponse("Enter a valid email address for your account (for example nomore@spam.com): ");
                counter++;
                if (email == null) return null;
                // check for valid email address
                if (!gamecontext.Emailregex.IsMatch(email))
                {
                    pcin.SendLine("&RInvalid email address.");
                    continue;
                }
                emailconfirm = pcin.GetResponse("Re-enter the email address to confirm: ");
                if (emailconfirm == null) return null;
                if (email != emailconfirm)
                {
                    pcin.SendLine("&RError, email doesn't match.");
                    continue;
                }
            } while (counter < gamecontext.MaxLoginAttempts && email != emailconfirm);

            if (counter >= gamecontext.MaxLoginAttempts)
            {
                pcin.SendLine("&RToo many bad inputs. Bye!");
                Disconnect(pcin);
                return null;
            }
            // Generate random password
            password = RandomPasswordGenerator(5);

            pcin.NameShort = name;
            pcin.NameFirst = name;

            // Create new account in the database 
            PlayerAccount pa = gamecontext.Database.CreateAccount(name, email, password);

            //Create one placeholder character
            gamecontext.Database.CreateCharacter(pa, "New", "Character");

            pcin.SendLine("Account successfully created.");

            // Send login info email
            gamecontext.Database.SendMail(gamecontext.SmtpServer, gamecontext.SmtpServerUsername, gamecontext.SmtpServerPassword, email, gamecontext.WelcomeEmailFrom, gamecontext.WelcomeEmailBcc, gamecontext.WelcomeEmailSubject, gamecontext.WelcomeEmailBody + "\r\n" + "username=" + name + "\r\n" + "password=" + password);

            pcin.SendLine("Check your email and login using your new username and password.");
            pcin.SendLine();
            StateInfo stateinfo = new StateInfo();
            stateinfo.Pc = pcin;
            stateinfo.GameContext = gamecontext;
            Authenticate(stateinfo);
            if (pcin.Authenticated)
                return pcin.AccountId;
            return null;

        }

        public void Authenticate(object stateinfoin)
        {

            StateInfo stateinfo = (StateInfo)stateinfoin;
            PlayerCharacter pcin = stateinfo.Pc;

            if (pcin == null) throw new ArgumentNullException("pcin");

            


            string username;
            string password;
            string accountid;
            int loginattempts = 0;

            // check for bypass auth setting for dev environment
            if (gamecontext.BypassAuthentication)
            {
                accountid = "{DA151F69-48C5-4798-A870-A5ECA4A7B63A}";
                pcin.Account = gamecontext.Database.LoadAccount(accountid);
                pcin.AccountId = accountid;
                pcin.Authenticated = true;
                return;

            }


            while (!pcin.Authenticated && loginattempts < gamecontext.MaxLoginAttempts)
            {
                pcin.SendLine();
                username = pcin.GetResponse("Enter your username or 'new': ");
                if (username == null)
                {
                    Disconnect(pcin);
                    pcin.ConnectionState = ConnectionState.Disconnected;
                }

                if (String.Compare(username, "new", true) == 0)
                {

                    accountid = CreateNewAccount(pcin);
                    if (accountid == null) Disconnect(pcin);
                    pcin.Authenticated = true;
                    break;
                }
                password = pcin.GetResponse("Enter your password: ");
                if (password == null)
                {
                    Disconnect(pcin);
                    pcin.ConnectionState = ConnectionState.Disconnected;
                }
                accountid = gamecontext.Database.ValidatePassword(username, password);
                if (!String.IsNullOrEmpty(accountid))
                {
                    pcin.Account = gamecontext.Database.LoadAccount(accountid);
                    pcin.AccountId = accountid;
                    pcin.Authenticated = true;
                }
                loginattempts++;
            }
            if (pcin.Authenticated) return;
            pcin.SendLine("Too many bad login attempts. Goodbye!");

            Disconnect(pcin);
            pcin.ConnectionState = ConnectionState.Disconnected;
            //return;
        }

        public void Disconnect(PlayerCharacter pcin)
        {
            if (pcin == null) throw new ArgumentNullException("pcin");

            pcin.Connected = false;
            pcin.ConnectionState = ConnectionState.Disconnected;
            gamecontext.RemovePlayerCharacter(pcin);
            
        }




    }
}
