using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameLibrary
{

    public class Scheduler:IDisposable
    {
        object lockobj;
        List<Command> commandschedule = new List<Command>();
        CommandComparer comparer = new CommandComparer();

        public Scheduler()
        {
            lockobj = new object();
        }

           //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Scheduler()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        public void AddToSchedule(Command command)
        {
            lock (lockobj)
            {

                if (command.Count < 1 && !command.RepeatForever)
                {
                    Console.WriteLine("ERROR: scheduled commands must have a count or repeatforever flag.");
                    return;
                }

                if (command.RepeatForever)
                {
                    // requires a delay setting or runonce and die
                    if (command.Delay == TimeSpan.MinValue)
                    {
                        Console.WriteLine("ERROR: Commands that run forever require a Delay TimeSpan.");
                        return;
                    }
                    else
                    {
                        command.NextTrigger = DateTime.Now.Add(command.Delay);
                    }
                    commandschedule.Add(command);
                    commandschedule.Sort(comparer);
                    return;

                }

                if (command.Delay == TimeSpan.MinValue)
                {
                    // We need server to figure out the delay in this case
                    if (command.End == DateTime.MinValue)
                    {
                        Console.WriteLine("ERROR: If you do not specify a delay, then you must specify at least an End DateTime.");
                        return;
                    }
                    if (command.Start > command.End)
                    {
                        Console.WriteLine("ERROR: Start time cannot be greater than end time.");
                        return;
                    }
                    if (command.Start == DateTime.MinValue)
                    {
                        command.Start = DateTime.Now;
                    }

                    TimeSpan duration = command.End - command.Start;
                    int incrementalseconds = (int)duration.TotalSeconds / command.Count;
                    command.Delay = TimeSpan.FromSeconds(incrementalseconds);
                }

                command.NextTrigger = DateTime.Now.Add(command.Delay);
                commandschedule.Add(command);
                commandschedule.Sort(comparer); 
            }
        }

        public bool Remove(Command command)
        {
            lock (lockobj)
            {
                commandschedule.Remove(command); 
            }
            return true;
        }

        public Command Pop()
        {
            Command command;
            lock (lockobj)
            {
                 command = commandschedule[0];
                commandschedule.Remove(command); 
            }
            return command;
        }

        public Command First
        {
            get
            {
                if (commandschedule.Count > 0) return commandschedule[0];
                else return null;
            }
        }

        public int Count
        {
            get
            {
                return commandschedule.Count;
            }
        }

        public string Display()
        {
            string list;
            lock (lockobj)
            {
                 list = string.Empty;
                foreach (Command command in commandschedule)
                {
                    list += command.Name + "\r\n";
                } 
            }
            return list;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RunScheduledCommands(Object threadstartupin)
        {
            ThreadInitializationData threadstartup = (ThreadInitializationData)threadstartupin;
            for (; ; )
            {
                // Nothing to run? Sleep 1/10 second.
                while (this.Count < 1) Thread.Sleep(100);
                // Not time to run the next command yet? Sleep 1/10 second.

                while (this.First.NextTrigger > DateTime.Now || this.First.Start > DateTime.Now) Thread.Sleep(100);

                // Time to run the command
                Command command = this.Pop();
                if (command == null)
                {
                    continue;
                }
                try
                {
                    command.DoCommand(null, threadstartup.GameContext, null, null);
                }
                catch (ThreadAbortException)
                {
                    //Server shutdown
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + " EXCEPTION running command " + command.Name + ": " + ex.Message + ex.StackTrace);
                    this.Remove(command);
                    return;
                }
                if (command.RepeatForever)
                {
                    this.AddToSchedule(command);
                    continue;
                }
                command.Count--;
                //Lib.Delayedcommands.Add(dc.actor, dc.command, dc.arguments, Lib.GetTime(), dc.delay, dc.loop);
                if (command.Count > 0)
                {
                    this.AddToSchedule(command);
                    continue;
                }
            }
        }
    }

    public class CommandComparer : IComparer<Command>
    {
        CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();

        int result;

        int IComparer<Command>.Compare(Command command1, Command command2)
        {
            result = comparer.Compare(command1.Delay, command2.Delay);
            return result;
        }


    }



}
