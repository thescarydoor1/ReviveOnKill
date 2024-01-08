using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviveOnKill
{
    class Timer
    {
        private int timeoutValue;
        private int currentTime;

        private const int TIMER_DISABLED = -1;
        private const int TIMER_ENABLED = 0;

        public Timer(int timeoutValue)
        {
            this.timeoutValue = timeoutValue;
            this.currentTime = TIMER_DISABLED;
        }

        public void Stop()
        {
            currentTime = TIMER_DISABLED;
        }

        public void Start()
        {
            if (timeoutValue > 0)
            {
                currentTime = TIMER_ENABLED;
            }
        }

        public bool Active()
        {
            return currentTime != TIMER_DISABLED;
        }

        public int Time()
        {
            return currentTime;
        }

        public void Update()
        {
            if (Active())
            {
                if (currentTime < timeoutValue)
                {
                    currentTime++;
                }
                else
                {
                    currentTime = TIMER_DISABLED;
                }
            }
        }
    }
}
