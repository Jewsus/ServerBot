using System;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Generic;

namespace ServerBot
{
    public class BotTimers
    {
        public Timer swearCountDown;
        public Timer messageTimer;

        public BotTimers()
        {
            swearCountDown = new Timer();
            messageTimer = new Timer();
        }
    }
}
