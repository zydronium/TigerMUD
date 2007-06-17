using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class RoomEventArgs : EventArgs
    {
        public readonly PlayerCharacter player;

        public RoomEventArgs(PlayerCharacter player)
        {
            this.player = player;
        }

    }
}
