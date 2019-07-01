using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Engine.Utility
{
    public static class RandomGenerator
    {
        public static int IntBetween(int lowerInclusive, int upperExclusive)
        {
            Random randomGenerator = new Random(Guid.NewGuid().GetHashCode());

            return randomGenerator.Next(lowerInclusive, upperExclusive);
        }

        public static double GetDoubleFromZeroToOne()
        {
            Random randomGenerator = new Random(Guid.NewGuid().GetHashCode());

            return randomGenerator.NextDouble();
        }
    }
}
