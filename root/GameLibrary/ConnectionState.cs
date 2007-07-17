using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public enum ConnectionState { NewConnection = 0, WaitingforClientResponse, NotWaitingForClientResponse, NewUser, WaitingAtMenuPrompt, QuestionPrompt, QuestionResponse, CommandPrompt, CommandResponse, Disconnected, ConnectedNotWaitingForClient };
}
