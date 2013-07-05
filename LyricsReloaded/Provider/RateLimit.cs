using System;

namespace CubeIsland.LyricsReloaded.Provider
{
    public class RateLimit
    {
        private long periodStart;
        private readonly long periodLength;
        private readonly int requestsPerPeriod;
        private volatile int currentRequests;

        public RateLimit(long periodLength, int requestsPerPeriod)
        {
            reset();
            this.periodLength = periodLength;
            this.requestsPerPeriod = requestsPerPeriod;
        }

        private void reset()
        {
            periodStart = -1;
            currentRequests = 0;
        }

        public bool tryIncrement()
        {
            if ((Environment.TickCount - periodStart) > periodLength)
            {
                reset();
            }
            if (currentRequests > requestsPerPeriod)
            {
                return false;
            }

            if (periodStart == -1)
            {
                periodStart = Environment.TickCount;
            }
            ++currentRequests;
            return true;
        }

        public static RateLimit parse(string input)
        {
            string[] parts = input.Split('/');

            int requestsPerPeriod;
            try
            {
                requestsPerPeriod = Convert.ToInt32(parts[0].Trim());
            }
            catch (FormatException)
            {
                return null;
            }
            long periodLength = 1000 * 60 * 60; // 1 hour

            if (parts.Length > 1)
            {
                switch (parts[1].Trim().ToLower())
                {
                    case "second":
                        periodLength = 1000L;
                        break;
                    case "minute":
                        periodLength = 1000L * 60;
                        break;
                    case "hour":
                        periodLength = 1000L * 60 * 60;
                        break;
                    case "day":
                        periodLength = 1000L * 60 * 60 * 24;
                        break;
                    case "week":
                        periodLength = 1000L * 60 * 60 * 24 * 7;
                        break;
                    case "month":
                        periodLength = 1000L * 60 * 60 * 24 * 30;
                        break;
                }
            }

            return new RateLimit(periodLength, requestsPerPeriod);
        }

        public void shutdown()
        {
            // TODO implement period start persistence
        }
    }
}